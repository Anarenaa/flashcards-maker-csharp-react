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
        if (cardProgresses == null || !cardProgresses.Any()) return null;

        var sessionMode = requestedMode ?? DetermineMode(cardProgresses.Min(cp => cp.Progress));

        return sessionMode switch
        {
            PracticeActivityType.Review => await PrepareSessionAsync(setId, cardProgresses, PracticeActivityType.Review, takeOne: true),
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
        var (selected, batchSize) = useBatching ? ExtractBatch(source) : (takeOne ? (source.Take(1).ToList(), 1) : (source, source.Count));

        Dictionary<int, List<string>> distractorsMap = null;
        if (needsDistractors)
        {
            distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, selected.Select(cp => cp.FlashcardId).ToList(), 3);
        }

        var dto = CreateBaseDto(setId, batchSize, type);
        foreach (var cp in selected)
        {
            var card = MapToCardDto(cp, type);
            if (distractorsMap != null && distractorsMap.TryGetValue(card.Id, out var d))
                card.Distractors = ShuffleDistractors(d, card.Definition);

            dto.Flashcards.Add(card);
        }
        return dto;
    }

    private async Task<PracticeSessionDTO> GetMixedSessionAsync(int setId, List<CardProgress> source)
    {
        var (selected, batchSize) = ExtractBatch(source);
        var assignments = selected.Select(cp => new { Data = cp, Type = (PracticeActivityType)_random.Next(1, 6) }).ToList();

        var quizIds = assignments.Where(a => a.Type == PracticeActivityType.Quiz || a.Type == PracticeActivityType.Context)
                                 .Select(a => a.Data.FlashcardId).ToList();

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

    // --- Helpers ---

    private (List<CardProgress> Selected, int BatchSize) ExtractBatch(List<CardProgress> source)
    {
        int size = CalculateOptimalBatchSize(source.Count);
        int take = (source.Count / size) * size;
        return (source.Take(take).ToList(), size);
    }

    private List<string> ShuffleDistractors(List<string> distractors, string correct)
    {
        distractors.Add(correct);
        return distractors.OrderBy(_ => _random.Next()).ToList();
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

    private int CalculateOptimalBatchSize(int total) => total switch
    {
        <= 5 => total,
        _ when total % 5 == 0 => 5,
        _ when total % 4 == 0 => 4,
        _ when total % 3 == 0 => 3,
        _ => 4
    };

    private PracticeActivityType DetermineMode(float progress) => progress switch
    {
        < 0.10f => PracticeActivityType.Review,
        < 0.30f => PracticeActivityType.Quiz,
        < 0.50f => PracticeActivityType.Matching,
        < 0.70f => PracticeActivityType.Writing,
        < 0.90f => PracticeActivityType.Context,
        _ => PracticeActivityType.Mixed
    };
}