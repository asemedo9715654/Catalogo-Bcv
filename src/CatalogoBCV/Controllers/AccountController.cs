using System.Security.Claims;
using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    public class AccountController : Controller
    {
        private readonly CatalogContext _context;

        public AccountController(CatalogContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            bool isValid = false;
            string role = "Reader";

            if (user != null)
            {
                // TODO: Usar hash real em produção
                if (user.PasswordHash == password) 
                {
                    isValid = true;
                    role = user.Role.ToString();
                }
            }
            else if (username == "admin" && password == "admin")
            {
                // Permitir admin/admin se não houver utilizadores na base
                if (!await _context.Users.AnyAsync())
                {
                    isValid = true;
                    role = "Admin";
                }
            }

            if (isValid)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Audit Login
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "Login",
                    EntityType = "User",
                    EntityId = username,
                    Username = username
                });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Credenciais inválidas");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "Unknown";
             _context.AuditLogs.Add(new AuditLog
            {
                Action = "Logout",
                EntityType = "User",
                EntityId = username,
                Username = username
            });
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
