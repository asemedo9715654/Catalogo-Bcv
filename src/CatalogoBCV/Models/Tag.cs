using System.Collections.Generic;

namespace CatalogoBCV.Models
{
    public class Tag : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public List<Table> Tables { get; set; } = new();
        public List<Column> Columns { get; set; } = new();
    }
}
