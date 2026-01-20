namespace CatalogoBCV.Models
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
