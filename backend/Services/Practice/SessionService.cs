using Core.DTOs;
using Core.DTOs.Practice;
using Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Repositories.Interfaces;
using Services.Practice.Interfaces;

namespace Services.Practice
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new();

        public SessionService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<PracticeSessionDTO?> GetPracticeSessionAsync(List<FlashcardDTO> flashcards, int setId, int userId, PracticeActivityType? requestedMode, bool isReversed)
        {
            var flashcardMap = flashcards.Where(f => f.Id.HasValue).ToDictionary(f => f.Id!.Value);
            if (!flashcardMap.Any()) return null;

            // creates records for the flashcards in the CardProgresses table if they don't exist yet
            await _unitOfWork.Practice.GetNewBatchForPracticeAsync(flashcards, userId);

            string flashcardsKeyPart = string.Join("_", flashcards.OrderBy(f => f.Id).Select(f => f.Id));
            string sessionCardsKey = $"session_cards_ids_{userId}_{setId}_{isReversed}_{flashcardsKeyPart}";

            if (!_cache.TryGetValue(sessionCardsKey, out List<int> cardIds))
            {
                cardIds = flashcardMap.Keys.ToList();
                _cache.Set(sessionCardsKey, cardIds, TimeSpan.FromMinutes(30));
            }

            // dynamically fetch the progress for the batch of cards for the user
            var progressMap = await _unitOfWork.Practice.GetBatchCardProgressMapAsync(userId, cardIds);

            var cardProgresses = cardIds.Select(id => new CardProgress
            {
                FlashcardId = id,
                UserId = userId,
                Progress = progressMap.ContainsKey(id) ? progressMap[id] : 0f
            }).ToList();

            var sessionMode = requestedMode ?? determineMode(cardProgresses.Min(cp => cp.Progress));

            return sessionMode switch
            {
                PracticeActivityType.Review => await prepareSessionAsync(setId, cardProgresses, flashcardMap, PracticeActivityType.Review, isReversed: isReversed),
                PracticeActivityType.Quiz => await prepareSessionAsync(setId, cardProgresses, flashcardMap, PracticeActivityType.Quiz, needsDistractors: true, isReversed: isReversed),
                PracticeActivityType.Matching => await prepareSessionAsync(setId, cardProgresses, flashcardMap, PracticeActivityType.Matching, isReversed: isReversed),
                PracticeActivityType.Writing => await prepareSessionAsync(setId, cardProgresses, flashcardMap, PracticeActivityType.Writing, isReversed: isReversed),
                PracticeActivityType.Mixed => await getMixedSessionAsync(setId, cardProgresses, flashcardMap, isReversed),
                _ => throw new ArgumentException($"Unsupported activity type: {sessionMode}")
            };
        }
        private async Task<PracticeSessionDTO> prepareSessionAsync(
            int setId, 
            List<CardProgress> source, 
            Dictionary<int, FlashcardDTO> flashcardMap,
            PracticeActivityType type,
            bool needsDistractors = false,
            bool isReversed = false)
        {
            Dictionary<int, List<string>> distractorsMap = null;
            if (needsDistractors)
            {
                distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, source.Select(cp => cp.FlashcardId).ToList(), 3, isReversed);
            }

            var dto = createBaseDto(type);
            foreach (var cp in source)
            {
                var card = mapToCardDto(cp, flashcardMap, type, isReversed);
                if (needsDistractors && distractorsMap != null && distractorsMap.TryGetValue(card.Id, out var d))
                {
                    string correctAnswer = card.Definition;
                    card.Options = buildQuizOptions(d, correctAnswer);
                }

                dto.Flashcards.Add(card);
            }

            return dto;
        }

        private async Task<PracticeSessionDTO> getMixedSessionAsync(
            int setId, 
            List<CardProgress> source, 
            Dictionary<int, FlashcardDTO> flashcardMap, 
            bool isReversed = false)
        {
            // Create a list of modes alternating between Quiz and Writing (randomly shuffled later)
            var modes = new List<PracticeActivityType>();
            bool toggle = true;

            foreach (var _ in source)
            {
                modes.Add(toggle ? PracticeActivityType.Quiz : PracticeActivityType.Writing);
                toggle = !toggle;
            }
            var assignedTypes = modes.OrderBy(_ => _random.Next()).ToList();

            // Create the DTO and populate it with flashcards of the assigned types
            var dto = createBaseDto(PracticeActivityType.Mixed);

            for (int i = 0; i < source.Count; i++)
            {
                var card = mapToCardDto(source[i], flashcardMap, assignedTypes[i], isReversed);
                dto.Flashcards.Add(card);
            }

            // If there are any Quiz cards, fetch their distractors and assign them
            var quizIds = dto.Flashcards.Where(c => c.CardActivityType == PracticeActivityType.Quiz).Select(c => c.Id).ToList();
            if (quizIds.Any())
            {
                var distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, quizIds, 3, isReversed);
                foreach (var card in dto.Flashcards.Where(c => c.CardActivityType == PracticeActivityType.Quiz))
                {
                    if (distractorsMap.TryGetValue(card.Id, out var d))
                    {
                        string correctAnswer = card.Definition;
                        card.Options = buildQuizOptions(d, correctAnswer);
                    }
                }
            }

            return dto;
        }

        private List<string> buildQuizOptions(List<string> distractors, string correct)
        {
            var options = distractors.ToList();
            options.Add(correct);

            return options.OrderBy(_ => _random.Next()).ToList();
        }

        private PracticeSessionDTO createBaseDto(PracticeActivityType type) => new()
        {
            SelectedActivity = type,
            Flashcards = new()
        };

        private FlashcardPracticeDTO mapToCardDto(CardProgress cp, Dictionary<int, FlashcardDTO> flashcardMap, PracticeActivityType type, bool isReversed)
        {
            var flashcard = flashcardMap[cp.FlashcardId];
            return new FlashcardPracticeDTO
            {
                Id = cp.FlashcardId,
                Term = isReversed ? flashcard.Definition : flashcard.Term,
                Definition = isReversed ? flashcard.Term : flashcard.Definition,
                CardActivityType = type
            };
        }

        private PracticeActivityType determineMode(float progress) => progress switch
        {
            < PracticeActivityLimit.ReviewLimit => PracticeActivityType.Review,
            < PracticeActivityLimit.QuizLimit => PracticeActivityType.Quiz,
            < PracticeActivityLimit.MatchingLimit => PracticeActivityType.Matching,
            < PracticeActivityLimit.WritingLimit => PracticeActivityType.Writing,
            _ => PracticeActivityType.Mixed
        };
    }
}