using System.Collections.Generic;
using System.Linq;
using CatalogoBCV.Data;
using CatalogoBCV.Models;
using CatalogoBCV.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    [Authorize(Policy = "CanManageRoles")]
    public class RolesController : Controller
    {
        private readonly CatalogContext _context;

        public RolesController(CatalogContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userCounts = await _context.Users
                .Where(u => u.IsActive)
                .GroupBy(u => u.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count);

            ViewBag.UserCounts = userCounts;

            var roles = await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(roles);
        }

        public async Task<IActionResult> Permissions(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            var allPermissions = await _context.Permissions
                .OrderBy(p => p.Name)
                .ToListAsync();

            var viewModel = new RolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permissions = allPermissions
                    .Select(p => new PermissionItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Assigned = role.RolePermissions.Any(rp => rp.PermissionId == p.Id)
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(int roleId, int[] selectedPermissionIds)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return NotFound();
            }

            var selectedIds = selectedPermissionIds != null
                ? new HashSet<int>(selectedPermissionIds)
                : new HashSet<int>();

            var toRemove = role.RolePermissions
                .Where(rp => !selectedIds.Contains(rp.PermissionId))
                .ToList();

            foreach (var rolePermission in toRemove)
            {
                _context.RolePermissions.Remove(rolePermission);
            }

            var existingIds = role.RolePermissions
                .Select(rp => rp.PermissionId)
                .ToHashSet();

            var toAdd = selectedIds
                .Where(id => !existingIds.Contains(id))
                .ToList();

            foreach (var permissionId in toAdd)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Permissões atualizadas com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (!ModelState.IsValid)
            {
                return View(role);
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Role criada com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            try
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role atualizada com sucesso.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Roles.AnyAsync(r => r.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                bool isInUse = await _context.Users.AnyAsync(u => u.RoleId == id);
                if (isInUse)
                {
                    TempData["Error"] = "Não é possível remover um role em uso por utilizadores.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role removida com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

