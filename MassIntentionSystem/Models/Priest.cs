namespace MassIntentionSystem.Models
{
    public class Priest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; } // I-dagdag itong line
        public bool IsActive { get; set; } = true;
    }
}