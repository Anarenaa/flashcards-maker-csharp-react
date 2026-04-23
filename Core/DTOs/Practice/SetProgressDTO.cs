using Core.DTOs.Practice;

namespace Core.DTOs.Practice
{
    public class SetProgressDTO
    {
        public int SetId { get; set; }
        public int TotalCards { get; set; }
        public float OverallProgress { get; set; } // 0.0 - 1.0
        
        
        public int MasteredCards { get; set; }    // >= 90%
        public int InProgressCards { get; set; }  // 10% - 90%
        public int NotStartedCards { get; set; }  // < 10%
        
        // Корисні розрахунки
        public double CompletionPercentage => TotalCards > 0 ? (double)MasteredCards / TotalCards * 100 : 0;
        public bool IsFullyMastered => OverallProgress >= 1.0f; // Повне проходження
    }
}
