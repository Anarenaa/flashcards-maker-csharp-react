using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class CategoryDTO
    {
        public int? Id { get; set; }
        [StringLength(50, ErrorMessage="Назва категорії занадто довга")]
        public required string Name { get; set; }
    }
}
