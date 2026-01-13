namespace CatalogoBCV.Models
{
    public enum TableStatus
    {
        EmValidacao,
        Certificada,
        Obsoleta
    }

    public enum ConfidentialityLevel
    {
        Publico,
        Interno,
        Restrito
    }

    public class Table
    {
        public int Id { get; set; }
        public int CatalogDatabaseId { get; set; }
        public CatalogDatabase CatalogDatabase { get; set; } = null!;
        public string Schema { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Table"; // Table, View
        public string? Description { get; set; }
        public string? Alias { get; set; }
        public long? RowCount { get; set; }
        public bool IsFactTable { get; set; } // Flag manual ou inferida
        public TableStatus Status { get; set; } = TableStatus.EmValidacao;

        // New properties
        public string? Owner { get; set; }
        public string? DataSteward { get; set; }
        public ConfidentialityLevel ConfidentialityLevel { get; set; } = ConfidentialityLevel.Interno;
        public string? AffectedReports { get; set; }
        public string? DependentDashboards { get; set; }

        public int? DomainId { get; set; }
        public Domain? Domain { get; set; }

        public List<Column> Columns { get; set; } = new();
        public List<TableComment> Comments { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }
}
