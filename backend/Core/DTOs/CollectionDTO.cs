namespace Core.DTOs
{
    public class CollectionDTO
    {
        public int? Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int SetsCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
