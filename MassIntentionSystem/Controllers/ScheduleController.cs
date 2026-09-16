// Controllers/ScheduleController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;

namespace MassIntentionSystem.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task PopulatePriestsAsync(object? selected = null)
        {
            ViewBag.PriestId = new SelectList(
                await _context.Priests.Where(p => p.IsActive).ToListAsync(), "Id", "Name", selected);
        }

        // GET: /Schedule/
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.MassSchedules
                .Include(s => s.Priest)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.Time)
                .ToListAsync();
            return View(schedules);
        }

        // GET: /Schedule/Create
        public async Task<IActionResult> Create()
        {
            await PopulatePriestsAsync();
            return View();
        }

        // POST: /Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MassSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.MassSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Naidagdag ang bagong Mass Schedule slot.";
                return RedirectToAction(nameof(Index));
            }
            await PopulatePriestsAsync(schedule.PriestId);
            return View(schedule);
        }

        // GET: /Schedule/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.MassSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            await PopulatePriestsAsync(schedule.PriestId);
            return View(schedule);
        }

        // POST: /Schedule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MassSchedule schedule)
        {
            if (id != schedule.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(schedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Na-update ang Mass Schedule.";
                return RedirectToAction(nameof(Index));
            }
            await PopulatePriestsAsync(schedule.PriestId);
            return View(schedule);
        }

        // GET: /Schedule/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var schedule = await _context.MassSchedules
                .Include(s => s.Priest)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null) return NotFound();
            return View(schedule);
        }
    }
}