using CatalogoBCV.Data;
using CatalogoBCV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Controllers
{
    [Authorize(Policy = "CanViewCatalog")]
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
                .Include(d => d.SubDomains)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domain == null)
            {
                return NotFound();
            }

            return View(domain);
        }

        // GET: Domains/Create
        [Authorize(Policy = "CanEditCatalog")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Domains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Domain domain)
        {
            if (ModelState.IsValid)
            {
                _context.Add(domain);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Domínio criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(domain);
        }

        // GET: Domains/Edit/5
        [Authorize(Policy = "CanEditCatalog")]
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
        [Authorize(Policy = "CanEditCatalog")]
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
                    TempData["Success"] = "Domínio atualizado com sucesso!";
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
        [Authorize(Policy = "CanEditCatalog")]
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
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain != null)
            {
                _context.Domains.Remove(domain);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Domínio removido com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DomainExists(int id)
        {
            return _context.Domains.Any(e => e.Id == id);
        }

        // --- SubDomains Actions ---

        // GET: Domains/SubDomainDetails/5
        public async Task<IActionResult> SubDomainDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subDomain = await _context.SubDomains
                .Include(s => s.Domain)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subDomain == null)
            {
                return NotFound();
            }

            return View(subDomain);
        }

        // GET: Domains/CreateSubDomain
        [Authorize(Policy = "CanEditCatalog")]
        public IActionResult CreateSubDomain(int? domainId)
        {
            ViewData["DomainId"] = new SelectList(_context.Domains, "Id", "Name", domainId);
            return View();
        }

        // POST: Domains/CreateSubDomain
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> CreateSubDomain([Bind("Id,Name,Description,DomainId")] SubDomain subDomain)
        {
            // Remove Domain from validation as it is a required navigation property but not bound here
            ModelState.Remove("Domain");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(subDomain);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }

                    TempData["Success"] = "Subdomínio criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = subDomain.DomainId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocorreu um erro ao salvar: " + ex.Message);
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateSubDomain", subDomain);
            }

            ViewData["DomainId"] = new SelectList(_context.Domains, "Id", "Name", subDomain.DomainId);
            return View(subDomain);
        }

        // GET: Domains/EditSubDomain/5
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> EditSubDomain(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subDomain = await _context.SubDomains.FindAsync(id);
            if (subDomain == null)
            {
                return NotFound();
            }
            ViewData["DomainId"] = new SelectList(_context.Domains, "Id", "Name", subDomain.DomainId);
            return View(subDomain);
        }

        // POST: Domains/EditSubDomain/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> EditSubDomain(int id, [Bind("Id,Name,Description,DomainId")] SubDomain subDomain)
        {
            if (id != subDomain.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Domain");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subDomain);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Subdomínio atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubDomainExists(subDomain.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = subDomain.DomainId });
            }
            ViewData["DomainId"] = new SelectList(_context.Domains, "Id", "Name", subDomain.DomainId);
            return View(subDomain);
        }

        // GET: Domains/DeleteSubDomain/5
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> DeleteSubDomain(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subDomain = await _context.SubDomains
                .Include(s => s.Domain)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subDomain == null)
            {
                return NotFound();
            }

            return View(subDomain);
        }

        // POST: Domains/DeleteSubDomain/5
        [HttpPost, ActionName("DeleteSubDomain")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditCatalog")]
        public async Task<IActionResult> DeleteSubDomainConfirmed(int id)
        {
            var subDomain = await _context.SubDomains.FindAsync(id);
            if (subDomain != null)
            {
                var domainId = subDomain.DomainId;
                _context.SubDomains.Remove(subDomain);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subdomínio removido com sucesso!";
                return RedirectToAction(nameof(Details), new { id = domainId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SubDomainExists(int id)
        {
            return _context.SubDomains.Any(e => e.Id == id);
        }
    }
}
