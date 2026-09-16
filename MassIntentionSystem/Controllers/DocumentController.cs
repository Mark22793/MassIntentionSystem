using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Services;

namespace MassIntentionSystem.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DocumentService _documentService;

        public DocumentController(ApplicationDbContext context, DocumentService documentService)
        {
            _context = context;
            _documentService = documentService;
        }

        // GET: /Document/
        public async Task<IActionResult> Index()
        {
            ViewBag.Priests = new SelectList(await _context.Priests.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Document/GenerateDoc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDoc(DateTime massDate, List<string> selectedTimes, List<int> priestIds)
        {
            if (selectedTimes == null || !selectedTimes.Any())
            {
                ModelState.AddModelError("", "Paki-check ng kahit isang Oras ng Misa.");
                ViewBag.Priests = new SelectList(await _context.Priests.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
                return View("Index");
            }

            var scheduleMap = new Dictionary<TimeSpan, int>();
            var allTimes = new List<string> { "06:00:00", "07:30:00", "09:00:00", "10:30:00", "12:00:00", "16:00:00", "17:30:00", "19:00:00" };

            for (int i = 0; i < allTimes.Count; i++)
            {
                string timeStr = allTimes[i];
                if (selectedTimes.Contains(timeStr))
                {
                    TimeSpan ts = TimeSpan.Parse(timeStr);
                    int pId = (priestIds != null && priestIds.Count > i) ? priestIds[i] : 0;
                    scheduleMap[ts] = pId;
                }
            }

            byte[] fileBytes = await _documentService.GenerateMultiTimeMassDocAsync(massDate, scheduleMap);
            string fileName = $"Mass_Intentions_{massDate:yyyyMMdd}.doc";

            return File(fileBytes, "application/msword", fileName);
        }

        // GET: /Document/History
        public async Task<IActionResult> History(DateTime? filterDate)
        {
            var dateToQuery = filterDate ?? DateTime.Today;

            var printedIntentions = await _context.MassIntentions
                .Where(m => m.IsPrinted && m.MassDate.Date == dateToQuery.Date)
                .OrderByDescending(m => m.PrintedAt)
                .ThenBy(m => m.MassTime)
                .ToListAsync();

            ViewBag.SelectedDate = dateToQuery.ToString("yyyy-MM-dd");
            return View(printedIntentions);
        }
    }
}