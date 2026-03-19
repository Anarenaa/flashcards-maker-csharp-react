namespace Core.Models.ManyToMany
{
    public class CollectionSet
    {
        public int CollectionId { get; set; }
        public required Collection Collection { get; set; }

        public int SetId { get; set; }
        public required Set Set { get; set; }
    }
}
