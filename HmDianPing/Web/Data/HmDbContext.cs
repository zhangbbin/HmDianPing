using HmDianPing.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HmDianPing.Web.Data;

public class HmDbContext : DbContext
{
    public HmDbContext(DbContextOptions<HmDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopDish> ShopDishes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<VoucherOrder> VoucherOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ShopDish>()
            .HasIndex(x => new { x.ShopId, x.SortOrder });

        modelBuilder.Entity<Shop>()
            .HasMany(x => x.Dishes)
            .WithOne(x => x.Shop)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
