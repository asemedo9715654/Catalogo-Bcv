using System;

namespace CatalogoBCV.Models
{
    public class TableComment : BaseEntity
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public CommentType Type { get; set; } = CommentType.Suggestion;
    }
}
