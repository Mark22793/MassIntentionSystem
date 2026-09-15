using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;

namespace MassIntentionSystem.Controllers
{
    public class PriestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PriestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Priest/
        public async Task<IActionResult> Index()
        {
            var priests = await _context.Priests.ToListAsync();
            return View(priests);
        }

        // GET: /Priest/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Priest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Priest priest)
        {
            if (ModelState.IsValid)
            {
                _context.Priests.Add(priest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(priest);
        }

        // GET: /Priest/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var priest = await _context.Priests.FindAsync(id);
            if (priest == null) return NotFound();

            return View(priest);
        }

        // POST: /Priest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Priest priest)
        {
            if (id != priest.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(priest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(priest);
        }
    }
}