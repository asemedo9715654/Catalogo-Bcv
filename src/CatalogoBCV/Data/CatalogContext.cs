using CatalogoBCV.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CatalogoBCV.Data
{
    public class CatalogContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogContext(DbContextOptions<CatalogContext> options, IHttpContextAccessor httpContextAccessor = null) : base(options) 
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            // Use "System" or empty string if context is missing (e.g. background tasks or migrations)
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                    if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                    {
                        entry.Entity.CreatedBy = currentUser;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.Now;
                    entry.Entity.UpdatedBy = currentUser;

                    // Ensure CreatedAt/CreatedBy are not modified during update
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.Now;
                    entry.Entity.DeletedBy = currentUser;
                }
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<CatalogDatabase> CatalogDatabases { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TableComment> TableComments { get; set; }
        public DbSet<ColumnComment> ColumnComments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<SubDomain> SubDomains { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<SourceSystem> SourceSystems { get; set; }
        public DbSet<BusinessTerm> BusinessTerms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.RoleId)
                .HasColumnName("Role");
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Administrador do sistema" },
                new Role { Id = 2, Name = "Editor", Description = "Pode editar conteúdos" },
                new Role { Id = 3, Name = "Reader", Description = "Apenas leitura" }
            );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "CanViewCatalog", Description = "Pode ver o catálogo" },
                new Permission { Id = 2, Name = "CanEditCatalog", Description = "Pode editar o catálogo" },
                new Permission { Id = 3, Name = "CanViewAudit", Description = "Pode ver logs de auditoria" },
                new Permission { Id = 4, Name = "CanManageUsers", Description = "Pode gerir utilizadores" },
                new Permission { Id = 5, Name = "CanManageRoles", Description = "Pode gerir roles" },
                new Permission { Id = 6, Name = "CanManageSettings", Description = "Pode gerir configurações" },
                new Permission { Id = 7, Name = "CanCreateCatalog", Description = "Pode criar novas conexões de catálogo" }
            );

            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 },
                new RolePermission { RoleId = 1, PermissionId = 5 },
                new RolePermission { RoleId = 1, PermissionId = 6 },
                new RolePermission { RoleId = 1, PermissionId = 7 },
                new RolePermission { RoleId = 2, PermissionId = 1 },
                new RolePermission { RoleId = 2, PermissionId = 2 },
                new RolePermission { RoleId = 2, PermissionId = 7 },
                new RolePermission { RoleId = 3, PermissionId = 1 }
            );

            modelBuilder.Entity<CatalogDatabase>().HasIndex(d => new { d.Server, d.DatabaseName }).IsUnique();

            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting { Key = "AppTitle", Value = "Catalogo BCV", Description = "Application Title", Group = "General", Type = "string" },
                new SystemSetting { Key = "Theme", Value = "light", Description = "UI Theme", Group = "Appearance", Type = "string" },
                new SystemSetting { Key = "PageSize", Value = "10", Description = "Default items per page", Group = "General", Type = "number" },
                new SystemSetting { Key = "PrimaryColor", Value = "#0d6efd", Description = "Primary Color", Group = "Appearance", Type = "color" },
                new SystemSetting { Key = "HeaderColor", Value = "#ffffff", Description = "Header Background Color", Group = "Appearance", Type = "color" },
                new SystemSetting { Key = "SidebarColor", Value = "#f8f9fa", Description = "Sidebar Background Color", Group = "Appearance", Type = "color" }
            );

            // Apply Global Query Filter for Soft Delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(CatalogContext).GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    var genericMethod = method?.MakeGenericMethod(entityType.ClrType);
                    genericMethod?.Invoke(null, new object[] { modelBuilder });
                }
            }
        }

        private static void SetGlobalQueryFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
