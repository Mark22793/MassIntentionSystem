using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassIntentionSystem.Models
{
    public class GeneratedDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime TargetMassDate { get; set; }

        [Required]
        public TimeSpan TargetMassTime { get; set; }

        public int PriestId { get; set; }
        [ForeignKey("PriestId")]
        public virtual Priest? Priest { get; set; }

        [StringLength(255)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}