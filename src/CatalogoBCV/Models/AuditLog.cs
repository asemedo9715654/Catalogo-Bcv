namespace CatalogoBCV.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login
        public string EntityType { get; set; } = string.Empty; // Database, Table, Column, User
        public string EntityId { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
