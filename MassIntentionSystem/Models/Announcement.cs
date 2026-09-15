using System.ComponentModel.DataAnnotations;

namespace MassIntentionSystem.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime DatePosted { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}