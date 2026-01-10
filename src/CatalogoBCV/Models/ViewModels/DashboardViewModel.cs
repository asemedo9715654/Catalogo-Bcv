using System.Collections.Generic;
using CatalogoBCV.Models;

namespace CatalogoBCV.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalDatabases { get; set; }
        public int TotalTables { get; set; }
        public int TotalColumns { get; set; }
        public int TotalUsers { get; set; }
        public double DocumentationPercentage { get; set; }
        
        // New Stats
        public int FactTablesCount { get; set; }
        public int DimensionTablesCount { get; set; }
        public List<DomainStat> TopDomains { get; set; } = new();
        public List<TableStat> LargestTables { get; set; } = new();

        public List<CatalogDatabase> DatabasesStatus { get; set; } = new List<CatalogDatabase>();
    }

    public class DomainStat
    {
        public string Name { get; set; } = string.Empty;
        public int TableCount { get; set; }
    }

    public class TableStat
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public long RowCount { get; set; }
    }
}
