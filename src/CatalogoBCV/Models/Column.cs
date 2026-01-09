namespace CatalogoBCV.Models
{
    public class Column
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public string? Description { get; set; }
        public string? Alias { get; set; }
        public List<ColumnComment> Comments { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }
}
