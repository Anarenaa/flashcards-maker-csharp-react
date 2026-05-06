using Core.DTOs.Practice;
using Core.Models;
using Repositories.Interfaces;
using Services.Practice;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random = new();

    public SessionService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode)
    {
        var cardProgresses = await _unitOfWork.Practice.GetNewBatchForPracticeAsync(setId, userId, 20);

        if (cardProgresses == null || !cardProgresses.Any())
        {
            // Це захисний блок: якщо прогресу немає, але картки в сеті є, беремо їх напряму
            var rawCards = await _unitOfWork.Flashcards.GetAllAsync(f => f.SetId == setId);
            if (!rawCards.Any()) return null; // Сет реально порожній

            // Тимчасово створюємо об'єкти прогресу в пам'яті для відображення
            cardProgresses = rawCards.Select(f => new CardProgress
            {
                Flashcard = f,
                FlashcardId = f.Id,
                UserId = userId,
                Progress = 0
            }).ToList();
        }

        // Визначаємо режим
        var sessionMode = requestedMode ?? DetermineMode(cardProgresses.Min(cp => cp.Progress));

        // Підготовка сесії залежно від режиму
        return sessionMode switch
        {
            PracticeActivityType.Review => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Review, takeOne: false), // змінено на false щоб бачити всі карти
            PracticeActivityType.Quiz => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Quiz, needsDistractors: true),
            PracticeActivityType.Matching => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Matching, useBatching: true),
            PracticeActivityType.Writing => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Writing),
            PracticeActivityType.Context => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Context),
            PracticeActivityType.Mixed => await GetMixedSessionAsync(setId, cardProgresses),
            _ => throw new ArgumentException($"Unsupported activity type: {sessionMode}")
        };
    }

    private async Task<PracticeSessionDTO> PrepareSessionAsync(
        int setId,
        List<CardProgress> source,
        PracticeActivityType type,
        bool needsDistractors = false,
        bool useBatching = false,
        bool takeOne = false)
    {
        // Вибираємо картки
        var selected = takeOne ? source.Take(1).ToList() : source;
        int batchSize = useBatching ? CalculateOptimalBatchSize(selected.Count) : selected.Count;

        // Отримуємо варіанти відповідей (дистрактори)
        Dictionary<int, List<string>> distractorsMap = null;
        if (needsDistractors)
        {
            distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, selected.Select(cp => cp.FlashcardId).ToList(), 3);
        }

        var dto = CreateBaseDto(setId, batchSize, type);
        foreach (var cp in selected)
        {
            var card = MapToCardDto(cp, type);

            // Додаємо дистрактори, якщо це Quiz
            if (needsDistractors && distractorsMap != null && distractorsMap.TryGetValue(card.Id, out var d))
            {
                card.Distractors = ShuffleDistractors(d, card.Definition);
            }
            else if (needsDistractors)
            {
                // Якщо дистракторів немає в базі (мало карток), створюємо порожній список
                card.Distractors = new List<string> { card.Definition };
            }

            dto.Flashcards.Add(card);
        }
        return dto;
    }

    // Решта методів (Mixed, Helpers) залишаються без змін
    private async Task<PracticeSessionDTO> GetMixedSessionAsync(int setId, List<CardProgress> source)
    {
        var (selected, batchSize) = ExtractBatch(source);
        var assignments = selected.Select(cp => new { Data = cp, Type = (PracticeActivityType)_random.Next(1, 5) }).ToList(); // 5 бо 6 - це Mixed

        var quizIds = assignments.Where(a => a.Type == PracticeActivityType.Quiz).Select(a => a.Data.FlashcardId).ToList();
        var distractorsMap = quizIds.Any() ? await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, quizIds, 3) : null;

        var dto = CreateBaseDto(setId, batchSize, PracticeActivityType.Mixed);
        foreach (var item in assignments)
        {
            var card = MapToCardDto(item.Data, item.Type);
            if (distractorsMap != null && distractorsMap.TryGetValue(card.Id, out var d))
                card.Distractors = ShuffleDistractors(d, card.Definition);

            dto.Flashcards.Add(card);
        }
        return dto;
    }

    private (List<CardProgress> Selected, int BatchSize) ExtractBatch(List<CardProgress> source)
    {
        int size = CalculateOptimalBatchSize(source.Count);
        return (source.Take(source.Count).ToList(), size);
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

    private FlashcardPracticeDTO MapToCardDto(CardProgress cp, PracticeActivityType type) => new()
    {
        Id = cp.FlashcardId,
        Term = cp.Flashcard.Term,
        Definition = cp.Flashcard.Definition,
        CardType = type
    };

    private int CalculateOptimalBatchSize(int total) => total > 0 ? (total <= 5 ? total : 5) : 0;

    private PracticeActivityType DetermineMode(float progress) => progress switch
    {
        < 0.15f => PracticeActivityType.Review,
        < 0.35f => PracticeActivityType.Quiz,
        < 0.55f => PracticeActivityType.Matching,
        < 0.75f => PracticeActivityType.Writing,
        < 0.90f => PracticeActivityType.Context,
        _ => PracticeActivityType.Mixed
    };
}