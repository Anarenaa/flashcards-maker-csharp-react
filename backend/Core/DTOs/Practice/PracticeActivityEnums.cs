namespace Core.DTOs.Practice
{
    public enum PracticeActivityType
    {
        Review = 1,       // (0-10% of progress)
        Quiz = 2,         // (10-30% of progress)
        Matching = 3,     // (30-50% of progress)
        Writing = 4,      // (50-90% of progress)
        Mixed = 5         // (90-100% of progress)
    }

    public static class PracticeActivityLimit
    {
        public const float ReviewLimit = 0.10f;
        public const float QuizLimit = 0.30f;
        public const float MatchingLimit = 0.50f;
        public const float WritingLimit = 0.90f;
        public const float MaxLimit = 1.0f;
    }
}