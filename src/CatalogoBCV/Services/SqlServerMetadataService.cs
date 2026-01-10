using CatalogoBCV.Models;
using Microsoft.Data.SqlClient;

namespace CatalogoBCV.Services
{
    public class SqlServerMetadataService : IMetadataService
    {
        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Table>> GetMetadataAsync(string connectionString)
        {
            var tables = new List<Table>();

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    t.TABLE_SCHEMA, 
                    t.TABLE_NAME, 
                    c.COLUMN_NAME, 
                    c.DATA_TYPE, 
                    c.IS_NULLABLE,
                    CASE WHEN k.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsPrimaryKey,
                    CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsForeignKey
                FROM 
                    INFORMATION_SCHEMA.TABLES t
                    JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
                    LEFT JOIN (
                        SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME AND tc.TABLE_SCHEMA = ku.TABLE_SCHEMA
                    ) k ON c.TABLE_SCHEMA = k.TABLE_SCHEMA AND c.TABLE_NAME = k.TABLE_NAME AND c.COLUMN_NAME = k.COLUMN_NAME
                    LEFT JOIN (
                        SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_TYPE = 'FOREIGN KEY' AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME AND tc.TABLE_SCHEMA = ku.TABLE_SCHEMA
                    ) fk ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA AND c.TABLE_NAME = fk.TABLE_NAME AND c.COLUMN_NAME = fk.COLUMN_NAME
                WHERE 
                    t.TABLE_TYPE = 'BASE TABLE'
                ORDER BY 
                    t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

            using (var cmd = new SqlCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                Table? currentTable = null;

                while (await reader.ReadAsync())
                {
                    var schema = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    var columnName = reader.GetString(2);
                    var dataType = reader.GetString(3);
                    var isNullable = reader.GetString(4) == "YES";
                    var isPrimaryKey = reader.GetInt32(5) == 1;
                    var isForeignKey = reader.GetInt32(6) == 1;

                    if (currentTable == null || currentTable.Name != tableName || currentTable.Schema != schema)
                    {
                        currentTable = new Table
                        {
                            Schema = schema,
                            Name = tableName,
                            Type = "Table",
                            Columns = new List<Column>()
                        };
                        tables.Add(currentTable);
                    }

                    currentTable.Columns.Add(new Column
                    {
                        Name = columnName,
                        DataType = dataType,
                        IsNullable = isNullable,
                        IsPrimaryKey = isPrimaryKey,
                        IsForeignKey = isForeignKey
                    });
                }
            }

            // Get Row Counts
            string rowCountQuery = @"
                SELECT 
                    s.name AS SchemaName, 
                    t.name AS TableName, 
                    SUM(p.rows) AS [RowCount]
                FROM 
                    sys.tables t
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    INNER JOIN sys.indexes i ON t.object_id = i.object_id
                    INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
                WHERE 
                    t.is_ms_shipped = 0 
                    AND i.index_id < 2
                GROUP BY 
                    s.name, t.name";

            using (var cmdCount = new SqlCommand(rowCountQuery, conn))
            using (var readerCount = await cmdCount.ExecuteReaderAsync())
            {
                while (await readerCount.ReadAsync())
                {
                    var schema = readerCount.GetString(0);
                    var tableName = readerCount.GetString(1);
                    var rowCount = readerCount.GetInt64(2);

                    var table = tables.FirstOrDefault(t => t.Schema == schema && t.Name == tableName);
                    if (table != null)
                    {
                        table.RowCount = rowCount;
                    }
                }
            }

            return tables;
        }
    }
}
