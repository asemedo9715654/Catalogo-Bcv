using CatalogoBCV.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CatalogDatabase> CatalogDatabases { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TableComment> TableComments { get; set; }
        public DbSet<ColumnComment> ColumnComments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<CatalogDatabase>().HasIndex(d => new { d.Server, d.DatabaseName }).IsUnique();

            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting { Key = "AppTitle", Value = "Catalogo BCV", Description = "Application Title", Group = "General", Type = "string" },
                new SystemSetting { Key = "Theme", Value = "light", Description = "UI Theme", Group = "Appearance", Type = "string" },
                new SystemSetting { Key = "PageSize", Value = "10", Description = "Default items per page", Group = "General", Type = "number" },
                new SystemSetting { Key = "PrimaryColor", Value = "#0d6efd", Description = "Primary Color", Group = "Appearance", Type = "color" },
                new SystemSetting { Key = "HeaderColor", Value = "#ffffff", Description = "Header Background Color", Group = "Appearance", Type = "color" },
                new SystemSetting { Key = "SidebarColor", Value = "#f8f9fa", Description = "Sidebar Background Color", Group = "Appearance", Type = "color" }
            );
        }
    }
}
