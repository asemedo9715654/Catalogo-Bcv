using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models
{
    public class SourceSystem
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
    }
}
