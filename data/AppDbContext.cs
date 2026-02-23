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

        // ✅ REQUIRED for updates / thank-you posts
        public DbSet<NeedUpdate> NeedUpdates { get; set; }

        public DbSet<NeedPhoto> NeedPhotos => Set<NeedPhoto>();
    }
}
