using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;

namespace MassIntentionSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Report/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Report/Intentions
        public async Task<IActionResult> Intentions(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.MassIntentions
                .Include(i => i.Payment)
                .Include(i => i.MassSchedule)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(i => i.MassDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.MassDate <= endDate.Value);

            var list = await query.OrderByDescending(i => i.MassDate).ToListAsync();
            return View(list);
        }

        // GET: /Report/Payments
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                .Include(p => p.MassIntention)
                .Where(p => p.Status == "Approved" || p.Status == "Verified")
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.TotalCollection = payments.Sum(p => p.Amount);
            return View(payments);
        }
    }
}