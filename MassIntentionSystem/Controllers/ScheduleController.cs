using Microsoft.AspNetCore.Mvc;
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

        // GET: /Schedule/
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.MassSchedules.ToListAsync();
            return View(schedules);
        }

        // GET: /Schedule/Create
        public IActionResult Create()
        {
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
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        // GET: /Schedule/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.MassSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

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
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }
    }
}