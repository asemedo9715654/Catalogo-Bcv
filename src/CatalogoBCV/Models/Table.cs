namespace CatalogoBCV.Models
{
    public class Table
    {
        public int Id { get; set; }
        public int CatalogDatabaseId { get; set; }
        public CatalogDatabase CatalogDatabase { get; set; } = null!;
        public string Schema { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Table"; // Table, View
        public string? Description { get; set; }
        public string? Alias { get; set; }
        public bool IsFactTable { get; set; } // Flag manual ou inferida

        public int? DomainId { get; set; }
        public Domain? Domain { get; set; }

        public List<Column> Columns { get; set; } = new();
        public List<TableComment> Comments { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }
}
