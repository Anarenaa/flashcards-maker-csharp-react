namespace Core.DTOs
{
    public class PublicUserDTO
    {
        public int Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string UserName { get; set; }

        public List<SetDTO> Sets { get; set; } = new List<SetDTO>();
    }
}
