using Core.DTOs.Practice;

namespace Core.DTOs.Practice
{
    public class UserProgressDTO
    {
        public int UserId { get; set; }
        
        // Статистика по сетах, які користувач проходив
        public List<SetProgressSummary> SetProgresses { get; set; } = new();
        
        // Загальна статистика по пройдених сетах
        public int PracticedSets { get; set; }           // Кількість сетів, які почав проходити
        public int CompletedSets { get; set; }            // Кількість повністю завершених сетів
        
        // Загальний прогрес по всіх пройдених картах
        public int PracticedCards { get; set; }          // Загальна кількість карток, які практикував
        public int MasteredCards { get; set; }            // Кількість повністю вивчених карток
        public int InProgressCards { get; set; }          // Картки в процесі вивчення
        public int NotStartedCards { get; set; }          // Картки, які ще не почав
        
        public float OverallProgress { get; set; }        // 0.0 - 1.0
        public double CompletionPercentage => PracticedCards > 0 ? (double)MasteredCards / PracticedCards * 100 : 0;
        
        // Статистика активності
        public DateTime LastActivity { get; set; }
        public int TotalPracticeSessions { get; set; }
    }

    public class SetProgressSummary
    {
        public int SetId { get; set; }
        public float Progress { get; set; }
        public int MasteredCards { get; set; }
        public bool IsCompleted => Progress >= 1.0f; // Змінено на 1.0f для повного проходження
    }
}
