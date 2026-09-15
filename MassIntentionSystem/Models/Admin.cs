using System.ComponentModel.DataAnnotations;

namespace MassIntentionSystem.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Secretariat"; // SuperAdmin, Secretariat

        public DateTime LastLogin { get; set; }
    }
}