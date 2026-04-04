using Core.DTOs.Practice;
using Repositories.Interfaces;

namespace Services.Practice
{
    public class AnswerService : IAnswerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnswerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType)
        {
            // Отримуємо правильну відповідь з флешкартки
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(flashcardId);
            if (flashcard == null) return false;

            var correctAnswer = flashcard.Definition;
            
            if (string.IsNullOrEmpty(userAnswer) || string.IsNullOrEmpty(correctAnswer))
                return false;

            switch (activityType)
            {
                case PracticeActivityType.Review:
                    // Для Review - просто показуємо відповідь, завжди "правильно"
                    return true;
                
                case PracticeActivityType.Quiz:
                case PracticeActivityType.Matching:
                    // Для Quiz та Matching - точна відповідь
                    return userAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                
                case PracticeActivityType.Writing:
                case PracticeActivityType.Context:
                    // Для Writing та Context - гнучка перевірка
                    return CheckFlexibleAnswer(userAnswer, correctAnswer);
                
                case PracticeActivityType.Mixed:
                    // Для Mixed - гнучка перевірка (найскладніший)
                    return CheckFlexibleAnswer(userAnswer, correctAnswer);
                
                default:
                    return userAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool CheckFlexibleAnswer(string userAnswer, string correctAnswer)
        {
            var userClean = userAnswer.Trim().ToLowerInvariant();
            var correctClean = correctAnswer.Trim().ToLowerInvariant();
            
            // Точна відповідь
            if (userClean == correctClean)
                return true;
            
            // Ігноруємо артиклі (a/an/the)
            var articles = new[] { "a ", "an ", "the " };
            foreach (var article in articles)
            {
                if (userClean.StartsWith(article) && userClean.Substring(article.Length) == correctClean)
                    return true;
                
                if (correctClean.StartsWith(article) && correctClean.Substring(article.Length) == userClean)
                    return true;
            }
            
            return false;
        }
    }
}
