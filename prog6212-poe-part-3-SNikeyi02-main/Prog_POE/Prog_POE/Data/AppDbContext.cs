using Microsoft.EntityFrameworkCore;
using Prog_POE.Models;

namespace Prog_POE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimAudit> ClaimAudits { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClaimAudit>()
                .HasOne(ca => ca.Processor)
                .WithMany(u => u.ProcessedClaims)
                .HasForeignKey(ca => ca.ProcessorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportingDocument>()
                .HasOne(sd => sd.Claim)
                .WithMany(c => c.SupportingDocuments)
                .HasForeignKey(sd => sd.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}