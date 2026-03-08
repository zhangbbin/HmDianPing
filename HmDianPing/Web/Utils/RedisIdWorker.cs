using StackExchange.Redis;

/// @<summary>
/// 全局唯一 ID 生成器
/// 当订单量达到亿级时，单表存不下，需要分库分表。如果用数据库自增 ID，不同表的 ID 会重复。我们需要一个全局唯一的 ID。
/// 算法设计: 符号位(1 bit) + 时间戳(31 bits) + 序列号 (32 bits)
/// @</summary>

namespace HmDianPing.Web.Utils
{
    public class RedisIdWorker
    {
        private readonly IConnectionMultiplexer _redis;

        // 基准时间：2026-01-01 00:00:00 的时间戳 (秒)
        private const long BEGIN_TIMESTAMP = 1767225600L;

        // 序列号的位数
        private const int COUNT_BITS = 32;

        public RedisIdWorker(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// 生成全局唯一 ID
        /// </summary>
        /// <param name="keyPrefix">业务前缀 (如 "order")</param>
        /// <returns></returns>
        public async Task<long> NextIdAsync(string keyPrefix)
        {
            // 1. 生成时间戳
            var now = DateTime.Now;
            long newSecond = new DateTimeOffset(now).ToUnixTimeSeconds();
            long timestamp = newSecond - BEGIN_TIMESTAMP;

            // 2. 生成序列号
            string datePart = now.ToString("yyyy:MM:dd");
            string key = "icr:" + keyPrefix + ":" + datePart;

            var db = _redis.GetDatabase();
            // Redis INCR 是原子操作
            long count = await db.StringIncrementAsync(key);

            // 3. 拼接并返回
            // 运算逻辑：时间戳向左移32位，然后与序列号做“或”运算
            return (timestamp << COUNT_BITS) | count;
        }
    }
}
