using System.Security.Claims;
using CatalogoBCV.Data;
using CatalogoBCV.Models;
using CatalogoBCV.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    public class AccountController : Controller
    {
        private readonly CatalogContext _context;
        private readonly IPasswordService _passwordService;

        public AccountController(CatalogContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
            
            bool isValid = false;
            string role = "Reader";

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.PasswordSalt))
                {
                    // Legacy: Plain text check
                    if (user.PasswordHash == password) 
                    {
                        isValid = true;
                        role = user.Role?.Name ?? "Reader";
                    }
                }
                else
                {
                    // New: Hashed check
                    var salt = Convert.FromBase64String(user.PasswordSalt);
                    if (_passwordService.VerifyPassword(password, user.PasswordHash, salt))
                    {
                        isValid = true;
                        role = user.Role?.Name ?? "Reader";
                    }
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
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, username));
                claims.Add(new Claim(ClaimTypes.Role, role));

                var roleEntity = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Name == role);

                if (roleEntity != null)
                {
                    foreach (var rolePermission in roleEntity.RolePermissions)
                    {
                        if (rolePermission.Permission != null)
                        {
                            claims.Add(new Claim("permission", rolePermission.Permission.Name));
                        }
                    }
                }

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

                TempData["Success"] = $"Bem-vindo, {username}!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Credenciais inválidas");
            TempData["Error"] = "Credenciais inválidas";
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
            TempData["Success"] = "Sessão terminada com sucesso.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
