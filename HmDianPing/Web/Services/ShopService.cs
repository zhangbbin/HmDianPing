using HmDianPing.Web.Data;
using HmDianPing.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HmDianPing.Web.Services
{
    public class ShopService
    {
        private readonly HmDbContext _context;

        // 构造函数注入 DbContext
        public ShopService(HmDbContext dbContext)
        {
            _context = dbContext;
        }

        // 获取所有店铺，按评分降序排序
        public async Task<PagedResult<Shop>> GetAllShopsAsync(string? searchText = null, int pageIndex = 1, int pageSize = 6)
        {
            // 创建查询构建器 (IQueryable)，此时还没发送 SQL
            var query = _context.Shops.AsQueryable();


            // 1. 如果有搜索词，动态添加 WHERE 条件
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // 这里的 Contains 会被翻译成 SQL 的 LIKE '%keyword%'
                // 我们同时搜索 店铺名 和 商圈
                query = query.Where(s => s.Name.Contains(searchText) || s.Area.Contains(searchText));
            }

            // 2. 【关键】在分页前，先计算符合条件的总条数
            int totalCount = await query.CountAsync();

            // 3. 计算要跳过多少条
            int skip = (pageIndex - 1) * pageSize;

            // 4. 获取当前页数据
            var items = await query
                .OrderByDescending(s => s.Score) // 按评分降序排序
                .Skip(skip)                      // 跳过前面的记录
                .Take(pageSize)                  // 取出当前页的记录数
                .ToListAsync();                  // 执行查询

            // 5. 组装结果
            return new PagedResult<Shop>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 根据 ID 获取单个店铺详情
        public async Task<Shop?> GetShopByIdAsync(long id)
        {
            return await _context.Shops.FindAsync(id);
        }

        // 给店铺增加热度
        public async Task AddCommentCountAsync(long shopId)
        { 
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop != null) { 
                shop.Comments += 1;
                await _context.SaveChangesAsync();
            }
        }

        // 更新店铺信息
        public async Task UpdateShopAsync(Shop shop)
        {
            _context.Shops.Update(shop);
            await _context.SaveChangesAsync();
        }

        // 添加新店铺
        public async Task AddShopAsync(Shop shop)
        {
            shop.CreateTime = DateTime.Now;
            shop.UpdateTime = DateTime.Now;

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();
        }

        // 根据 ID 删除店铺
        public async Task DeleteShopAsync(long id)
        { 
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            { 
                _context.Shops.Remove(shop);
                await _context.SaveChangesAsync();
            }
        }
    }
}
