using CatalogoBCV.Data;
using CatalogoBCV.Models;
using CatalogoBCV.Models.ViewModels;
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

        public async Task<IActionResult> Metrics()
        {
            var viewModel = new UserMetricsViewModel();

            // 1. Total Logins per User
            var loginsPerUser = await _context.AuditLogs
                .Where(a => a.Action == "Login")
                .GroupBy(a => a.Username)
                .Select(g => new UserLoginMetric { Username = g.Key, TotalLogins = g.Count() })
                .ToListAsync();
            viewModel.UserLogins = loginsPerUser;

            // 2. Total Logins Last 30 Days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).Date;
            var loginsLast30Days = await _context.AuditLogs
                .Where(a => a.Action == "Login" && a.Timestamp >= thirtyDaysAgo)
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new DailyLoginMetric { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Fill in missing days with 0
            var fullLast30Days = new List<DailyLoginMetric>();
            for (int i = 0; i <= 30; i++)
            {
                var date = thirtyDaysAgo.AddDays(i);
                if (date > DateTime.UtcNow.Date) break;
                
                var existing = loginsLast30Days.FirstOrDefault(l => l.Date == date);
                fullLast30Days.Add(existing ?? new DailyLoginMetric { Date = date, Count = 0 });
            }
            viewModel.Last30DaysLogins = fullLast30Days;

            // 3. Usage Time per User (Estimated)
            // Strategy: Fetch all logs (projection to minimal fields), group by User and Date in memory
            var allActivity = await _context.AuditLogs
                .Select(a => new { a.Username, a.Timestamp })
                .ToListAsync();

            var usage = allActivity
                .GroupBy(a => new { a.Username, Date = a.Timestamp.Date })
                .Select(g => new
                {
                    Username = g.Key.Username,
                    Date = g.Key.Date,
                    Duration = (g.Max(x => x.Timestamp) - g.Min(x => x.Timestamp)).TotalHours
                })
                .GroupBy(x => x.Username)
                .Select(g => new UserUsageMetric
                {
                    Username = g.Key,
                    TotalHours = Math.Round(g.Sum(x => x.Duration), 2)
                })
                .ToList();

            viewModel.UserUsage = usage;

            // 4. Daily Usage Last 30 Days
            var dailyUsage = allActivity
                .Where(a => a.Timestamp >= thirtyDaysAgo)
                .GroupBy(a => new { a.Username, Date = a.Timestamp.Date })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Duration = (g.Max(x => x.Timestamp) - g.Min(x => x.Timestamp)).TotalHours
                })
                .GroupBy(x => x.Date)
                .Select(g => new DailyUsageMetric
                {
                    Date = g.Key,
                    TotalHours = Math.Round(g.Sum(x => x.Duration), 2)
                })
                .ToList();

            var fullLast30DaysUsage = new List<DailyUsageMetric>();
            for (int i = 0; i <= 30; i++)
            {
                var date = thirtyDaysAgo.AddDays(i);
                if (date > DateTime.UtcNow.Date) break;

                var existing = dailyUsage.FirstOrDefault(l => l.Date == date);
                fullLast30DaysUsage.Add(existing ?? new DailyUsageMetric { Date = date, TotalHours = 0 });
            }
            viewModel.Last30DaysUsage = fullLast30DaysUsage;

            return View(viewModel);
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
