using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERP.Models;

namespace ERP.Data
{
    public class GLMSDbContext : IdentityDbContext<ApplicationUser>
    {
        public GLMSDbContext(DbContextOptions<GLMSDbContext> options) : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.CostUSD)
                .HasColumnType("decimal(18, 4)");

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.CostZAR)
                .HasColumnType("decimal(18, 4)");
        }
    }
}