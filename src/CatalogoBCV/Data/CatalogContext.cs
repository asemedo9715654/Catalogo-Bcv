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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<CatalogDatabase>().HasIndex(d => new { d.Server, d.DatabaseName }).IsUnique();
        }
    }
}
