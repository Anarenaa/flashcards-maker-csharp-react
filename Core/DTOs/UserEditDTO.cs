namespace Core.DTOs
{
    public class UserEditDTO
    {
        public int Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        
        public int? CollectionsCount { get; set; }
        public int SetsCount { get; set; } = 0;
        public int FlashcardsCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
    }
}
