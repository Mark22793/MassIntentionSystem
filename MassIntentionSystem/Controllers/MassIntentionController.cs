using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;
using MassIntentionSystem.Services;

namespace MassIntentionSystem.Controllers
{
    public class MassIntentionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DocumentService _documentService;
        private readonly PaymentService _paymentService;
        private readonly NotificationService _notificationService;

        public MassIntentionController(
            ApplicationDbContext context,
            DocumentService documentService,
            PaymentService paymentService,
            NotificationService notificationService)
        {
            _context = context;
            _documentService = documentService;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        // GET: /MassIntention
        public async Task<IActionResult> Index(string searchString, string status, DateTime? searchDate, int page = 1)
        {
            const int pageSize = 15;

            var query = _context.MassIntentions
                .Include(m => m.Payment)
                .Include(m => m.MassSchedule)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string searchLower = searchString.Trim().ToLower();
                query = query.Where(m => m.ReferenceNo.ToLower().Contains(searchLower) ||
                                         m.RequestorName.ToLower().Contains(searchLower) ||
                                         (m.ContactNumber != null && m.ContactNumber.Contains(searchLower)));
            }

            if (searchDate.HasValue)
            {
                query = query.Where(m => m.MassDate.Date == searchDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Paid")
                {
                    query = query.Where(m => m.PaymentStatus == "Verified" ||
                                             m.PaymentStatus == "Confirmed" ||
                                             m.PaymentStatus == "CONFIRMED / PAID" ||
                                             m.PaymentStatus == "Paid");
                }
                else if (status == "Pending")
                {
                    query = query.Where(m => m.PaymentStatus == "Pending" || string.IsNullOrEmpty(m.PaymentStatus));
                }
                else
                {
                    query = query.Where(m => m.PaymentStatus == status);
                }
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentDate = searchDate?.ToString("yyyy-MM-dd");

            int totalCount = await query.CountAsync();
            int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var intentions = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(intentions);
        }

        // POST: /MassIntention/DeleteAllFiltered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllFiltered(string searchString, string status, DateTime? searchDate)
        {
            try
            {
                var query = _context.MassIntentions
                    .Include(m => m.Payment)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    string searchLower = searchString.Trim().ToLower();
                    query = query.Where(m => m.ReferenceNo.ToLower().Contains(searchLower) ||
                                             m.RequestorName.ToLower().Contains(searchLower) ||
                                             (m.ContactNumber != null && m.ContactNumber.Contains(searchLower)));
                }

                if (searchDate.HasValue)
                {
                    query = query.Where(m => m.MassDate.Date == searchDate.Value.Date);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (status == "Paid")
                    {
                        query = query.Where(m => m.PaymentStatus == "Verified" ||
                                                 m.PaymentStatus == "Confirmed" ||
                                                 m.PaymentStatus == "CONFIRMED / PAID" ||
                                                 m.PaymentStatus == "Paid");
                    }
                    else if (status == "Pending")
                    {
                        query = query.Where(m => m.PaymentStatus == "Pending" || string.IsNullOrEmpty(m.PaymentStatus));
                    }
                    else
                    {
                        query = query.Where(m => m.PaymentStatus == status);
                    }
                }

                var itemsToDelete = await query.ToListAsync();
                int count = itemsToDelete.Count;

                if (count == 0)
                {
                    TempData["ErrorMessage"] = "Walang records na nabura.";
                    return RedirectToAction(nameof(Index), new { searchString, status, searchDate = searchDate?.ToString("yyyy-MM-dd") });
                }

                foreach (var item in itemsToDelete)
                {
                    if (item.Payment != null)
                    {
                        _context.Payments.Remove(item.Payment);
                    }
                    _context.MassIntentions.Remove(item);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Matagumpay na nabura ang {count} na record(s).";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Nagka-error sa pagbura: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /MassIntention/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
            var model = new MassIntention { MassDate = DateTime.Today.AddDays(1) };
            return View(model);
        }

        // POST: /MassIntention/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MassDate,RequestorName,ContactNumber,RequestorEmail,PriestName")] MassIntention model,
            List<string> categories,
            List<string> offeringNamesList,
            string dateMode,
            DateTime? endDate,
            bool allMassesOfDay,
            string rawMassTime,
            decimal amount,
            string paymentMethod,
            IFormFile? paymentProof)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(model.RequestorName) ||
                string.IsNullOrWhiteSpace(model.ContactNumber) ||
                categories == null ||
                !categories.Any() ||
                offeringNamesList == null ||
                !offeringNamesList.Any(n => !string.IsNullOrWhiteSpace(n)))
            {
                ViewBag.Error = "Paki-kumpleto ang lahat ng kinakailangang fields.";
                ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                return View(model);
            }

            try
            {
                var defaultTimes = new List<TimeSpan>
                {
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(7, 30, 0),
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(10, 30, 0),
                    new TimeSpan(12, 0, 0),
                    new TimeSpan(16, 0, 0),
                    new TimeSpan(17, 30, 0),
                    new TimeSpan(19, 0, 0)
                };

                TimeSpan selectedSingleTime = new TimeSpan(6, 0, 0);
                string timeInput = !string.IsNullOrEmpty(rawMassTime) ? rawMassTime : Request.Form["rawMassTime"].ToString();
                if (!string.IsNullOrEmpty(timeInput))
                {
                    if (TimeSpan.TryParse(timeInput, out var parsedSpan))
                        selectedSingleTime = parsedSpan;
                    else if (DateTime.TryParse(timeInput, out var parsedDt))
                        selectedSingleTime = parsedDt.TimeOfDay;
                }

                DateTime start = model.MassDate.Date;
                DateTime end = (dateMode == "range" && endDate.HasValue) ? endDate.Value.Date : start;

                if (end < start)
                {
                    ViewBag.Error = "Ang End Date ay hindi pwedeng mas maaga sa Start Date.";
                    ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                    return View(model);
                }

                string sharedRefNo = _notificationService != null
                    ? _notificationService.GenerateReferenceNumber()
                    : "SD-" + DateTime.Now.ToString("yyyy") + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper();

                var createdIntentions = new List<MassIntention>();

                string? proofPath = null;
                if (paymentProof != null && _paymentService != null)
                {
                    proofPath = await _paymentService.UploadPaymentProofAsync(paymentProof);
                }

                for (int i = 0; i < categories.Count; i++)
                {
                    string categoryString = categories[i];
                    string names = offeringNamesList.Count > i ? offeringNamesList[i] : "";

                    if (string.IsNullOrWhiteSpace(names)) continue;

                    if (!Enum.TryParse<IntentionCategory>(categoryString, true, out var parsedCategory))
                    {
                        parsedCategory = IntentionCategory.Other;
                    }

                    for (DateTime date = start; date <= end; date = date.AddDays(1))
                    {
                        var timesForThisDay = allMassesOfDay ? defaultTimes : new List<TimeSpan> { selectedSingleTime };

                        foreach (var time in timesForThisDay)
                        {
                            var newIntention = new MassIntention
                            {
                                ReferenceNo = sharedRefNo,
                                RequestorName = model.RequestorName,
                                ContactNumber = model.ContactNumber,
                                RequestorEmail = model.RequestorEmail,
                                PriestName = model.PriestName,
                                Category = parsedCategory,
                                OfferingNames = names,
                                MassDate = date,
                                MassTime = time,
                                CreatedAt = DateTime.Now,
                                PaymentStatus = "Pending",
                                IsAdminEncoded = false
                            };

                            _context.MassIntentions.Add(newIntention);
                            await _context.SaveChangesAsync();

                            if (amount > 0 && createdIntentions.Count == 0)
                            {
                                var payment = new Payment
                                {
                                    MassIntentionId = newIntention.Id,
                                    Amount = amount,
                                    PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Unspecified" : paymentMethod,
                                    Status = "Pending",
                                    ProofOfPaymentPath = proofPath,
                                    PaymentDate = DateTime.Now
                                };

                                _context.Payments.Add(payment);
                                await _context.SaveChangesAsync();

                                newIntention.PaymentId = payment.Id;
                                await _context.SaveChangesAsync();
                            }

                            createdIntentions.Add(newIntention);
                        }
                    }
                }

                if (createdIntentions.Any())
                {
                    try
                    {
                        if (_notificationService != null)
                        {
                            await _notificationService.SendConfirmationAsync(createdIntentions.First());
                        }
                    }
                    catch
                    {
                        // Fallback
                    }

                    return RedirectToAction(nameof(Confirmation), new { id = createdIntentions.First().Id });
                }

                ViewBag.Error = "Paki-siguraduhin na may inilagay na mga pangalan sa iyong intention.";
                ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Database Error: " + (ex.InnerException?.Message ?? ex.Message);
                ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                return View(model);
            }
        }

        // GET: /MassIntention/AdminCreate
        public async Task<IActionResult> AdminCreate()
        {
            ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        // POST: /MassIntention/AdminCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCreate(MassIntention model, decimal amountPaid, string rawMassTime)
        {
            ModelState.Remove(nameof(MassIntention.ReferenceNo));
            ModelState.Remove(nameof(MassIntention.PaymentStatus));

            try
            {
                string timeInput = !string.IsNullOrEmpty(rawMassTime) ? rawMassTime : Request.Form["MassTime"].ToString();
                if (!string.IsNullOrEmpty(timeInput))
                {
                    if (TimeSpan.TryParse(timeInput, out TimeSpan parsedSpan))
                        model.MassTime = parsedSpan;
                    else if (DateTime.TryParse(timeInput, out DateTime parsedDt))
                        model.MassTime = parsedDt.TimeOfDay;
                }

                if (amountPaid <= 0 && Request.Form.ContainsKey("amountPaid"))
                    decimal.TryParse(Request.Form["amountPaid"], out amountPaid);

                if (string.IsNullOrWhiteSpace(model.RequestorName) || string.IsNullOrWhiteSpace(model.OfferingNames))
                {
                    ViewBag.Error = "Paki-kumpleto ang pangalan ng client at ang mga intention.";
                    ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                    return View(model);
                }

                model.ReferenceNo = "WALK-" + DateTime.Now.Year + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
                model.CreatedAt = DateTime.Now;
                model.PaymentStatus = "CONFIRMED / PAID";
                model.IsAdminEncoded = true;

                _context.MassIntentions.Add(model);
                await _context.SaveChangesAsync();

                var payment = new Payment
                {
                    MassIntentionId = model.Id,
                    Amount = amountPaid,
                    PaymentMethod = "Cash",
                    Status = "Verified",
                    IsVerified = true,
                    PaymentDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                model.PaymentId = payment.Id;
                await _context.SaveChangesAsync();

                TempData["PrintedAmount"] = amountPaid.ToString();

                return RedirectToAction(nameof(PrintReceipt), new { id = model.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Database Error: " + (ex.InnerException?.Message ?? ex.Message);
                ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                return View(model);
            }
        }

        // POST: /MassIntention/ApproveByRef
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveByRef(string referenceNo)
        {
            if (string.IsNullOrWhiteSpace(referenceNo))
            {
                TempData["ErrorMessage"] = "Maling Reference Number.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var intentions = await _context.MassIntentions
                    .Include(m => m.Payment)
                    .Where(m => m.ReferenceNo == referenceNo)
                    .ToListAsync();

                if (!intentions.Any())
                {
                    TempData["ErrorMessage"] = "Walang nahanap na record.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var item in intentions)
                {
                    item.PaymentStatus = "CONFIRMED / PAID";

                    if (item.Payment != null)
                    {
                        item.Payment.Status = "Verified";
                        item.Payment.IsVerified = true;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Ang lahat ng Misa sa Ref No: {referenceNo} ay na-approve na!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Nagka-error sa pag-approve: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /MassIntention/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var intention = await _context.MassIntentions.FindAsync(id);
            if (intention == null) return NotFound();
            return View(intention);
        }

        // GET: /MassIntention/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var intention = await _context.MassIntentions
                .Include(m => m.Payment)
                .Include(m => m.MassSchedule)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (intention == null) return NotFound();
            return View(intention);
        }

        // GET: /MassIntention/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var intention = await _context.MassIntentions
                .Include(m => m.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (intention == null) return NotFound();

            ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
            return View(intention);
        }

        // POST: /MassIntention/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("RequestorName,ContactNumber,RequestorEmail,PriestName,Category,OfferingNames,MassDate,PaymentStatus")] MassIntention form,
            string rawMassTime,
            decimal amount)
        {
            var intention = await _context.MassIntentions
                .Include(m => m.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (intention == null) return NotFound();

            // Server-side validation of required fields
            if (string.IsNullOrWhiteSpace(form.RequestorName) ||
                string.IsNullOrWhiteSpace(form.ContactNumber) ||
                string.IsNullOrWhiteSpace(form.OfferingNames))
            {
                ViewBag.Error = "Paki-kumpleto ang mga kinakailangang fields (Pangalan, Contact, at Intention).";
                ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                // Preserve user input for redisplay
                intention.RequestorName = form.RequestorName;
                intention.ContactNumber = form.ContactNumber;
                intention.RequestorEmail = form.RequestorEmail;
                intention.PriestName = form.PriestName;
                intention.Category = form.Category;
                intention.OfferingNames = form.OfferingNames;
                intention.MassDate = form.MassDate == default ? intention.MassDate : form.MassDate;
                return View(intention);
            }

            try
            {
                // Parse the mass time (accepts "HH:mm:ss", "HH:mm" or a full datetime)
                string timeInput = !string.IsNullOrEmpty(rawMassTime) ? rawMassTime : Request.Form["rawMassTime"].ToString();
                if (!string.IsNullOrEmpty(timeInput))
                {
                    if (TimeSpan.TryParse(timeInput, out var parsedSpan))
                        intention.MassTime = parsedSpan;
                    else if (DateTime.TryParse(timeInput, out var parsedDt))
                        intention.MassTime = parsedDt.TimeOfDay;
                }

                intention.RequestorName = form.RequestorName.Trim();
                intention.ContactNumber = form.ContactNumber.Trim();
                intention.RequestorEmail = form.RequestorEmail;
                intention.PriestName = form.PriestName;
                intention.Category = form.Category;
                intention.OfferingNames = form.OfferingNames;
                intention.MassDate = form.MassDate;

                if (!string.IsNullOrWhiteSpace(form.PaymentStatus))
                    intention.PaymentStatus = form.PaymentStatus;

                // Keep the linked payment amount in sync when provided
                if (amount > 0)
                {
                    if (intention.Payment != null)
                    {
                        intention.Payment.Amount = amount;
                    }
                    else
                    {
                        var payment = new Payment
                        {
                            MassIntentionId = intention.Id,
                            Amount = amount,
                            PaymentMethod = "Cash",
                            Status = intention.PaymentStatus,
                            PaymentDate = DateTime.Now
                        };
                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();
                        intention.PaymentId = payment.Id;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Matagumpay na na-update ang Mass Intention.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Database Error: " + (ex.InnerException?.Message ?? ex.Message);
                ViewBag.Priests = await _context.Priests.OrderBy(p => p.Name).ToListAsync();
                return View(intention);
            }
        }

        // GET: /MassIntention/Certificate/5  (Landscape Mass Intention Certificate)
        public async Task<IActionResult> Certificate(int id)
        {
            var intention = await _context.MassIntentions
                .Include(m => m.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (intention == null) return NotFound();
            return View(intention);
        }

        // GET: /MassIntention/Track
        public IActionResult Track() => View();

        // POST: /MassIntention/Track
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(string referenceNo)
        {
            if (string.IsNullOrWhiteSpace(referenceNo))
            {
                ViewBag.Error = "Paki-lagay ang Reference Code.";
                return View();
            }

            string cleaned = referenceNo.Trim();
            var intention = await _context.MassIntentions
                .FirstOrDefaultAsync(m => m.ReferenceNo.ToUpper() == cleaned.ToUpper());

            if (intention == null)
            {
                ViewBag.Error = "Walang nahanap na Mass Intention para sa Reference Code na ito.";
                return View();
            }

            return RedirectToAction(nameof(Details), new { id = intention.Id });
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
                        _context.Payments.Remove(intention.Payment);

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

        // GET: /MassIntention/ExportToWord
        public async Task<IActionResult> ExportToWord(DateTime massDate, string? massTime, int priestId = 0)
        {
            TimeSpan? targetTime = null;

            if (!string.IsNullOrEmpty(massTime))
            {
                if (TimeSpan.TryParse(massTime, out var ts))
                    targetTime = ts;
                else if (DateTime.TryParse(massTime, out var dt))
                    targetTime = dt.TimeOfDay;
            }

            byte[] fileBytes = await _documentService.GenerateMassIntentionDocAsync(massDate, targetTime, priestId);

            string timePart = targetTime.HasValue
                ? $"{targetTime.Value.Hours:D2}{targetTime.Value.Minutes:D2}"
                : "ALL";

            string fileName = $"Mass_Intentions_{massDate:yyyyMMdd}_{timePart}.doc";
            return File(fileBytes, "application/msword", fileName);
        }
    }
}