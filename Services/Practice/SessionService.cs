using Core.DTOs.Practice;
using Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Practice;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeminiService _geminiService;
    private readonly IMemoryCache _cache;
    private readonly Random _random = new();

    public SessionService(IUnitOfWork unitOfWork, IGeminiService geminiService, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _geminiService = geminiService;
        _cache = cache;
    }

    public async Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode, int currentIndex = 0)
    {
        string sessionCardsKey = $"session_cards_{userId}_{setId}";

        if (!_cache.TryGetValue(sessionCardsKey, out List<CardProgress> cardProgresses))
        {
            cardProgresses = await _unitOfWork.Practice.GetNewBatchForPracticeAsync(setId, userId, 20);

            if (cardProgresses == null || !cardProgresses.Any())
            {
                var rawCards = await _unitOfWork.Flashcards.GetAllAsync(f => f.SetId == setId);
                if (!rawCards.Any()) return null;

                cardProgresses = rawCards.Select(f => new CardProgress
                {
                    Flashcard = f,
                    FlashcardId = f.Id,
                    UserId = userId,
                    Progress = 0
                }).ToList();
            }

            // Важливо: фіксуємо порядок карток один раз для всієї сесії
            cardProgresses = cardProgresses.OrderBy(_ => _random.Next()).ToList();

            _cache.Set(sessionCardsKey, cardProgresses, TimeSpan.FromMinutes(30));
        }

        var sessionMode = requestedMode ?? DetermineMode(cardProgresses.Min(cp => cp.Progress));

        return sessionMode switch
        {
            PracticeActivityType.Review => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Review),
            PracticeActivityType.Quiz => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Quiz, needsDistractors: true),
            PracticeActivityType.Matching => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Matching, useBatching: true),
            PracticeActivityType.Writing => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Writing),
            PracticeActivityType.Context => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Context, currentIndex: currentIndex),
            PracticeActivityType.Mixed => await GetMixedSessionAsync(setId, cardProgresses, currentIndex, userId),
            _ => throw new ArgumentException($"Unsupported activity type: {sessionMode}")
        };
    }

    private async Task<PracticeSessionDTO> PrepareSessionAsync(
        int setId, List<CardProgress> source, PracticeActivityType type,
        bool needsDistractors = false, bool useBatching = false, int currentIndex = 0)
    {
        int batchSize = useBatching ? CalculateOptimalBatchSize(source.Count) : source.Count;

        Dictionary<int, List<string>> distractorsMap = null;
        if (needsDistractors)
        {
            distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, source.Select(cp => cp.FlashcardId).ToList(), 3);
        }

        var dto = CreateBaseDto(setId, batchSize, type);
        foreach (var cp in source)
        {
            var card = MapToCardDto(cp, type);
            if (needsDistractors && distractorsMap != null && distractorsMap.TryGetValue(card.Id, out var d))
                card.Distractors = ShuffleDistractors(d, card.Definition);

            dto.Flashcards.Add(card);
        }

        await AddAiContextAsync(dto, currentIndex, type.ToString().ToLower());
        return dto;
    }

    private async Task<PracticeSessionDTO> GetMixedSessionAsync(int setId, List<CardProgress> source, int currentIndex, int userId)
    {
        // Ключ для кешування розподілу типів у Mixed режимі
        string mixedTypesKey = $"mixed_types_{userId}_{setId}";
        if (!_cache.TryGetValue(mixedTypesKey, out List<PracticeActivityType> assignedTypes))
        {
            int[] allowedModes = { 2, 4 };
            assignedTypes = source.Select(_ => (PracticeActivityType)allowedModes[_random.Next(allowedModes.Length)]).ToList();
            _cache.Set(mixedTypesKey, assignedTypes, TimeSpan.FromMinutes(30));
        }

        var dto = CreateBaseDto(setId, source.Count, PracticeActivityType.Mixed);

        for (int i = 0; i < source.Count; i++)
        {
            var card = MapToCardDto(source[i], assignedTypes[i]);
            dto.Flashcards.Add(card);
        }

        var quizIds = dto.Flashcards.Where(c => c.CardType == PracticeActivityType.Quiz).Select(c => c.Id).ToList();
        if (quizIds.Any())
        {
            var distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, quizIds, 3);
            foreach (var card in dto.Flashcards.Where(c => c.CardType == PracticeActivityType.Quiz))
            {
                if (distractorsMap.TryGetValue(card.Id, out var d))
                    card.Distractors = ShuffleDistractors(d, card.Definition);
            }
        }

        await AddAiContextAsync(dto, currentIndex, "mixed");
        return dto;
    }

    private async Task AddAiContextAsync(PracticeSessionDTO dto, int currentIndex, string modeSuffix)
    {
        if (currentIndex < 0 || currentIndex >= dto.Flashcards.Count) return;

        var currentCard = dto.Flashcards[currentIndex];

        if (currentCard.CardType == PracticeActivityType.Context || dto.SelectedActivity == PracticeActivityType.Context)
        {
            string cacheKey = $"context_card_{currentCard.Id}_{modeSuffix}";

            if (!_cache.TryGetValue(cacheKey, out ContextGameDto cachedGame))
            {
                cachedGame = await _geminiService.GenerateContextSentenceAsync(currentCard.Term, currentCard.Definition);

                if (cachedGame != null && !string.IsNullOrEmpty(cachedGame.Sentence))
                {
                    _cache.Set(cacheKey, cachedGame, TimeSpan.FromDays(1));
                }
            }

            if (cachedGame != null && !string.IsNullOrEmpty(cachedGame.Sentence))
            {
                currentCard.ContextSentence = cachedGame.Sentence;
                currentCard.ContextHint = (cachedGame.CorrectAnswer == currentCard.Term) ? currentCard.Definition : currentCard.Term;
                currentCard.Term = cachedGame.CorrectAnswer;
            }
        }
    }

    private List<string> ShuffleDistractors(List<string> distractors, string correct)
    {
        var list = distractors.ToList();
        if (!list.Contains(correct)) list.Add(correct);
        return list.OrderBy(_ => _random.Next()).ToList();
    }

    private PracticeSessionDTO CreateBaseDto(int setId, int batchSize, PracticeActivityType type) => new()
    {
        SetId = setId,
        BatchSize = batchSize,
        SelectedActivity = type,
        Flashcards = new()
    };

    private FlashcardPracticeDTO MapToCardDto(CardProgress cp, PracticeActivityType type)
    {
        var term = cp.Flashcard.Term;
        var definition = cp.Flashcard.Definition;

        if (type == PracticeActivityType.Writing)
        {
            int termWordsCount = term.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (termWordsCount > 3)
            {
                var temp = term;
                term = definition;
                definition = temp;
            }
        }

        return new FlashcardPracticeDTO
        {
            Id = cp.FlashcardId,
            Term = term,
            Definition = definition,
            CardType = type
        };
    }

    private int CalculateOptimalBatchSize(int total) => total > 0 ? (total <= 5 ? total : 5) : 0;

    private PracticeActivityType DetermineMode(float progress) => progress switch
    {
        < PracticeActivityLimit.ReviewLimit => PracticeActivityType.Review,
        < PracticeActivityLimit.QuizLimit => PracticeActivityType.Quiz,
        < PracticeActivityLimit.MatchingLimit => PracticeActivityType.Matching,
        < PracticeActivityLimit.WritingLimit => PracticeActivityType.Writing,
        //< PracticeActivityLimit.ContextLimit => PracticeActivityType.Context,
        _ => PracticeActivityType.Mixed
    };
}