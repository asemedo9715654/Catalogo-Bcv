using System;

namespace CatalogoBCV.Models
{
    public class ColumnComment : BaseEntity
    {
        public int Id { get; set; }
        public int ColumnId { get; set; }
        public Column Column { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public CommentType Type { get; set; } = CommentType.Suggestion;
    }
}
