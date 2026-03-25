using HmDianPing.Web.Data;
using HmDianPing.Web.Dtos;
using HmDianPing.Web.Models;
using HmDianPing.Web.Utils;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HmDianPing.Web.Services
{
    public class ShopService
    {
        private readonly HmDbContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<ShopService> _logger;

        // 构造函数注入 DbContext
        public ShopService(HmDbContext dbContext, IConnectionMultiplexer redis, ILogger<ShopService> logger)
        {
            _context = dbContext;
            _redis = redis;
            _logger = logger;
        }

        // 获取所有店铺，按评分降序排序
        public async Task<PagedResult<Shop>> GetAllShopsAsync(string? searchText = null, int pageIndex = 1, int pageSize = 6)
        {
            return await GetShopsByFilterAsync(new FilterRequest
            {
                SearchText = searchText,
                SortBy = ShopSortBy.Rating
            }, pageIndex, pageSize);
        }

        public async Task<ShopFilterOptionsDto> GetFilterOptionsAsync()
        {
            var query = _context.Shops.AsNoTracking();

            var categories = await query
                .Where(s => !string.IsNullOrWhiteSpace(s.TypeId))
                .Select(s => s.TypeId!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var regions = await query
                .Where(s => !string.IsNullOrWhiteSpace(s.Area))
                .Select(s => s.Area!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new ShopFilterOptionsDto
            {
                Categories = categories,
                Regions = regions
            };
        }

        public async Task<PagedResult<Shop>> GetShopsByFilterAsync(FilterRequest request, int pageIndex = 1, int pageSize = 6)
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Shops.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var keyword = request.SearchText.Trim();
                query = query.Where(s =>
                    s.Name.Contains(keyword) ||
                    (s.Area != null && s.Area.Contains(keyword)) ||
                    (s.Address != null && s.Address.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(s => s.TypeId == request.Category);
            }

            if (!string.IsNullOrWhiteSpace(request.Region))
            {
                query = query.Where(s => s.Area == request.Region);
            }

            if (request.MinAvgPrice.HasValue)
            {
                query = query.Where(s => s.AvgPrice >= request.MinAvgPrice.Value);
            }

            if (request.MaxAvgPrice.HasValue)
            {
                query = query.Where(s => s.AvgPrice <= request.MaxAvgPrice.Value);
            }

            if (request.MinRating.HasValue)
            {
                query = query.Where(s => s.Score >= request.MinRating.Value);
            }

            query = request.SortBy switch
            {
                ShopSortBy.Distance => query
                    .OrderBy(s => string.IsNullOrWhiteSpace(request.Region) ? 0 : (s.Area == request.Region ? 0 : 1))
                    .ThenByDescending(s => s.Score)
                    .ThenByDescending(s => s.Id),
                ShopSortBy.Popularity => query
                    .OrderByDescending(s => s.Comments)
                    .ThenByDescending(s => s.Score)
                    .ThenByDescending(s => s.Id),
                _ => query
                    .OrderByDescending(s => s.Score)
                    .ThenByDescending(s => s.Comments)
                    .ThenByDescending(s => s.Id)
            };

            int totalCount = await query.CountAsync();
            int skip = (pageIndex - 1) * pageSize;

            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Shop>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 根据 ID 获取单个店铺详情，支持缓存查询
        public async Task<Shop?> GetShopByIdAsync(long id)
        {
            var db = _redis.GetDatabase();
            string cacheKey = RedisConstants.CACHE_SHOP_KEY + id;
            string lockKey = RedisConstants.LOCK_SHOP_KEY + id;

            // 1. 先从 Redis 缓存中查询
            string? shopJson = await db.StringGetAsync(cacheKey);
            if (!string.IsNullOrEmpty(shopJson))
            {
                return JsonSerializer.Deserialize<Shop>(shopJson);
            }

            Shop? shop = null;
            try
            {
                // 2. 尝试获取互斥锁
                bool isLocked = await TryLockAsync(lockKey);
                if (!isLocked)
                {
                    // 获取锁失败 (说明有其他人正在重建缓存)
                    // 休眠一会，然后重试 (递归调用自己)
                    _logger.LogDebug("店铺 {Id} 缓存重建互斥锁获取失败，正在重试...", id);
                    await Task.Delay(50);
                    return await GetShopByIdAsync(id);
                }

                // 获取锁成功
                // 3. 再次检查 Redis (Double Check)
                // 为什么？因为你排队拿到锁的时候，可能前一个人已经把缓存建好了
                shopJson = await db.StringGetAsync(cacheKey);
                if (!string.IsNullOrEmpty(shopJson))
                {
                    return JsonSerializer.Deserialize<Shop>(shopJson);
                }

                _logger.LogInformation("正在从数据库加载店铺 {Id} 的数据...", id);
                // 4. 真正去查数据库
                // 使用 AsNoTracking 防止跟踪冲突
                shop = await _context.Shops
                    .AsNoTracking()
                    .Include(s => s.Dishes.OrderBy(d => d.SortOrder))
                    .FirstOrDefaultAsync(s => s.Id == id);

                // 模拟重建延时 (测试用，生产环境请删掉)
                // await Task.Delay(200); 

                if (shop == null)
                {
                    // (可选) 解决缓存穿透：写入空值
                    await db.StringSetAsync(cacheKey, "", TimeSpan.FromMinutes(2));
                    return null;
                }

                // 5. 写入 Redis
                string json = JsonSerializer.Serialize(shop);
                await db.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(RedisConstants.CACHE_SHOP_TTL));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询店铺 {Id} 详情时发生异常", id);
                throw;
            }
            finally
            {
                // 6. 释放锁 (放在 finally 块保底，死也要释放)
                await UnlockAsync(lockKey);
            }

            return shop;
        }

        // 给店铺增加热度
        public async Task AddCommentCountAsync(long shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop != null)
            {
                shop.Comments += 1;
                await _context.SaveChangesAsync();
            }
        }

        // 更新店铺信息
        public async Task UpdateShopAsync(Shop shop)
        {
            // 检查上下文中是否已经跟踪了同一个 ID 的对象
            var local = _context.Shops.Local.FirstOrDefault(s => s.Id == shop.Id);
            // 如果有，将其“解除跟踪” (Detach)，给新对象腾位置
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            // 现在可以安全地 Attach/ Update 新对象了
            _context.Shops.Update(shop);
            await _context.SaveChangesAsync();

            // 更新时删除缓存
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(RedisConstants.CACHE_SHOP_KEY + shop.Id);
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

                // 删除缓存
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(RedisConstants.CACHE_SHOP_KEY + id);
            }
        }

        // 尝试获取分布式锁，只有在锁不存在时才能成功
        public async Task<bool> TryLockAsync(string key)
        {
            var db = _redis.GetDatabase();
            // set lockKey "1" EX 10 NX
            return await db.StringSetAsync(key, "1", TimeSpan.FromSeconds(RedisConstants.LOCK_SHOP_TTL), When.NotExists);
        }

        // 释放锁
        public async Task UnlockAsync(string key)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
    }
}
