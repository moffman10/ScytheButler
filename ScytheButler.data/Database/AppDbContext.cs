using Microsoft.EntityFrameworkCore;
using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.Models;
// Make sure to include the namespace where your PlayerAchievement model is located, for example:
// using Dashboard.Web.Models.PlayerDetails;

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

        // --- Player Achievements ---
        public DbSet<PlayerAchievement> PlayerAchievements => Set<PlayerAchievement>();

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

            // --- Player Achievements Configuration ---
            modelBuilder.Entity<PlayerAchievement>()
                .HasKey(pa => pa.Id);

            // Enforce the unique constraint matching the SQL schema script
            modelBuilder.Entity<PlayerAchievement>()
                .HasIndex(pa => new { pa.PlayerId, pa.AchievementKey })
                .IsUnique();
        }
    }
}