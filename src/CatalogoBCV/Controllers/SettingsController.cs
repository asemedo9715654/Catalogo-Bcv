using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly CatalogContext _context;

        public SettingsController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings
                .OrderBy(s => s.Group)
                .ThenBy(s => s.Key)
                .ToListAsync();
            
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Dictionary<string, string> settings)
        {
            if (settings == null || !settings.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var kvp in settings)
            {
                var setting = await _context.SystemSettings.FindAsync(kvp.Key);
                if (setting != null)
                {
                    setting.Value = kvp.Value;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Configurações atualizadas com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
