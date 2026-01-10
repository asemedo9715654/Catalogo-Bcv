using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    [Authorize]
    public class DomainsController : Controller
    {
        private readonly CatalogContext _context;

        public DomainsController(CatalogContext context)
        {
            _context = context;
        }

        // GET: Domains
        public async Task<IActionResult> Index()
        {
            return View(await _context.Domains.Include(d => d.Tables).ToListAsync());
        }

        // GET: Domains/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Domains
                .Include(d => d.Tables)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domain == null)
            {
                return NotFound();
            }

            return View(domain);
        }

        // GET: Domains/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Domains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Domain domain)
        {
            if (ModelState.IsValid)
            {
                _context.Add(domain);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(domain);
        }

        // GET: Domains/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
            {
                return NotFound();
            }
            return View(domain);
        }

        // POST: Domains/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Domain domain)
        {
            if (id != domain.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(domain);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DomainExists(domain.Id))
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
            return View(domain);
        }

        // GET: Domains/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Domains
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domain == null)
            {
                return NotFound();
            }

            return View(domain);
        }

        // POST: Domains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain != null)
            {
                _context.Domains.Remove(domain);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DomainExists(int id)
        {
            return _context.Domains.Any(e => e.Id == id);
        }
    }
}
