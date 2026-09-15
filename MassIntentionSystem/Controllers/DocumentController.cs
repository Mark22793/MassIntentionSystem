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
        public async Task<IActionResult> GenerateDoc(DateTime massDate, TimeSpan massTime, int priestId)
        {
            if (priestId == 0)
            {
                ModelState.AddModelError("", "Paki-pili ang Paring magmisa.");
                ViewBag.Priests = new SelectList(await _context.Priests.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
                return View("Index");
            }

            // Tawagin ang Service para i-build ang Word document file
            byte[] fileBytes = await _documentService.GenerateMassIntentionDocAsync(massDate, massTime, priestId);

            string fileName = $"Mass_Intentions_{massDate:yyyyMMdd}_{massTime.Hours:D2}{massTime.Minutes:D2}.doc";

            return File(fileBytes, "application/msword", fileName);
        }
    }
}