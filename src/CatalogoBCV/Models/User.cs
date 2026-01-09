namespace CatalogoBCV.Models
{
    public enum UserRole
    {
        Admin,
        Editor,
        Reader
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
