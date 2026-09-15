using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassIntentionSystem.Controllers
{
    public class MassIntentionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MassIntentionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /MassIntention/
        public async Task<IActionResult> Index()
        {
            var intentions = await _context.MassIntentions
                .Include(m => m.Payment)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(intentions);
        }

        // GET: /MassIntention/AdminCreate
        public IActionResult AdminCreate()
        {
            return View();
        }

        // POST: /MassIntention/AdminCreate (WALK-IN CASH SUBMISSION)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCreate(MassIntention model, decimal amountPaid, string rawMassTime)
        {
            ModelState.Clear();

            try
            {
                // Parse ang MassTime mula sa string (hal. "06:00:00" o "06:00 AM")
                string timeInput = !string.IsNullOrEmpty(rawMassTime) ? rawMassTime : Request.Form["MassTime"].ToString();
                if (!string.IsNullOrEmpty(timeInput))
                {
                    if (TimeSpan.TryParse(timeInput, out TimeSpan parsedSpan))
                    {
                        model.MassTime = parsedSpan;
                    }
                    else if (DateTime.TryParse(timeInput, out DateTime parsedDt))
                    {
                        model.MassTime = parsedDt.TimeOfDay;
                    }
                }

                if (amountPaid <= 0 && Request.Form.ContainsKey("amountPaid"))
                {
                    decimal.TryParse(Request.Form["amountPaid"], out amountPaid);
                }

                model.ReferenceNo = "WALK-2026-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                model.CreatedAt = DateTime.Now;
                model.PaymentStatus = "Verified";

                _context.MassIntentions.Add(model);
                await _context.SaveChangesAsync();

                var payment = new Payment
                {
                    MassIntentionId = model.Id,
                    Amount = amountPaid,
                    PaymentMethod = "Cash",
                    Status = "Verified",
                    PaymentDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                TempData["PrintedAmount"] = amountPaid.ToString();

                return RedirectToAction(nameof(PrintReceipt), new { id = model.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Database Error: " + (ex.InnerException?.Message ?? ex.Message);
                return View(model);
            }
        }

        // GET: /MassIntention/PrintReceipt/5
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var intention = await _context.MassIntentions
                .Include(m => m.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (intention == null) return NotFound();

            return View(intention);
        }

        // POST: /MassIntention/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var intention = await _context.MassIntentions
                    .Include(m => m.Payment)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (intention != null)
                {
                    if (intention.Payment != null)
                    {
                        _context.Payments.Remove(intention.Payment);
                    }

                    _context.MassIntentions.Remove(intention);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ang Mass Intention ay matagumpay na nabura.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hindi nahanap ang record.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Nagka-error sa pagbura: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET/POST: Export to Word Document Function (ADVANCED TOKENIZER & MULTI-TAG PARSER)
        public async Task<IActionResult> ExportToWord(DateTime massDate, string massTime)
        {
            TimeSpan targetTime = TimeSpan.Zero;
            bool timeParsed = false;

            if (!string.IsNullOrEmpty(massTime))
            {
                if (TimeSpan.TryParse(massTime, out targetTime))
                {
                    timeParsed = true;
                }
                else if (DateTime.TryParse(massTime, out DateTime parsedDateTime))
                {
                    targetTime = parsedDateTime.TimeOfDay;
                    timeParsed = true;
                }
            }

            var allIntentionsOnDate = await _context.MassIntentions
                .Where(m => m.MassDate.Date == massDate.Date)
                .ToListAsync();

            var intentions = allIntentionsOnDate
                .Where(m => !timeParsed || m.MassTime == targetTime || m.MassTime == TimeSpan.Zero)
                .ToList();

            var healingList = new List<string>();
            var thanksgivingList = new List<string>();
            var eternalList = new List<string>();
            var specialList = new List<string>();

            foreach (var item in intentions)
            {
                string rawOfferings = item.OfferingNames ?? "";
                string defaultCategory = item.Category != null ? item.Category.ToString() : "Other";

                if (string.IsNullOrWhiteSpace(rawOfferings)) continue;

                // 1. I-breakdown ang string gamit ang Lookahead Regex sa bawat '['
                // Halimbawa: "[Thanksgiving] shs [Healing] dsg" -> Gagawing 2 parts: "[Thanksgiving] shs " at "[Healing] dsg"
                var parts = Regex.Split(rawOfferings, @"(?=\[)");

                foreach (var part in parts)
                {
                    string cleanPart = part.Trim();
                    if (string.IsNullOrEmpty(cleanPart)) continue;

                    // Kunin ang tag name at ang intention name
                    var match = Regex.Match(cleanPart, @"^\[(.*?)\]\s*:?\s*(.*)$", RegexOptions.Singleline);

                    if (match.Success)
                    {
                        string tag = match.Groups[1].Value.Trim().ToLower();
                        string rawName = match.Groups[2].Value.Trim();

                        // Tanggalin ang natitirang brackets kung may lumagpas
                        string cleanName = Regex.Replace(rawName, @"\[.*?\]", "").Trim();
                        cleanName = cleanName.TrimStart(':', '-', ' ').Trim();

                        if (string.IsNullOrEmpty(cleanName)) continue;

                        // I-categorize batay sa tag
                        if (tag.Contains("healing") || tag.Contains("health"))
                            healingList.Add(cleanName.ToUpper());
                        else if (tag.Contains("thanksgiving"))
                            thanksgivingList.Add(cleanName.ToUpper());
                        else if (tag.Contains("eternal") || tag.Contains("repose") || tag.Contains("yumao"))
                            eternalList.Add(cleanName.ToUpper());
                        else
                            specialList.Add(cleanName.ToUpper());
                    }
                    else
                    {
                        // Kung walang bracket tag sa part na ito, hatiin sa newlines at gamitin ang Default Category
                        var lines = cleanPart.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            string cleanName = Regex.Replace(line, @"\[.*?\]", "").Trim();
                            cleanName = cleanName.TrimStart(':', '-', ' ').Trim();

                            if (!string.IsNullOrEmpty(cleanName))
                            {
                                string cat = defaultCategory.ToLower();
                                if (cat.Contains("healing") || cat.Contains("health"))
                                    healingList.Add(cleanName.ToUpper());
                                else if (cat.Contains("thanksgiving"))
                                    thanksgivingList.Add(cleanName.ToUpper());
                                else if (cat.Contains("eternal") || cat.Contains("repose") || cat.Contains("yumao"))
                                    eternalList.Add(cleanName.ToUpper());
                                else
                                    specialList.Add(cleanName.ToUpper());
                            }
                        }
                    }
                }
            }

            string BuildListHtml(List<string> items)
            {
                if (!items.Any()) return "<li>NONE</li>";
                var sb = new StringBuilder();
                foreach (var name in items.Distinct())
                {
                    sb.Append($"<li>{name}</li>");
                }
                return sb.ToString();
            }

            string healingHtml = BuildListHtml(healingList);
            string thanksgivingHtml = BuildListHtml(thanksgivingList);
            string eternalHtml = BuildListHtml(eternalList);
            string specialHtml = BuildListHtml(specialList);

            string formattedTime = massTime;
            if (timeParsed)
            {
                formattedTime = DateTime.Today.Add(targetTime).ToString("hh:mm tt");
            }

            string htmlContent = $@"
    <html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>
    <head>
        <meta charset='utf-8'>
        <title>Mass Intentions</title>
        <style>
            @page Section1 {{
                size: 8.5in 11.0in;
                margin: 0.8in 0.8in 0.8in 0.8in;
                mso-page-orientation: portrait;
            }}
            div.Section1 {{ page: Section1; }}
            body {{ font-family: 'Arial', sans-serif; font-size: 10pt; color: #000; text-transform: uppercase; }}
            .header-title {{ text-align: center; font-weight: bold; font-size: 12pt; color: #1e293b; margin-bottom: 2px; text-transform: uppercase; }}
            .header-sub {{ text-align: center; font-weight: bold; font-size: 10pt; margin-bottom: 12px; text-transform: uppercase; }}
            .details {{ text-align: center; font-weight: bold; font-size: 9.5pt; margin-bottom: 15px; border-bottom: 1.5pt solid #444; padding-bottom: 8px; text-transform: uppercase; }}
            .section-title {{ font-weight: bold; font-size: 10pt; margin-top: 14px; margin-bottom: 4px; color: #1e293b; text-transform: uppercase; border-bottom: 1px solid #888; padding-bottom: 2px; }}
            ul {{ margin-top: 4px; margin-bottom: 12px; padding-left: 24px; }}
            li {{ margin-bottom: 3px; font-size: 10pt; text-transform: uppercase; }}
        </style>
    </head>
    <body>
        <div class='Section1'>
            <div class='header-title'>NATIONAL SHRINE OF OUR LADY OF LA NAVAL DE MANILA</div>
            <div class='header-sub'>SANTO DOMINGO CHURCH, QUEZON CITY</div>
            <div class='details'>
                MASS DATE: {massDate:MMMM dd, yyyy} &nbsp;|&nbsp; TIME: {formattedTime}
                <br />
                CELEBRANT / PRIEST: REV. FR. ROLANDO DELA ROSA, OP
            </div>

            <div class='section-title'>I. HEALING & GOOD HEALTH</div>
            <ul>{healingHtml}</ul>

            <div class='section-title'>II. THANKSGIVING</div>
            <ul>{thanksgivingHtml}</ul>

            <div class='section-title'>III. ETERNAL REPOSE (FOR THE FAITHFUL DEPARTED)</div>
            <ul>{eternalHtml}</ul>

            <div class='section-title'>IV. OTHER INTENTIONS & SPECIAL PETITIONS</div>
            <ul>{specialHtml}</ul>
        </div>
    </body>
    </html>";

            byte[] byteArray = Encoding.UTF8.GetBytes(htmlContent);
            string fileName = $"Mass_Intentions_{massDate:yyyyMMdd}_{massTime.Replace(":", "").Replace(" ", "")}.doc";

            return File(byteArray, "application/msword", fileName);
        }
    }
}