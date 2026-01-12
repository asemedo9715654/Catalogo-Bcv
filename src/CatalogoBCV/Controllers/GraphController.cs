using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

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

            IEnumerable<Table> tablesToShow;
            var relationships = new List<Tuple<Table, Table, string>>();

            if (tableId.HasValue)
            {
                var factTable = db.Tables.FirstOrDefault(t => t.Id == tableId);
                if (factTable != null)
                {
                    var relatedTables = new HashSet<Table> { factTable };
                    FindRelationships(factTable, db.Tables, relatedTables, relationships);
                    tablesToShow = relatedTables;
                }
                else
                {
                    tablesToShow = new List<Table>();
                }
            }
            else
            {
                // Show all tables
                tablesToShow = db.Tables;
                // Calculate relationships for ALL tables
                foreach (var table in db.Tables)
                {
                    FindRelationships(table, db.Tables, null, relationships);
                }
            }

            // Generate Mermaid definition
            var sb = new StringBuilder();
            sb.AppendLine("classDiagram");

            foreach (var table in tablesToShow)
            {
                var rawName = $"{table.Schema}.{table.Name}";
                var safeId = GetSafeId(rawName);
                var label = rawName.Replace("\"", "'");

                sb.AppendLine($"    class {safeId}[\"{label}\"] {{");

                foreach (var col in table.Columns.Take(20))
                {
                    var colType = col.DataType ?? "string";
                    var colName = col.Name ?? "Column";

                    colName = Regex.Replace(colName, @"[^a-zA-Z0-9_]", "_");
                    colType = Regex.Replace(colType, @"[^a-zA-Z0-9_]", "");

                    var suffix = "";
                    if (col.IsPrimaryKey) suffix += " PK";
                    if (col.IsForeignKey) suffix += " FK";

                    sb.AppendLine($"        {colType} {colName}{suffix}");
                }
                if (table.Columns.Count > 20)
                {
                    sb.AppendLine("        ...");
                }

                sb.AppendLine("    }");
            }

            foreach (var rel in relationships)
            {
                var factRaw = $"{rel.Item1.Schema}.{rel.Item1.Name}";
                var dimRaw = $"{rel.Item2.Schema}.{rel.Item2.Name}";

                var factId = GetSafeId(factRaw);
                var dimId = GetSafeId(dimRaw);

                // Ensure we don't draw lines to tables not in the diagram (should generally be safe here)
                // But specifically for 'Single Table' view, if FindRelationships found a match, it added it to relatedTables.
                
                sb.AppendLine($"    {factId} --> {dimId} : {rel.Item3}");
            }

            ViewBag.MermaidData = sb.ToString();
            ViewBag.DatabaseName = db.DatabaseName;
            ViewBag.DbId = db.Id;

            return View();
        }

        private string GetSafeId(string name)
        {
            return "T_" + Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        }

        private void FindRelationships(Table sourceTable, IEnumerable<Table> potentialTargets, HashSet<Table>? relatedTablesAccumulator, List<Tuple<Table, Table, string>> relationshipsAccumulator)
        {
            foreach (var col in sourceTable.Columns)
            {
                var colName = col.Name;
                var targets = new List<string>();

                // 1. Try stripping suffixes/prefixes
                if (colName.EndsWith("_surrogate_key", StringComparison.OrdinalIgnoreCase) && colName.Length > 14)
                {
                    targets.Add(colName.Substring(0, colName.Length - 14));
                }
                else if (colName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && colName.Length > 2)
                {
                    targets.Add(colName.Substring(0, colName.Length - 2));
                }
                else if (colName.EndsWith("Key", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                {
                    targets.Add(colName.Substring(0, colName.Length - 3));
                }
                else if (colName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                {
                    targets.Add(colName.Substring(0, colName.Length - 3));
                }

                if (colName.StartsWith("id_", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                {
                    targets.Add(colName.Substring(3));
                }
                if (colName.StartsWith("fk_", StringComparison.OrdinalIgnoreCase) && colName.Length > 3)
                {
                    targets.Add(colName.Substring(3));
                }

                // 2. Try full name
                targets.Add(colName);

                Table? match = null;

                foreach (var target in targets)
                {
                    var targetClean = target.Replace("_", "").ToLowerInvariant();

                    match = potentialTargets.FirstOrDefault(t =>
                    {
                        var tNameClean = t.Name.Replace("_", "").ToLowerInvariant();
                        
                        // Check exact matches or dim/t prefix matches
                        if (CheckMatch(tNameClean, targetClean)) return true;

                        // Check pluralized target matches (e.g. target="customer", table="customers")
                        if (CheckMatch(tNameClean, targetClean + "s")) return true;
                        if (CheckMatch(tNameClean, targetClean + "es")) return true;

                        return false;
                    });

                    if (match != null) break;
                }

                if (match != null && match.Id != sourceTable.Id)
                {
                    relatedTablesAccumulator?.Add(match);
                    relationshipsAccumulator.Add(new Tuple<Table, Table, string>(sourceTable, match, colName));
                }
            }
        }

        private bool CheckMatch(string tableName, string targetName)
        {
            return tableName == targetName ||
                   tableName == "dim" + targetName ||
                   tableName == targetName + "dim" ||
                   tableName == "t" + targetName ||
                   tableName == targetName + "t";
        }
    }
}
