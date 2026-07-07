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

    public async Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode)
    {
        string sessionCardsKey = $"session_cards_{userId}_{setId}";

        if (!_cache.TryGetValue(sessionCardsKey, out List<CardProgress> cardProgresses))
        {
            cardProgresses = await _unitOfWork.Practice.GetNewBatchForPracticeAsync(setId, userId, 20);

            if (cardProgresses == null || !cardProgresses.Any())
            {
                var rawCards = await _unitOfWork.Flashcards.GetAllAsync(filter: f => f.SetId == setId);
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
            PracticeActivityType.Mixed => await GetMixedSessionAsync(setId, cardProgresses, userId),
            _ => throw new ArgumentException($"Unsupported activity type: {sessionMode}")
        };
    }

    private async Task<PracticeSessionDTO> PrepareSessionAsync(
        int setId, List<CardProgress> source, PracticeActivityType type,
        bool needsDistractors = false, bool useBatching = false)
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

        return dto;
    }

    private async Task<PracticeSessionDTO> GetMixedSessionAsync(int setId, List<CardProgress> source, int userId)
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

        return dto;
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
        return new FlashcardPracticeDTO
        {
            Id = cp.FlashcardId,
            Term = cp.Flashcard.Term,
            Definition = cp.Flashcard.Definition,
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
        _ => PracticeActivityType.Mixed
    };
}