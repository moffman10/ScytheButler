using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScytheButler.data.DatabaseModels.LoginModels;
using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.data.DatabaseModels.Lfr;
using ScytheButler.Models;

namespace ScytheButler.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<WomGroup> Groups => Set<WomGroup>();
        public DbSet<WomPlayer> Players => Set<WomPlayer>();
        public DbSet<WomMembership> Memberships => Set<WomMembership>();

        public DbSet<Balance> Balances => Set<Balance>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        public DbSet<PlayerAchievement> PlayerAchievements => Set<PlayerAchievement>();

        public DbSet<LfrParty> LfrParties => Set<LfrParty>();
        public DbSet<LfrPartyMember> LfrPartyMembers => Set<LfrPartyMember>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WomMembership>()
                .HasKey(m => new
                {
                    m.PlayerId,
                    m.GroupId
                });

            modelBuilder.Entity<WomMembership>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Memberships)
                .HasForeignKey(m => m.GroupId);

            modelBuilder.Entity<WomMembership>()
                .HasOne(m => m.Player)
                .WithMany()
                .HasForeignKey(m => m.PlayerId);

            modelBuilder.Entity<Balance>()
                .HasKey(b => b.Bank);

            modelBuilder.Entity<PlayerAchievement>()
                .HasKey(pa => pa.Id);

            modelBuilder.Entity<PlayerAchievement>()
                .HasIndex(pa => new
                {
                    pa.PlayerId,
                    pa.AchievementKey
                })
                .IsUnique();

            modelBuilder.Entity<LfrParty>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<LfrParty>()
                .HasMany(p => p.Members)
                .WithOne(m => m.LfrParty)
                .HasForeignKey(m => m.LfrPartyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LfrPartyMember>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<LfrPartyMember>()
                .HasIndex(m => new
                {
                    m.LfrPartyId,
                    m.UserId
                })
                .IsUnique();

            modelBuilder.Entity<LfrPartyMember>()
                .HasIndex(m => new
                {
                    m.LfrPartyId,
                    m.Role
                })
                .IsUnique()
                .HasFilter("[Role] IS NOT NULL");
        }
    }
}