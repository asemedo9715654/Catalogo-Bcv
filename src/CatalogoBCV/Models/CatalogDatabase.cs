using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models
{
    public class CatalogDatabase
    {
        public int Id { get; set; }
        [Required]
        public string Server { get; set; } = string.Empty;
        [Required]
        public string DatabaseName { get; set; } = string.Empty;
        
        public bool UseWindowsAuthentication { get; set; }

        public string? Username { get; set; }
        
        public string? EncryptedPassword { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public List<Table> Tables { get; set; } = new();
    }
}
