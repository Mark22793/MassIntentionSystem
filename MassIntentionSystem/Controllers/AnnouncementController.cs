using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;

namespace MassIntentionSystem.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Announcement/
        public async Task<IActionResult> Index()
        {
            var list = await _context.Announcements.OrderByDescending(a => a.DatePosted).ToListAsync();
            return View(list);
        }

        // GET: /Announcement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.DatePosted = DateTime.Now;
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }
    }
}