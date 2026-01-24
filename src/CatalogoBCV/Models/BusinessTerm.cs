using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models
{
    public class BusinessTerm : BaseEntity
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Term { get; set; }
        
        [Required]
        public string Definition { get; set; }
        
        public string Owner { get; set; }
        
        public TermStatus Status { get; set; }
    }

    public enum TermStatus
    {
        [Display(Name = "Rascunho")]
        Draft,
        
        [Display(Name = "Aprovado")]
        Approved,
        
        [Display(Name = "Obsoleto")]
        Obsolete,
        
        [Display(Name = "Depreciado")]
        Deprecated
    }
}