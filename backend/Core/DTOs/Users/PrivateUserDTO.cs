namespace Core.DTOs.Users
{
    public class PrivateUserDTO : IUserProfileDTO
    {
        public int Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string UserName { get; set; }
        public bool IsPublic { get; set; }
        public int PublicSetsCount { get; set; }
    }
}
