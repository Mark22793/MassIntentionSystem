namespace MassIntentionSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int MassIntentionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? ProofOfPaymentPath { get; set; }

        // Dagdag para sa Controllers at Services (Nawawala kanina)
        public string Status { get; set; } = "Pending"; // e.g., "Approved", "Pending", "Rejected"
        public string? PaymentMethod { get; set; }     // e.g., "GCash", "Maya", "Cash"
        public bool IsVerified { get; set; } = false;

        // Navigation Property
        public virtual MassIntention? MassIntention { get; set; }
    }
}