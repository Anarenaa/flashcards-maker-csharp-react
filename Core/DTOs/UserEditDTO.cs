namespace Core.DTOs
{
    public class UserEditDTO
    {
        public int Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
    }
}
