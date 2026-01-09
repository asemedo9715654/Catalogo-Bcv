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
                    CASE WHEN k.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsPrimaryKey
                FROM 
                    INFORMATION_SCHEMA.TABLES t
                    JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
                    LEFT JOIN (
                        SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME AND tc.TABLE_SCHEMA = ku.TABLE_SCHEMA
                    ) k ON c.TABLE_SCHEMA = k.TABLE_SCHEMA AND c.TABLE_NAME = k.TABLE_NAME AND c.COLUMN_NAME = k.COLUMN_NAME
                WHERE 
                    t.TABLE_TYPE = 'BASE TABLE'
                ORDER BY 
                    t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            Table? currentTable = null;

            while (await reader.ReadAsync())
            {
                var schema = reader.GetString(0);
                var tableName = reader.GetString(1);
                var columnName = reader.GetString(2);
                var dataType = reader.GetString(3);
                var isNullable = reader.GetString(4) == "YES";
                var isPrimaryKey = reader.GetInt32(5) == 1;

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
                    IsPrimaryKey = isPrimaryKey
                });
            }

            return tables;
        }
    }
}
