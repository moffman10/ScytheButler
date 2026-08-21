using Microsoft.EntityFrameworkCore;
using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.Models;
using System.Reflection.Emit;

namespace ScytheButler.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- Wise Old Man Clan Roster ---
        public DbSet<WomGroup> Groups => Set<WomGroup>();
        public DbSet<WomPlayer> Players => Set<WomPlayer>();
        public DbSet<WomMembership> Memberships => Set<WomMembership>();

        // --- Discord Bot Coffer Data ---
        public DbSet<Balance> Balances => Set<Balance>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clan Roster relationships
            modelBuilder.Entity<WomMembership>()
                .HasKey(m => new { m.PlayerId, m.GroupId });

            modelBuilder.Entity<WomMembership>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Memberships)
                .HasForeignKey(m => m.GroupId);

            modelBuilder.Entity<WomMembership>()
                .HasOne(m => m.Player)
                .WithMany()
                .HasForeignKey(m => m.PlayerId);

            // Coffer/Balance configuration
            modelBuilder.Entity<Balance>()
                .HasKey(b => b.Bank);
        }
    }
}
