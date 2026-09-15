using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;

namespace MassIntentionSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            ViewBag.Announcements = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.DatePosted)
                .Take(3)
                .ToListAsync();

            ViewBag.Schedules = await _context.MassSchedules
                .Where(s => s.IsActive)
                .OrderBy(s => s.Time)
                .ToListAsync();

            return View();
        }

        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/MassSchedule
        public async Task<IActionResult> MassSchedule()
        {
            var schedules = await _context.MassSchedules
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(schedules);
        }

        // GET: /Home/Announcements
        public async Task<IActionResult> Announcements()
        {
            var announcements = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.DatePosted)
                .ToListAsync();

            return View(announcements);
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }
    }
}