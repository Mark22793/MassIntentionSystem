using MassIntentionSystem.Models;

namespace MassIntentionSystem.Services
{
    public class NotificationService
    {
        // Helper function para mag-generate ng Unique Reference Code halimbawa: SD-2026-X89A
        public string GenerateReferenceNumber()
        {
            string year = DateTime.Now.Year.ToString();
            string randomCode = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            return $"SD-{year}-{randomCode}";
        }

        // Service log confirmation (pwedeng i-integrate sa SendGrid/SmtpClient para sa email)
        public async Task<bool> SendConfirmationAsync(MassIntention intention)
        {
            // Simula ng mock sending functionality
            await Task.Delay(100);

            // Log message structure:
            string message = $"Santo Domingo Church: Natanggap namin ang iyong Mass Intention Request ({intention.ReferenceNo}). Status: {intention.PaymentStatus}.";

            Console.WriteLine(message);
            return true;
        }
    }
}