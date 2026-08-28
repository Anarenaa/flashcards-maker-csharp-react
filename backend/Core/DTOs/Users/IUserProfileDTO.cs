namespace Core.DTOs.Users
{
    public interface IUserProfileDTO
    {
        int Id { get; set; }
        string UserName { get; set; }
        string? AvatarUrl { get; set; }
        bool IsPublic { get; set; }
    }
}
