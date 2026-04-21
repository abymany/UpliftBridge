using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UpliftBridge.Models;

namespace UpliftBridge.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Need> Needs { get; set; } = null!;
        public DbSet<Pledge> Pledges { get; set; } = null!;
        public DbSet<GiftOrder> GiftOrders { get; set; } = null!;
        public DbSet<Story> Stories { get; set; } = null!;
        public DbSet<NeedUpdate> NeedUpdates { get; set; } = null!;
        public DbSet<NeedPhoto> NeedPhotos { get; set; } = null!;
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Need>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.GoalAmount).HasColumnType("decimal(18,2)");
                entity.Property(x => x.AmountRaised).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<NeedPhoto>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Need)
                    .WithMany()
                    .HasForeignKey(x => x.NeedId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NeedUpdate>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Story>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Pledge>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<GiftOrder>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
        }
    }
}