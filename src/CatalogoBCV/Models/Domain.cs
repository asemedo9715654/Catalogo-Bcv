namespace CatalogoBCV.Models
{
    public class Domain : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<Table> Tables { get; set; } = new();
    }
}
