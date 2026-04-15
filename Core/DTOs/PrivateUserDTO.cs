namespace Core.DTOs
{
    public class PrivateUserDTO
    {
        public int Id { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string? AvatarUrl { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public bool IsPublic { get; set; }

        public int? CollectionsCount { get; set; }
        public int? SetsCount { get; set; }
        public int? FlashcardsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
