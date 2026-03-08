using StackExchange.Redis;

namespace HmDianPing.Web.Utils
{
    public class SimpleRedisLock
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly string _name;

        private const string KEY_PREFIX = "lock:";
        private const string ID_PREFIX = "uuid:";
        private readonly string _threadId;

        public SimpleRedisLock(IConnectionMultiplexer redis, string name)
        {
            _redis = redis;
            _name = name;
            // 生成一个 "UUID + 线程ID" 作为锁的值，防止误删别人的锁
            _threadId = ID_PREFIX + Guid.NewGuid().ToString("N");
        }

        // 尝试获取锁
        public async Task<bool> TryLockAsync(long timeoutSec)
        { 
            var db = _redis.GetDatabase();
            // SET lock:order:1001 "uuid-xxxx" EX 10 NX
            return await db.StringSetAsync(
                    KEY_PREFIX + _name,
                    _threadId,
                    TimeSpan.FromSeconds(timeoutSec),
                    When.NotExists
                );
        }

    }
}
