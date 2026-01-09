using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoBCV.Controllers
{
    [Authorize]
    public class MetadataController : Controller
    {
        private readonly CatalogContext _context;

        public MetadataController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> EditTable(int? id)
        {
            if (id == null) return NotFound();

            var table = await _context.Tables.Include(t => t.CatalogDatabase).FirstOrDefaultAsync(t => t.Id == id);
            if (table == null) return NotFound();

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTable(int id, [Bind("Id,Description,Alias")] Table tableDto)
        {
            if (id != tableDto.Id) return NotFound();

            var table = await _context.Tables.Include(t => t.CatalogDatabase).FirstOrDefaultAsync(t => t.Id == id);
            if (table == null) return NotFound();

            // Guardar valores antigos para auditoria
            var oldDescription = table.Description;
            var oldAlias = table.Alias;

            // Atualizar
            table.Description = tableDto.Description;
            table.Alias = tableDto.Alias;

            if (oldDescription != table.Description)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateDescription",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldDescription,
                    NewValue = table.Description,
                    Username = User.Identity?.Name ?? "System"
                });
            }

            if (oldAlias != table.Alias)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateAlias",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldAlias,
                    NewValue = table.Alias,
                    Username = User.Identity?.Name ?? "System"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("TableDetails", "Catalog", new { id = table.Id });
        }

        public async Task<IActionResult> EditColumn(int? id)
        {
            if (id == null) return NotFound();

            var column = await _context.Columns.Include(c => c.Table).FirstOrDefaultAsync(c => c.Id == id);
            if (column == null) return NotFound();

            return View(column);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditColumn(int id, [Bind("Id,Description,Alias")] Column columnDto)
        {
            if (id != columnDto.Id) return NotFound();

            var column = await _context.Columns.Include(c => c.Table).FirstOrDefaultAsync(c => c.Id == id);
            if (column == null) return NotFound();

            // Guardar valores antigos para auditoria
            var oldDescription = column.Description;
            var oldAlias = column.Alias;

            // Atualizar
            column.Description = columnDto.Description;
            column.Alias = columnDto.Alias;

            if (oldDescription != column.Description)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateDescription",
                    EntityType = "Column",
                    EntityId = column.Id.ToString(),
                    OldValue = oldDescription,
                    NewValue = column.Description,
                    Username = "User" // TODO: Identity
                });
            }

            if (oldAlias != column.Alias)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateAlias",
                    EntityType = "Column",
                    EntityId = column.Id.ToString(),
                    OldValue = oldAlias,
                    NewValue = column.Alias,
                    Username = "User"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("TableDetails", "Catalog", new { id = column.TableId });
        }
    }
}
