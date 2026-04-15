using System.Reflection.Metadata;

namespace Core.DTOs
{
    public class PublicUserDTO : PrivateUserDTO
    {
        public List<SetDTO> Sets { get; set; } = new List<SetDTO>();
    }
}
