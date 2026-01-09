using System;

namespace CatalogoBCV.Models
{
    public class ColumnComment
    {
        public int Id { get; set; }
        public int ColumnId { get; set; }
        public Column Column { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
