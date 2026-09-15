using MassIntentionSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MassIntentionSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<MassIntention> MassIntentions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Priest> Priests { get; set; }
        public DbSet<MassSchedule> MassSchedules { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<GeneratedDocument> GeneratedDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precision setup para sa financial transaction
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}