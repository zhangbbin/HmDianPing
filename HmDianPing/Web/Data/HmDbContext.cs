using HmDianPing.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HmDianPing.Web.Data;

public class HmDbContext : DbContext
{
    public HmDbContext(DbContextOptions<HmDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 这里可以添加更多 Fluent API 配置
    }
}
