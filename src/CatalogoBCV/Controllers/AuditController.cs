using CatalogoBCV.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoBCV.Controllers
{
    [Authorize]
    public class AuditController : Controller
    {
        private readonly CatalogContext _context;

        public AuditController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.AuditLogs.OrderByDescending(a => a.Timestamp).ToListAsync());
        }
    }
}
