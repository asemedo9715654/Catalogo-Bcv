using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly CatalogContext _context;

        public UsersController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // Em produção: Hash password
                _context.Add(user);
                
                // Audit
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "CreateUser",
                    EntityType = "User",
                    EntityId = user.Username,
                    Username = User.Identity?.Name ?? "Admin"
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "Utilizador criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
    }
}
