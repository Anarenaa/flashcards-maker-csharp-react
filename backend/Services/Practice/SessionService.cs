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

        public async Task<PracticeSessionDTO> GetPracticeSessionAsync(List<FlashcardDTO> flashcards, int setId, int userId, PracticeActivityType? requestedMode, bool isReversed)
        {
            string flashcardsKeyPart = string.Join("_", flashcards.OrderBy(f => f.Id).Select(f => f.Id));
            string sessionCardsKey = $"session_cards_{userId}_{setId}_{flashcardsKeyPart}";

            if (!_cache.TryGetValue(sessionCardsKey, out List<CardProgress> cardProgresses))
            {
                cardProgresses = await _unitOfWork.Practice.GetNewBatchForPracticeAsync(flashcards, userId);

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

                _cache.Set(sessionCardsKey, cardProgresses, TimeSpan.FromMinutes(30));
            }

            var sessionMode = requestedMode ?? determineMode(cardProgresses.Min(cp => cp.Progress));

            return sessionMode switch
            {
                PracticeActivityType.Review => await prepareSessionAsync(setId, cardProgresses, PracticeActivityType.Review, isReversed: isReversed),
                PracticeActivityType.Quiz => await prepareSessionAsync(setId, cardProgresses, PracticeActivityType.Quiz, needsDistractors: true, isReversed: isReversed),
                PracticeActivityType.Matching => await prepareSessionAsync(setId, cardProgresses, PracticeActivityType.Matching, isReversed: isReversed),
                PracticeActivityType.Writing => await prepareSessionAsync(setId, cardProgresses, PracticeActivityType.Writing, isReversed: isReversed),
                PracticeActivityType.Mixed => await getMixedSessionAsync(setId, cardProgresses, isReversed),
                _ => throw new ArgumentException($"Unsupported activity type: {sessionMode}")
            };
        }

        private async Task<PracticeSessionDTO> prepareSessionAsync(
            int setId, List<CardProgress> source, PracticeActivityType type,
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
                var card = mapToCardDto(cp, type);
                if (needsDistractors && distractorsMap != null && distractorsMap.TryGetValue(card.Id, out var d))
                {
                    string correctAnswer = isReversed ? card.Term : card.Definition;
                    card.Distractors = shuffleDistractors(d, correctAnswer);
                }

                dto.Flashcards.Add(card);
            }

            return dto;
        }

        private async Task<PracticeSessionDTO> getMixedSessionAsync(int setId, List<CardProgress> source, bool isReversed = false)
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
                var card = mapToCardDto(source[i], assignedTypes[i]);
                dto.Flashcards.Add(card);
            }

            // If there are any Quiz cards, fetch their distractors and assign them
            var quizIds = dto.Flashcards.Where(c => c.CardType == PracticeActivityType.Quiz).Select(c => c.Id).ToList();
            if (quizIds.Any())
            {
                var distractorsMap = await _unitOfWork.Practice.GetBatchDistractorsAsync(setId, quizIds, 3, isReversed);
                foreach (var card in dto.Flashcards.Where(c => c.CardType == PracticeActivityType.Quiz))
                {
                    if (distractorsMap.TryGetValue(card.Id, out var d))
                    {
                        string correctAnswer = isReversed ? card.Term : card.Definition;
                        card.Distractors = shuffleDistractors(d, correctAnswer);
                    }
                }
            }

            return dto;
        }

        private List<string> shuffleDistractors(List<string> distractors, string correct)
        {
            var list = distractors.ToList();
            if (!list.Contains(correct)) list.Add(correct);
            return list.OrderBy(_ => _random.Next()).ToList();
        }

        private PracticeSessionDTO createBaseDto(PracticeActivityType type) => new()
        {
            SelectedActivity = type,
            Flashcards = new()
        };

        private FlashcardPracticeDTO mapToCardDto(CardProgress cp, PracticeActivityType type)
        {
            return new FlashcardPracticeDTO
            {
                Id = cp.FlashcardId,
                Term = cp.Flashcard.Term,
                Definition = cp.Flashcard.Definition,
                CardType = type
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