using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;

namespace MassIntentionSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard/
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalRequests = await _context.MassIntentions.CountAsync();
            ViewBag.PendingRequests = await _context.MassIntentions.CountAsync(m => m.PaymentStatus == "Pending");
            ViewBag.VerifiedRequests = await _context.MassIntentions.CountAsync(m => m.PaymentStatus == "Verified");
            ViewBag.TodayIntentions = await _context.MassIntentions.CountAsync(m => m.MassDate.Date == DateTime.Today);

            var recentRequests = await _context.MassIntentions
                .Include(m => m.Payment)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(recentRequests);
        }
    }
}