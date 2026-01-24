using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoBCV.Controllers
{
    [Authorize(Policy = "CanViewCatalog")]
    public class BusinessTermsController : Controller
    {
        private readonly CatalogContext _context;

        public BusinessTermsController(CatalogContext context)
        {
            _context = context;
        }

        // GET: BusinessTerms
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.BusinessTerms.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.Term.Contains(searchString) || t.Definition.Contains(searchString));
            }

            return View(await query.OrderBy(t => t.Term).ToListAsync());
        }

        // GET: BusinessTerms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessTerm = await _context.BusinessTerms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (businessTerm == null)
            {
                return NotFound();
            }

            return View(businessTerm);
        }

        // GET: BusinessTerms/Create
        [Authorize(Policy = "CanEditCatalog")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: BusinessTerms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> Create([Bind("Id,Term,Definition,Owner,Status")] BusinessTerm businessTerm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(businessTerm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(businessTerm);
        }

        // GET: BusinessTerms/Edit/5
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessTerm = await _context.BusinessTerms.FindAsync(id);
            if (businessTerm == null)
            {
                return NotFound();
            }
            return View(businessTerm);
        }

        // POST: BusinessTerms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Term,Definition,Owner,Status")] BusinessTerm businessTerm)
        {
            if (id != businessTerm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve original creation data if not tracked, though BaseEntity handling in Context usually covers it if attached correctly.
                    // But here we are attaching a detached entity.
                    // Better approach: fetch existing, update properties.
                    var existing = await _context.BusinessTerms.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Term = businessTerm.Term;
                    existing.Definition = businessTerm.Definition;
                    existing.Owner = businessTerm.Owner;
                    existing.Status = businessTerm.Status;
                    
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessTermExists(businessTerm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(businessTerm);
        }

        // GET: BusinessTerms/Delete/5
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessTerm = await _context.BusinessTerms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (businessTerm == null)
            {
                return NotFound();
            }

            return View(businessTerm);
        }

        // POST: BusinessTerms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var businessTerm = await _context.BusinessTerms.FindAsync(id);
            if (businessTerm != null)
            {
                _context.BusinessTerms.Remove(businessTerm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusinessTermExists(int id)
        {
            return _context.BusinessTerms.Any(e => e.Id == id);
        }
    }
}
