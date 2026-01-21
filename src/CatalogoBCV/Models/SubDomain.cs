using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models
{
    public class SubDomain : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int DomainId { get; set; }
        public Domain Domain { get; set; } = null!;
    }
}
