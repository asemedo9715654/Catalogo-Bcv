using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CatalogoBCV.Controllers
{
    [Authorize(Policy = "CanManageUsers")]
    public class UsersController : Controller
    {
        private readonly CatalogContext _context;

        public UsersController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_context.Roles.OrderBy(r => r.Name), "Id", "Name");
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
            ViewBag.Roles = new SelectList(_context.Roles.OrderBy(r => r.Name), "Id", "Name");
            return View(user);
        }
    }
}
