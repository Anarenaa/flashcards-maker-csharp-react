namespace Core.DTOs
{
    public class SetWithProgressDTO : SetDTO
    {
        public float OverallProgress { get; set; }
        public bool IsCompleted => OverallProgress >= 1.0f;
    }
}
