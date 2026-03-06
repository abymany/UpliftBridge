using Microsoft.EntityFrameworkCore;
using UpliftBridge.Models;

namespace UpliftBridge.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Need> Needs { get; set; }

        public DbSet<Pledge> Pledges => Set<Pledge>();

        public DbSet<GiftOrder> GiftOrders { get; set; }

        public DbSet<Story> Stories { get; set; }

        // REQUIRED for updates / thank-you posts
        public DbSet<NeedUpdate> NeedUpdates { get; set; }

        public DbSet<NeedPhoto> NeedPhotos => Set<NeedPhoto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // If your Postgres schema stores these as INTEGER (0/1),
            // this conversion prevents:
            // "argument of WHERE must be type boolean, not type integer"

            modelBuilder.Entity<Need>()
                .Property(x => x.IsPublished)
                .HasConversion<int>();

            modelBuilder.Entity<NeedUpdate>()
                .Property(x => x.IsVisible)
                .HasConversion<int>();

            modelBuilder.Entity<Story>()
                .Property(x => x.IsPublished)
                .HasConversion<int>();

            // Uncomment later only if those columns are also int(0/1) in DB:
            // modelBuilder.Entity<Need>()
            //     .Property(x => x.PreferDirectToInstitution)
            //     .HasConversion<int>();

            // modelBuilder.Entity<GiftOrder>()
            //     .Property(x => x.IsAnonymous)
            //     .HasConversion<int>();
        }
    }
}