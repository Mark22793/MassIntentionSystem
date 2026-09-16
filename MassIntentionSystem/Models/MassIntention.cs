using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassIntentionSystem.Models
{
    public class MassIntention
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ReferenceNo { get; set; } = string.Empty; // Hal: SD-2026-0001

        [Required]
        [DataType(DataType.Date)]
        public DateTime MassDate { get; set; }

        [Required]
        public TimeSpan MassTime { get; set; }

        [Required]
        public IntentionCategory Category { get; set; }

        [Required]
        public string OfferingNames { get; set; } = string.Empty; // Mga pangalan ng ipagdarasal

        [Required]
        [StringLength(100)]
        public string RequestorName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? RequestorEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Verified, Cancelled

        public bool IsAdminEncoded { get; set; } = false; // true kung walk-in encoding ng Admin

        // BAGONG PROPERTIES PARA SA HISTORY TRACKING
        public bool IsPrinted { get; set; } = false;

        public string? PriestName { get; set; }
        public DateTime? PrintedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key Relationships
        public int? PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public virtual Payment? Payment { get; set; }

        public int? MassScheduleId { get; set; }
        [ForeignKey("MassScheduleId")]
        public virtual MassSchedule? MassSchedule { get; set; }

       
    }
}