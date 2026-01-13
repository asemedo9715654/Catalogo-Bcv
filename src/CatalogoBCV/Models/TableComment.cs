using System;

namespace CatalogoBCV.Models
{
    public class TableComment
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public CommentType Type { get; set; } = CommentType.Suggestion;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
