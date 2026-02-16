using Microsoft.EntityFrameworkCore;
using ScytheButler.Models;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace ScytheButler.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<BalanceEntry> Balances { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BalanceEntry>()
                .HasKey(b => b.Bank);
        }
    }
}
