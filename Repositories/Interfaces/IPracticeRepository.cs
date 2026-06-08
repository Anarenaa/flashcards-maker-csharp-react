using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace Repositories.Interfaces
{
    public interface IPracticeRepository
    {
        Task<IEnumerable<CardProgress>> GetAllAsync(
            Expression<Func<CardProgress, bool>>? filter = null,
            string includeProperties = "");
        // Взяти нову порцію карток для "зациклювання" (тих, що ще не вивчені до 1.0)
        Task<List<CardProgress>> GetNewBatchForPracticeAsync(int setId, int userId, int limit);

        // Оновити прогрес (LastReview, NextReview та саме значення Progress)
        Task UpdateProgressAsync(CardProgress progress);

        // Допоміжний метод для Quiz
        Task<List<string>> GetDistractorsAsync(int setId, int excludeCardId, int count);

        // Новий метод для отримання конкретного прогресу картки
        Task<CardProgress> GetCardProgressAsync(int userId, int flashcardId);

        // Новий метод для отримання прогресу одного сету
        Task<List<CardProgress>> GetSetProgressAsync(int userId, int setId);

        // Новий метод для отримання прогресу сетів одним запитом
        Task<List<CardProgress>> GetProgressForSetsAsync(int userId, List<int> setIds);

        // Новий метод для створення прогресу
        Task CreateProgressAsync(CardProgress progress);

        // Отримати всі прогреси користувача (з усіх сетів, які він практикував)
        Task<List<CardProgress>> GetAllUserProgressAsync(int userId);

        // Batch метод для отримання дестракторів для множини карток
        Task<Dictionary<int, List<string>>> GetBatchDistractorsAsync(int setId, List<int> excludeCardIds, int count);
        
        void DeleteRange(IEnumerable<CardProgress> entities);
    }
}
