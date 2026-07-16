namespace Core.DTOs
{
    public class SetDetailDTO : SetDTO
    {
        public List<CategoryDTO> Categories { get; set; } = new();
    }
}
