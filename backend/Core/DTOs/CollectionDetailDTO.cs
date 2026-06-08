namespace Core.DTOs
{
    public class CollectionDetailDTO : CollectionDTO
    {
        public List<SetDTO> Sets { get; set; } = new List<SetDTO>();
    }
}
