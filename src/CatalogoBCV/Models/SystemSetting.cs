using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models
{
    public class SystemSetting : BaseEntity
    {
        [Key]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Group { get; set; }
        
        public string Type { get; set; } = "string"; // string, boolean, number
    }
}
