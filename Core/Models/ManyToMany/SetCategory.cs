namespace Core.Models.ManyToMany
{
    public class SetCategory
    {
        public int SetId { get; set; }
        public required Set Set { get; set; }

        public int CategoryId { get; set; }
        public required Category Category { get; set; }
    }
}
