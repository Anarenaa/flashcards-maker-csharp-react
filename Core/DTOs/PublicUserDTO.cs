using System.Reflection.Metadata;

namespace Core.DTOs
{
    public class PublicUserDTO : UserEditDTO
    {
        public List<SetDTO> Sets { get; set; } = new List<SetDTO>();
    }
}
