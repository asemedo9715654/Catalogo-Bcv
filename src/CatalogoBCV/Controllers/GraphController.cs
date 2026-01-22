using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace CatalogoBCV.Controllers
{
    [Authorize(Policy = "CanViewCatalog")]
    public class GraphController : Controller
    {
        private readonly CatalogContext _context;

        public GraphController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? dbId, int? tableId, bool embedded = false)
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

            // Generate Cytoscape definition
            var nodes = new List<object>();
            var edges = new List<object>();

            foreach (var table in tablesToShow)
            {
                var safeId = $"T_{table.Id}";
                var columns = table.Columns.Select(c => new { name = c.Name, type = c.DataType, isPk = c.IsPrimaryKey, isFk = c.IsForeignKey }).Take(20).ToList();
                
                string nodeType = "dim";
                if (table.IsFactTable)
                {
                    nodeType = "fact";
                }
                else if (table.Name.Contains("Tempo", StringComparison.OrdinalIgnoreCase) || 
                         table.Name.Contains("Time", StringComparison.OrdinalIgnoreCase) ||
                         table.Name.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
                         table.Name.Contains("Dim_Data", StringComparison.OrdinalIgnoreCase) ||
                         table.Name.Contains("Calend", StringComparison.OrdinalIgnoreCase))
                {
                    nodeType = "time";
                }

                nodes.Add(new
                {
                    data = new
                    {
                        id = safeId,
                        name = table.Name,
                        columns = columns,
                        schema = table.Schema,
                        type = nodeType
                    }
                });
            }

            foreach (var rel in relationships)
            {
                var factId = $"T_{rel.Item1.Id}";
                var dimId = $"T_{rel.Item2.Id}";
                var label = rel.Item3;

                edges.Add(new
                {
                    data = new
                    {
                        source = factId,
                        target = dimId,
                        label = label
                    }
                });
            }

            ViewBag.GraphData = System.Text.Json.JsonSerializer.Serialize(new { nodes, edges });
            ViewBag.DatabaseName = db.DatabaseName;
            ViewBag.DbId = db.Id;
            ViewBag.Embedded = embedded;

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
