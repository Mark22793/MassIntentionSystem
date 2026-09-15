using MassIntentionSystem.Data;
using MassIntentionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MassIntentionSystem.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PaymentService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Processing ng image upload mula sa Public Form
        public async Task<string> UploadPaymentProofAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payment-proofs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/payment-proofs/" + uniqueFileName;
        }

        // Verification Function para sa Admin
        public async Task<bool> VerifyPaymentAsync(int massIntentionId, string adminStatus)
        {
            var intention = await _context.MassIntentions
                .Include(m => m.Payment)
                .FirstOrDefaultAsync(m => m.Id == massIntentionId);

            if (intention == null) return false;

            intention.PaymentStatus = adminStatus; // "Verified" o "Cancelled"
            if (intention.Payment != null)
            {
                intention.Payment.Status = adminStatus == "Verified" ? "Approved" : "Rejected";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}