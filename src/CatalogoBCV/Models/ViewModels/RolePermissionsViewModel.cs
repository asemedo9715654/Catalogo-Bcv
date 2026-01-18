using System.Collections.Generic;

namespace CatalogoBCV.Models.ViewModels
{
    public class RolePermissionsViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new List<PermissionItem>();
    }

    public class PermissionItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Assigned { get; set; }
    }
}

