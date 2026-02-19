using Microsoft.EntityFrameworkCore;
using ScytheButler.Models;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace ScytheButler.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Balance> Balances { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>().HasKey(b => b.Bank);
            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
        }
    }
}
