namespace MassIntentionSystem.Models
{
    public class MassSchedule
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
        public string Language { get; set; } = string.Empty;

        // Dagdag para sa Controllers (Nawawala kanina)
        public bool IsActive { get; set; } = true;

        // Foreign Key & Navigation Property para sa Priest
        public int? PriestId { get; set; }
        public virtual Priest? Priest { get; set; }
    }
}