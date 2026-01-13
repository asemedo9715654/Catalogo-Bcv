using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CatalogoBCV.Data;
using CatalogoBCV.Models;

namespace CatalogoBCV.Controllers
{
    public class SourceSystemsController : Controller
    {
        private readonly CatalogContext _context;

        public SourceSystemsController(CatalogContext context)
        {
            _context = context;
        }

        // GET: SourceSystems
        public async Task<IActionResult> Index()
        {
            return View(await _context.SourceSystems.ToListAsync());
        }

        // GET: SourceSystems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceSystem = await _context.SourceSystems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sourceSystem == null)
            {
                return NotFound();
            }

            return View(sourceSystem);
        }

        // GET: SourceSystems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SourceSystems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive")] SourceSystem sourceSystem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sourceSystem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sourceSystem);
        }

        // GET: SourceSystems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceSystem = await _context.SourceSystems.FindAsync(id);
            if (sourceSystem == null)
            {
                return NotFound();
            }
            return View(sourceSystem);
        }

        // POST: SourceSystems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] SourceSystem sourceSystem)
        {
            if (id != sourceSystem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sourceSystem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SourceSystemExists(sourceSystem.Id))
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
            return View(sourceSystem);
        }

        // GET: SourceSystems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceSystem = await _context.SourceSystems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sourceSystem == null)
            {
                return NotFound();
            }

            return View(sourceSystem);
        }

        // POST: SourceSystems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sourceSystem = await _context.SourceSystems.FindAsync(id);
            if (sourceSystem != null)
            {
                _context.SourceSystems.Remove(sourceSystem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SourceSystemExists(int id)
        {
            return _context.SourceSystems.Any(e => e.Id == id);
        }
    }
}
