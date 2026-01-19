using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models
{
    public class CatalogDatabase : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public string Server { get; set; } = string.Empty;
        [Required]
        public string DatabaseName { get; set; } = string.Empty;
        
        public bool UseWindowsAuthentication { get; set; }

        public string? Username { get; set; }
        
        public string? EncryptedPassword { get; set; }
        
        public List<Table> Tables { get; set; } = new();
    }
}
