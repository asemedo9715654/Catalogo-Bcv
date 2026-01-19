using System;

namespace CatalogoBCV.Models
{
    public abstract class BaseEntity 
    { 
        // Auditoria 
        public DateTime CreatedAt { get; set; } 
        public string CreatedBy { get; set; } = string.Empty;
 
        public DateTime? UpdatedAt { get; set; } 
        public string? UpdatedBy { get; set; } 
 
        // Soft Delete 
        public bool IsDeleted { get; set; } 
        public DateTime? DeletedAt { get; set; } 
        public string? DeletedBy { get; set; } 
    }
}
