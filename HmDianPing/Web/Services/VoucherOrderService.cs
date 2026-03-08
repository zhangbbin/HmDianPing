using HmDianPing.Web.Data;
using HmDianPing.Web.Models;
using HmDianPing.Web.Utils;
using Microsoft.EntityFrameworkCore;

namespace HmDianPing.Web.Services
{
    public class VoucherOrderService
    {
        private readonly HmDbContext _context;
        private readonly RedisIdWorker _idWorker;

        public VoucherOrderService(HmDbContext context, RedisIdWorker idWorker)
        {
            _context = context;
            _idWorker = idWorker;
        }

        /// <summary>
        /// 秒杀下单 (基础版 - 有超卖问题)
        /// </summary>
        /// <param name="voucherId">优惠券ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>订单ID (返回 -1 代表失败)</returns>
        public async Task<long> SeckillVoucherAsync(long voucherId, long userId)
        {
            // 1. 查询优惠券
            // 使用 AsNoTracking 提高性能
            var voucher = await _context.Vouchers.FindAsync(voucherId);

            // 2. 基础校验
            if (voucher == null) return -1;
            if (voucher.BeginTime > DateTime.Now) return -1;
            if (voucher.EndTime < DateTime.Now) return -1;

            // 3. 内存预判库存
            if (voucher.Stock < 1) return -1;

            // 4. 【核心修改】利用数据库乐观锁扣减库存
            // 生成 SQL: UPDATE tb_voucher SET Stock = Stock - 1 WHERE Id = ? AND Stock > 0
            int rows = await _context.Vouchers
                .Where(v => v.Id == voucherId && v.Stock >= 1)
                .ExecuteUpdateAsync(setters => setters.SetProperty(v => v.Stock, v => v.Stock - 1));

            // 判断是否扣减成功
            if (rows < 1)
            {
                // 扣减失败，说明刚才一瞬间库存没了，或者被别人改了
                return -1;
            }

            // 5. 创建订单
            var order = new VoucherOrder();
            long orderId = await _idWorker.NextIdAsync("order");
            order.Id = orderId;
            order.UserId = userId;
            order.VoucherId = voucherId;

            _context.VoucherOrders.Add(order);

            // 6. 提交事务
            await _context.SaveChangesAsync();
            return orderId;
        }
    }
}
