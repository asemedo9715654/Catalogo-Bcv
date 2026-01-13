using CatalogoBCV.Data;
using CatalogoBCV.Models;
using CatalogoBCV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = QuestPDF.Infrastructure.Color;
using Table = CatalogoBCV.Models.Table; // Explicitly use the model Table

namespace CatalogoBCV.Controllers
{
    [Authorize]
    public class CatalogController : Controller
    {
        private readonly CatalogContext _context;
        private readonly IMetadataService _metadataService;

        public CatalogController(CatalogContext context, IMetadataService metadataService)
        {
            _context = context;
            _metadataService = metadataService;
        }

        public async Task<IActionResult> Index()
        {
            var databases = await _context.CatalogDatabases
                .Include(d => d.Tables)
                .ToListAsync();
            return View(databases);
        }

        public async Task<IActionResult> GenerateDocumentation(int id, string format = "md")
        {
            var db = await _context.CatalogDatabases
                .Include(d => d.Tables)
                .ThenInclude(t => t.Columns)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (db == null) return NotFound();

            if (format.ToLower() == "word")
            {
                return GenerateWordDocumentation(db);
            }
            else if (format.ToLower() == "pdf")
            {
                return GeneratePdfDocumentation(db);
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"# Documentação do Catálogo de Dados - {db.DatabaseName}");
            sb.AppendLine($"Gerado em: {DateTime.Now}");
            sb.AppendLine();

            sb.AppendLine($"## Base de Dados: {db.DatabaseName}");
            sb.AppendLine($"Servidor: {db.Server}");
            sb.AppendLine();

            foreach (var table in db.Tables)
            {
                sb.AppendLine($"### Tabela: {table.Schema}.{table.Name}");
                sb.AppendLine($"Descrição: {table.Description ?? "N/A"}");
                sb.AppendLine();
                sb.AppendLine("| Coluna | Tipo | Nulável | PK | FK | Descrição |");
                sb.AppendLine("| --- | --- | --- | --- | --- | --- |");

                foreach (var col in table.Columns)
                {
                    sb.AppendLine($"| {col.Name} | {col.DataType} | {(col.IsNullable ? "Sim" : "Não")} | {(col.IsPrimaryKey ? "Sim" : "Não")} | {(col.IsForeignKey ? "Sim" : "Não")} | {col.Description ?? ""} |");
                }
                sb.AppendLine();
            }

            var content = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(content, "text/markdown", $"Catalogo_{db.DatabaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
        }

        private IActionResult GenerateWordDocumentation(CatalogDatabase db)
        {
            using (var mem = new MemoryStream())
            {
                using (var wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
                {
                    var mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    var body = mainPart.Document.AppendChild(new Body());

                    // Title
                    body.AppendChild(new Paragraph(new Run(new Text($"Documentação do Catálogo de Dados - {db.DatabaseName}")) { RunProperties = new RunProperties(new Bold(), new FontSize { Val = "32" }) }));
                    body.AppendChild(new Paragraph(new Run(new Text($"Gerado em: {DateTime.Now}"))));
                    body.AppendChild(new Paragraph(new Run(new Text("")))); // Empty line

                    // DB Info
                    body.AppendChild(new Paragraph(new Run(new Text($"Base de Dados: {db.DatabaseName}")) { RunProperties = new RunProperties(new Bold(), new FontSize { Val = "28" }) }));
                    body.AppendChild(new Paragraph(new Run(new Text($"Servidor: {db.Server}"))));
                    body.AppendChild(new Paragraph(new Run(new Text(""))));

                    foreach (var table in db.Tables)
                    {
                        body.AppendChild(new Paragraph(new Run(new Text($"Tabela: {table.Schema}.{table.Name}")) { RunProperties = new RunProperties(new Bold(), new FontSize { Val = "24" }) }));
                        body.AppendChild(new Paragraph(new Run(new Text($"Descrição: {table.Description ?? "N/A"}"))));
                        
                        // Table for Columns
                        var wordTable = new DocumentFormat.OpenXml.Wordprocessing.Table();
                        
                        // Table Properties (Borders)
                        var tblProps = new TableProperties(
                            new TableBorders(
                                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                            )
                        );
                        wordTable.AppendChild(tblProps);

                        // Header Row
                        var tr = new TableRow();
                        tr.Append(
                            CreateCell("Coluna", true),
                            CreateCell("Tipo", true),
                            CreateCell("Nulável", true),
                            CreateCell("PK", true),
                            CreateCell("FK", true),
                            CreateCell("Descrição", true)
                        );
                        wordTable.Append(tr);

                        // Data Rows
                        foreach (var col in table.Columns)
                        {
                            var row = new TableRow();
                            row.Append(
                                CreateCell(col.Name),
                                CreateCell(col.DataType),
                                CreateCell(col.IsNullable ? "Sim" : "Não"),
                                CreateCell(col.IsPrimaryKey ? "Sim" : "Não"),
                                CreateCell(col.IsForeignKey ? "Sim" : "Não"),
                                CreateCell(col.Description ?? "")
                            );
                            wordTable.Append(row);
                        }

                        body.AppendChild(wordTable);
                        body.AppendChild(new Paragraph(new Run(new Text(""))));
                    }
                }

                return File(mem.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Catalogo_{db.DatabaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
            }
        }

        private TableCell CreateCell(string text, bool bold = false)
        {
            var run = new Run(new Text(text));
            if (bold) run.RunProperties = new RunProperties(new Bold());
            return new TableCell(new Paragraph(run));
        }

        private IActionResult GeneratePdfDocumentation(CatalogDatabase db)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Documentação - {db.DatabaseName}")
                        .SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Gerado em: {DateTime.Now}");
                            x.Item().Text($"Servidor: {db.Server}");

                            foreach (var table in db.Tables)
                            {
                                x.Item().Column(c =>
                                {
                                    c.Spacing(5);
                                    c.Item().Text($"Tabela: {table.Schema}.{table.Name}").FontSize(16).Bold();
                                    c.Item().Text($"Descrição: {table.Description ?? "N/A"}").Italic();

                                    c.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(3);
                                        });

                                        t.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Coluna");
                                            header.Cell().Element(CellStyle).Text("Tipo");
                                            header.Cell().Element(CellStyle).Text("Nulável");
                                            header.Cell().Element(CellStyle).Text("PK");
                                            header.Cell().Element(CellStyle).Text("FK");
                                            header.Cell().Element(CellStyle).Text("Descrição");

                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                                            }
                                        });

                                        foreach (var col in table.Columns)
                                        {
                                            t.Cell().Element(CellStyle).Text(col.Name);
                                            t.Cell().Element(CellStyle).Text(col.DataType);
                                            t.Cell().Element(CellStyle).Text(col.IsNullable ? "Sim" : "Não");
                                            t.Cell().Element(CellStyle).Text(col.IsPrimaryKey ? "Sim" : "Não");
                                            t.Cell().Element(CellStyle).Text(col.IsForeignKey ? "Sim" : "Não");
                                            t.Cell().Element(CellStyle).Text(col.Description ?? "");

                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                                            }
                                        }
                                    });
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Catalogo_{db.DatabaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CatalogDatabase catalogDatabase)
        {
            if (!catalogDatabase.UseWindowsAuthentication)
            {
                if (string.IsNullOrEmpty(catalogDatabase.Username))
                    ModelState.AddModelError("Username", "Utilizador é obrigatório para autenticação SQL.");
                if (string.IsNullOrEmpty(catalogDatabase.EncryptedPassword))
                    ModelState.AddModelError("EncryptedPassword", "Password é obrigatória para autenticação SQL.");
            }

            if (ModelState.IsValid)
            {
                // Construir connection string para teste
                string connectionString;
                if (catalogDatabase.UseWindowsAuthentication)
                {
                    connectionString = $"Server={catalogDatabase.Server};Database={catalogDatabase.DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
                }
                else
                {
                    connectionString = $"Server={catalogDatabase.Server};Database={catalogDatabase.DatabaseName};User Id={catalogDatabase.Username};Password={catalogDatabase.EncryptedPassword};TrustServerCertificate=True;";
                }

                if (await _metadataService.TestConnectionAsync(connectionString))
                {
                    // Importar metadados
                    var tables = await _metadataService.GetMetadataAsync(connectionString);
                    catalogDatabase.Tables = tables;
                    catalogDatabase.CreatedAt = DateTime.UtcNow;
                    catalogDatabase.LastUpdated = DateTime.UtcNow;
                    catalogDatabase.CreatedBy = User.Identity?.Name ?? "System";

                    _context.Add(catalogDatabase);
                    
                    // Audit Log
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Action = "Create",
                        EntityType = "CatalogDatabase",
                        EntityId = catalogDatabase.DatabaseName,
                        Username = User.Identity?.Name ?? "System"
                    });

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Base de dados adicionada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Não foi possível conectar à base de dados.";
                    ModelState.AddModelError("", "Não foi possível conectar à base de dados.");
                }
            }
            return View(catalogDatabase);
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();

            var db = await _context.CatalogDatabases
                .Include(d => d.Tables)
                .ThenInclude(t => t.Columns)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (db == null) return NotFound();

            string connectionString;
            if (db.UseWindowsAuthentication)
            {
                connectionString = $"Server={db.Server};Database={db.DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
            }
            else
            {
                connectionString = $"Server={db.Server};Database={db.DatabaseName};User Id={db.Username};Password={db.EncryptedPassword};TrustServerCertificate=True;";
            }

            if (await _metadataService.TestConnectionAsync(connectionString))
            {
                var freshTables = await _metadataService.GetMetadataAsync(connectionString);
                int newTablesCount = 0;
                int newColumnsCount = 0;
                int updatedColumnsCount = 0;
                bool hasSchemaChanges = false;
                bool hasStatsChanges = false;

                foreach (var freshTable in freshTables)
                {
                    var existingTable = db.Tables.FirstOrDefault(t => t.Schema == freshTable.Schema && t.Name == freshTable.Name);

                    if (existingTable == null)
                    {
                        // New table
                        db.Tables.Add(freshTable);
                        newTablesCount++;
                        hasSchemaChanges = true;
                    }
                    else
                    {
                        if (existingTable.RowCount != freshTable.RowCount)
                        {
                            existingTable.RowCount = freshTable.RowCount;
                            hasStatsChanges = true;
                        }

                        // Check for new or updated columns
                        foreach (var freshColumn in freshTable.Columns)
                        {
                            var existingColumn = existingTable.Columns.FirstOrDefault(c => c.Name == freshColumn.Name);
                            if (existingColumn == null)
                            {
                                existingTable.Columns.Add(freshColumn);
                                newColumnsCount++;
                                hasSchemaChanges = true;
                            }
                            else
                            {
                                if (existingColumn.IsPrimaryKey != freshColumn.IsPrimaryKey ||
                                    existingColumn.IsForeignKey != freshColumn.IsForeignKey ||
                                    existingColumn.DataType != freshColumn.DataType ||
                                    existingColumn.IsNullable != freshColumn.IsNullable)
                                {
                                    existingColumn.IsPrimaryKey = freshColumn.IsPrimaryKey;
                                    existingColumn.IsForeignKey = freshColumn.IsForeignKey;
                                    existingColumn.DataType = freshColumn.DataType;
                                    existingColumn.IsNullable = freshColumn.IsNullable;
                                    updatedColumnsCount++;
                                    hasSchemaChanges = true;
                                }
                            }
                        }
                        
                        if (existingTable.IsFactTable != freshTable.IsFactTable)
                        {
                            existingTable.IsFactTable = freshTable.IsFactTable;
                            hasSchemaChanges = true;
                        }
                    }
                }

                if (hasSchemaChanges)
                {
                    // Audit Log
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Action = "Update",
                        EntityType = "CatalogDatabase",
                        EntityId = db.DatabaseName,
                        Username = User.Identity?.Name ?? "System",
                        NewValue = $"{newTablesCount} novas tabelas, {newColumnsCount} novas colunas, {updatedColumnsCount} colunas atualizadas."
                    });
                    
                    TempData["SuccessMessage"] = $"Atualização concluída: {newTablesCount} novas tabelas, {newColumnsCount} novas colunas, {updatedColumnsCount} colunas atualizadas.";
                }

                if (hasSchemaChanges || hasStatsChanges)
                {
                    db.LastUpdated = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    if (!hasSchemaChanges && hasStatsChanges)
                    {
                         TempData["InfoMessage"] = "Estatísticas atualizadas com sucesso.";
                    }
                }
                else
                {
                    TempData["InfoMessage"] = "Nenhuma alteração detectada.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Não foi possível conectar à base de dados para atualização.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { databases = new List<object>(), tables = new List<object>(), columns = new List<object>() });
            }

            var term = query.Trim();

            var databases = await _context.CatalogDatabases
                .Where(d => d.DatabaseName.Contains(term) || d.Server.Contains(term))
                .Select(d => new { id = d.Id, name = d.DatabaseName, server = d.Server, type = "Database" })
                .Take(5)
                .ToListAsync();

            var tables = await _context.Tables
                .Include(t => t.CatalogDatabase)
                .Where(t => t.Name.Contains(term) || (t.Alias != null && t.Alias.Contains(term)) || (t.Description != null && t.Description.Contains(term)))
                .Select(t => new { id = t.Id, name = t.Name, schema = t.Schema, databaseName = t.CatalogDatabase.DatabaseName, type = "Table" })
                .Take(10)
                .ToListAsync();

            var columns = await _context.Columns
                .Include(c => c.Table)
                .ThenInclude(t => t.CatalogDatabase)
                .Where(c => c.Name.Contains(term) || (c.Alias != null && c.Alias.Contains(term)) || (c.Description != null && c.Description.Contains(term)))
                .Select(c => new { id = c.TableId, tableName = c.Table.Name, columnName = c.Name, databaseName = c.Table.CatalogDatabase.DatabaseName, type = "Column" })
                .Take(10)
                .ToListAsync();

            return Json(new { databases, tables, columns });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var db = await _context.CatalogDatabases
                .Include(d => d.Tables)
                .ThenInclude(t => t.Columns)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (db == null) return NotFound();

            return View(db);
        }

        public async Task<IActionResult> TableDetails(int? id)
        {
            if (id == null) return NotFound();

            var table = await _context.Tables
                .Include(t => t.Columns)
                    .ThenInclude(c => c.Comments)
                .Include(t => t.Columns)
                    .ThenInclude(c => c.Tags)
                .Include(t => t.CatalogDatabase)
                .Include(t => t.Comments)
                .Include(t => t.Tags)
                .Include(t => t.Domain)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (table == null) return NotFound();

            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> AddTableComment(int tableId, string content, CommentType type)
        {
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction(nameof(TableDetails), new { id = tableId });

            var table = await _context.Tables.FindAsync(tableId);
            if (table == null) return NotFound();

            var comment = new TableComment
            {
                TableId = tableId,
                Content = content,
                Type = type,
                Author = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            _context.TableComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TableDetails), new { id = tableId });
        }

        [HttpPost]
        public async Task<IActionResult> EditTableComment(int commentId, string content, CommentType type)
        {
            var comment = await _context.TableComments.FindAsync(commentId);
            if (comment == null) return NotFound();

            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction(nameof(TableDetails), new { id = comment.TableId });

            // Optional: Check if user is author or admin
            // if (comment.Author != User.Identity?.Name && !User.IsInRole("Admin")) return Forbid();

            comment.Content = content;
            comment.Type = type;
            // comment.CreatedAt = DateTime.Now; // Keep original date or add UpdatedAt? Keeping original for now.
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TableDetails), new { id = comment.TableId });
        }

        [HttpPost]
        public async Task<IActionResult> AddColumnComment(int columnId, string content, CommentType type)
        {
            if (string.IsNullOrWhiteSpace(content)) 
            {
                 var col = await _context.Columns.FindAsync(columnId);
                 return RedirectToAction(nameof(TableDetails), new { id = col?.TableId });
            }

            var column = await _context.Columns.Include(c => c.Table).FirstOrDefaultAsync(c => c.Id == columnId);
            if (column == null) return NotFound();

            var comment = new ColumnComment
            {
                ColumnId = columnId,
                Content = content,
                Type = type,
                Author = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            _context.ColumnComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TableDetails), new { id = column.TableId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTableTag(int tableId, string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return RedirectToAction(nameof(TableDetails), new { id = tableId });

            var table = await _context.Tables.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == tableId);
            if (table == null) return NotFound();

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new CatalogoBCV.Models.Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync(); // Save to get Id
            }

            if (!table.Tags.Any(t => t.Id == tag.Id))
            {
                table.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(TableDetails), new { id = tableId });
        }

        [HttpPost]
        public async Task<IActionResult> AddColumnTag(int columnId, string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) 
            {
                 var col = await _context.Columns.FindAsync(columnId);
                 return RedirectToAction(nameof(TableDetails), new { id = col?.TableId });
            }

            var column = await _context.Columns.Include(c => c.Tags).Include(c => c.Table).FirstOrDefaultAsync(c => c.Id == columnId);
            if (column == null) return NotFound();

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new CatalogoBCV.Models.Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            if (!column.Tags.Any(t => t.Id == tag.Id))
            {
                column.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(TableDetails), new { id = column.TableId });
        }
    }
}
