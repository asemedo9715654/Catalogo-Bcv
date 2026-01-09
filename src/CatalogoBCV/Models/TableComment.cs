using System;

namespace CatalogoBCV.Models
{
    public class TableComment
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
