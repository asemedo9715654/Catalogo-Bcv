using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace CatalogoBCV.Controllers
{
    [Authorize(Policy = "CanManageSettings")]
    public class SettingsController : Controller
    {
        private readonly CatalogContext _context;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(CatalogContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLogo(IFormFile logoFile)
        {
            if (logoFile == null || logoFile.Length == 0)
            {
                TempData["Error"] = "Por favor, selecione um arquivo de imagem válido.";
                return RedirectToAction(nameof(Index));
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico" };
            var extension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Formato de arquivo não suportado. Use PNG, JPG, GIF, SVG ou ICO.";
                return RedirectToAction(nameof(Index));
            }

            // Create unique filename to avoid caching issues
            var fileName = $"custom_logo_{DateTime.Now.Ticks}{extension}";
            var uploadDir = Path.Combine(_environment.WebRootPath, "images");
            
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(stream);
            }

            // Update setting
            var setting = await _context.SystemSettings.FindAsync("LogoPath");
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = "LogoPath",
                    Value = $"/images/{fileName}",
                    Description = "Caminho do arquivo de logo da aplicação",
                    Group = "Visual",
                    Type = "string"
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                // Delete old custom logo if it exists and is not one of the defaults
                if (!string.IsNullOrEmpty(setting.Value) && 
                    !setting.Value.Equals("/images/logo.png", StringComparison.OrdinalIgnoreCase) && 
                    !setting.Value.Equals("/images/logo1.png", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var oldFileName = Path.GetFileName(setting.Value);
                        var oldFilePath = Path.Combine(uploadDir, oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch
                    {
                        // Ignore errors deleting old file
                    }
                }
                
                setting.Value = $"/images/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Logo atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
