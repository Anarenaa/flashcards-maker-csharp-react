using Core.DTOs.Practice;
using Repositories.Interfaces;

namespace Services
{
    public class PracticeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _random = new();

        public PracticeService(IUnitOfWork practiceRepo) => _unitOfWork = practiceRepo;

        public async Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode)
        {
            var cardProgresses = await _unitOfWork.Practice.GetNewBatchForPracticeAsync(setId, userId, 20);
            if (!cardProgresses.Any()) return null;

            // Розраховуємо динамічний BatchSize
            int batchSize = CalculateOptimalBatchSize(cardProgresses.Count);
            int totalToTake = (cardProgresses.Count / batchSize) * batchSize;
            var selectedData = cardProgresses.Take(totalToTake).ToList();

            var minProgress = selectedData.Min(cp => cp.Progress);
            var sessionMode = requestedMode ?? DetermineMode(minProgress);

            var dto = new PracticeSessionDTO
            {
                SetId = setId,
                SetTitle = selectedData.First().Flashcard.Set.Name,
                BatchSize = batchSize,
                SelectedActivity = sessionMode
            };

            foreach (var cp in selectedData)
            {
                var currentType = (sessionMode == PracticeActivityType.Mixed)
                    ? (PracticeActivityType)_random.Next(1, 5)
                    : sessionMode;

                var cardDto = new FlashcardPracticeDTO
                {
                    Id = cp.FlashcardId,
                    Term = cp.Flashcard.Term,
                    Definition = cp.Flashcard.Definition,
                    CardType = currentType
                };

                if (currentType == PracticeActivityType.Quiz)
                {
                    var distractors = await _unitOfWork.Practice.GetDistractorsAsync(setId, cp.FlashcardId, 3);
                    distractors.Add(cardDto.Definition);
                    cardDto.Distractors = distractors.OrderBy(x => _random.Next()).ToList();
                }

                dto.Flashcards.Add(cardDto);
            }

            return dto;
        }

        private int CalculateOptimalBatchSize(int total)
        {
            if (total <= 5) return total;
            if (total % 5 == 0) return 5;
            if (total % 4 == 0) return 4;
            if (total % 3 == 0) return 3;
            return 4;
        }

        private PracticeActivityType DetermineMode(float progress) => progress switch
        {
            < 0.2f => PracticeActivityType.Quiz,
            < 0.4f => PracticeActivityType.Matching,
            < 0.6f => PracticeActivityType.Writing,
            < 0.8f => PracticeActivityType.Context,
            _ => PracticeActivityType.Mixed
        };
    }
}
