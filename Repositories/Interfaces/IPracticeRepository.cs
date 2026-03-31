using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace Repositories.Interfaces
{
    public interface IPracticeRepository
    {
        // Взяти нову порцію карток для "зациклювання" (тих, що ще не вивчені до 1.0)
        Task<List<CardProgress>> GetNewBatchForPracticeAsync(int setId, int userId, int limit);

        // Оновити прогрес (LastReview, NextReview та саме значення Progress)
        Task UpdateProgressAsync(CardProgress progress);

        // Допоміжний метод для Quiz
        Task<List<string>> GetDistractorsAsync(int setId, int excludeCardId, int count);
    }
}
