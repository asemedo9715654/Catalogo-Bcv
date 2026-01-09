using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoBCV.Controllers
{
    [Authorize]
    public class GraphController : Controller
    {
        private readonly CatalogContext _context;

        public GraphController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? dbId, int? tableId)
        {
            if (dbId == null && tableId == null) return NotFound();

            if (tableId.HasValue && dbId == null)
            {
                var table = await _context.Tables
                   .Include(t => t.CatalogDatabase)
                   .FirstOrDefaultAsync(t => t.Id == tableId);
                if (table != null)
                {
                    dbId = table.CatalogDatabaseId;
                }
            }

            if (dbId == null) return NotFound();

            var db = await _context.CatalogDatabases
                .Include(d => d.Tables)
                .ThenInclude(t => t.Columns)
                .FirstOrDefaultAsync(d => d.Id == dbId);

            if (db == null) return NotFound();

            // Determine which tables to show
            IEnumerable<Table> tablesToShow = db.Tables;
            List<Tuple<Table, Table, string>> relationships = new List<Tuple<Table, Table, string>>();

            if (tableId.HasValue)
            {
                var factTable = db.Tables.FirstOrDefault(t => t.Id == tableId);
                if (factTable != null)
                {
                    var relatedTables = new HashSet<Table>();
                    relatedTables.Add(factTable);

                    // Heuristic: Find related tables based on column names (Star Schema)
                    foreach (var col in factTable.Columns)
                    {
                        var colName = col.Name;
                        var potentialTargets = new List<string>();

                        // 1. Try stripping suffixes/prefixes
                        if (colName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && colName.Length > 2)
                        {
                            potentialTargets.Add(colName.Substring(0, colName.Length - 2));
                        }
                        else if (colName.EndsWith("Key", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                        {
                            potentialTargets.Add(colName.Substring(0, colName.Length - 3));
                        }
                        else if (colName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                        {
                            potentialTargets.Add(colName.Substring(0, colName.Length - 3));
                        }
                        
                        if (colName.StartsWith("id_", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                        {
                             potentialTargets.Add(colName.Substring(3));
                        }
                        if (colName.StartsWith("fk_", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                        {
                             potentialTargets.Add(colName.Substring(3));
                        }

                        // 2. Try full name (e.g. "CurrencyIsoCode" -> "CurrencyIsoCode" table)
                        potentialTargets.Add(colName);

                        Table? match = null;

                        foreach (var target in potentialTargets)
                        {
                            // Normalize target for comparison
                            var targetClean = target.Replace("_", "").ToLowerInvariant();

                            match = db.Tables.FirstOrDefault(t =>
                            {
                                var tNameClean = t.Name.Replace("_", "").ToLowerInvariant();
                                return tNameClean == targetClean ||
                                       tNameClean == "dim" + targetClean ||
                                       tNameClean == targetClean + "dim" ||
                                       tNameClean == "t" + targetClean || // T_Prefix
                                       tNameClean == targetClean + "t";   // Suffix
                            });

                            if (match != null) break;
                        }

                        if (match != null && match.Id != factTable.Id)
                        {
                            relatedTables.Add(match);
                            relationships.Add(new Tuple<Table, Table, string>(factTable, match, colName));
                        }
                    }
                    tablesToShow = relatedTables;
                }
            }

            // Gerar definição Mermaid
            var sb = new StringBuilder();
            sb.AppendLine("classDiagram");

            foreach (var table in tablesToShow)
            {
                // Use Schema.Name and wrap in quotes to handle special characters/spaces
                // Escape quotes in the name itself
                var rawName = $"{table.Schema}.{table.Name}";
                var safeId = "T_" + System.Text.RegularExpressions.Regex.Replace(rawName, @"[^a-zA-Z0-9_]", "_");
                var label = rawName.Replace("\"", "'"); // Replace double quotes with single for display safety

                sb.AppendLine($"    class {safeId}[\"{label}\"] {{");
                
                // Add columns (limit to first 10 to avoid huge diagrams)
                foreach (var col in table.Columns.Take(10))
                {
                    var colType = col.DataType ?? "string";
                    var colName = col.Name ?? "Column";
                    
                    // Sanitize for Mermaid: replace spaces and special chars to prevent syntax errors
                    // Keep alphanumeric and underscores
                    colName = System.Text.RegularExpressions.Regex.Replace(colName, @"[^a-zA-Z0-9_]", "_");
                    
                    // Ensure type is also safe (though usually it is)
                    colType = System.Text.RegularExpressions.Regex.Replace(colType, @"[^a-zA-Z0-9_]", "");

                    sb.AppendLine($"        {colType} {colName}");
                }
                if (table.Columns.Count > 10)
                {
                    sb.AppendLine("        ...");
                }

                sb.AppendLine("    }");
            }

            foreach (var rel in relationships)
            {
                var factRaw = $"{rel.Item1.Schema}.{rel.Item1.Name}";
                var dimRaw = $"{rel.Item2.Schema}.{rel.Item2.Name}";

                var factId = "T_" + System.Text.RegularExpressions.Regex.Replace(factRaw, @"[^a-zA-Z0-9_]", "_");
                var dimId = "T_" + System.Text.RegularExpressions.Regex.Replace(dimRaw, @"[^a-zA-Z0-9_]", "_");

                sb.AppendLine($"    {factId} --> {dimId} : {rel.Item3}");
            }

            ViewBag.MermaidData = sb.ToString();
            ViewBag.DatabaseName = db.DatabaseName;
            ViewBag.DbId = db.Id;

            

            return View();
        }
    }
}
