using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            ViewData["DomainId"] = new SelectList(_context.Domains, "Id", "Name", table.DomainId);
            ViewData["SourceSystemId"] = new SelectList(_context.SourceSystems.Where(s => s.IsActive), "Id", "Name", table.SourceSystemId);
            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTable(int id, [Bind("Id,Description,Alias,DomainId,Status,IsFactTable,Owner,DataCustodian,DataSteward,ConfidentialityLevel,AffectedReports,DependentDashboards,LoadFrequency,LastSuccessfulLoad,ValidationRules,SourceSystemId")] Table tableDto)
        {
            if (id != tableDto.Id) return NotFound();

            var table = await _context.Tables.Include(t => t.CatalogDatabase).FirstOrDefaultAsync(t => t.Id == id);
            if (table == null) return NotFound();

            // Guardar valores antigos para auditoria
            var oldDescription = table.Description;
            var oldAlias = table.Alias;
            var oldDomainId = table.DomainId;
            var oldStatus = table.Status;
            var oldIsFact = table.IsFactTable;
            var oldLoadFrequency = table.LoadFrequency;
            var oldLastSuccessfulLoad = table.LastSuccessfulLoad;
            var oldValidationRules = table.ValidationRules;
            var oldSourceSystemId = table.SourceSystemId;

            // Atualizar
            table.Description = tableDto.Description;
            table.Alias = tableDto.Alias;
            table.DomainId = tableDto.DomainId;
            table.Status = tableDto.Status;
            table.IsFactTable = tableDto.IsFactTable;
            table.SourceSystemId = tableDto.SourceSystemId;

            // New fields
            table.Owner = tableDto.Owner;
            table.DataCustodian = tableDto.DataCustodian;
            table.DataSteward = tableDto.DataSteward;
            table.ConfidentialityLevel = tableDto.ConfidentialityLevel;
            table.AffectedReports = tableDto.AffectedReports;
            table.DependentDashboards = tableDto.DependentDashboards;
            
            // Data Quality
            table.LoadFrequency = tableDto.LoadFrequency;
            table.LastSuccessfulLoad = tableDto.LastSuccessfulLoad;
            table.ValidationRules = tableDto.ValidationRules;

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

            if (oldDomainId != table.DomainId)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateDomain",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldDomainId?.ToString(),
                    NewValue = table.DomainId?.ToString(),
                    Username = User.Identity?.Name ?? "System"
                });
            }

            if (oldStatus != table.Status)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateStatus",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldStatus.ToString(),
                    NewValue = table.Status.ToString(),
                    Username = User.Identity?.Name ?? "System"
                });
            }
            
            if (oldIsFact != table.IsFactTable)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateIsFactTable",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldIsFact ? "True" : "False",
                    NewValue = table.IsFactTable ? "True" : "False",
                    Username = User.Identity?.Name ?? "System"
                });
            }

            if (oldLoadFrequency != table.LoadFrequency)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateLoadFrequency",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldLoadFrequency,
                    NewValue = table.LoadFrequency,
                    Username = User.Identity?.Name ?? "System"
                });
            }

            if (oldLastSuccessfulLoad != table.LastSuccessfulLoad)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateLastSuccessfulLoad",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldLastSuccessfulLoad?.ToString("g"),
                    NewValue = table.LastSuccessfulLoad?.ToString("g"),
                    Username = User.Identity?.Name ?? "System"
                });
            }

            if (oldValidationRules != table.ValidationRules)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateValidationRules",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldValidationRules,
                    NewValue = table.ValidationRules,
                    Username = User.Identity?.Name ?? "System"
                });
            }

            if (oldSourceSystemId != table.SourceSystemId)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateSourceSystem",
                    EntityType = "Table",
                    EntityId = table.Id.ToString(),
                    OldValue = oldSourceSystemId?.ToString(),
                    NewValue = table.SourceSystemId?.ToString(),
                    Username = User.Identity?.Name ?? "System"
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Tabela atualizada com sucesso!";
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
        public async Task<IActionResult> EditColumn(int id, [Bind("Id,Description,Alias,NullPercentage")] Column columnDto)
        {
            if (id != columnDto.Id) return NotFound();

            var column = await _context.Columns.Include(c => c.Table).FirstOrDefaultAsync(c => c.Id == id);
            if (column == null) return NotFound();

            // Guardar valores antigos para auditoria
            var oldDescription = column.Description;
            var oldAlias = column.Alias;
            var oldNullPercentage = column.NullPercentage;

            // Atualizar
            column.Description = columnDto.Description;
            column.Alias = columnDto.Alias;
            column.NullPercentage = columnDto.NullPercentage;

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

            if (oldNullPercentage != column.NullPercentage)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "UpdateNullPercentage",
                    EntityType = "Column",
                    EntityId = column.Id.ToString(),
                    OldValue = oldNullPercentage?.ToString(),
                    NewValue = column.NullPercentage?.ToString(),
                    Username = User.Identity?.Name ?? "System"
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Coluna atualizada com sucesso!";
            return RedirectToAction("TableDetails", "Catalog", new { id = column.TableId });
        }
    }
}
