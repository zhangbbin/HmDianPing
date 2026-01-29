using HmDianPing.Web.Data;
using HmDianPing.Web.Models;
using HmDianPing.Web.Utils;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace HmDianPing.Web.Services
{
    public class UserService
    {
        private readonly HmDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public UserService(HmDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        /// <summary>
        /// 发送手机验证码
        /// </summary>
        /// <returns>返回 null 表示成功，返回字符串表示错误信息</returns>
        public async Task<string?> SendCodeAsync(string phone)
        {
            phone = phone?.Trim() ?? "";
            // 1. 校验手机号格式 (简单的正则)
            // 1开头，第二位3-9，后面接9位数字
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^1[3-9]\d{9}$"))
            {
                return "手机号格式错误！";
            }

            // 2. 生成 6 位随机验证码
            var code = Random.Shared.Next(100000, 999999).ToString();

            // 3. 保存验证码到 Redis
            // Key: login:code:13800138000
            // Value: 123456
            // Expiry: 2分钟
            var db = _redis.GetDatabase();
            var key = RedisConstants.LOGIN_CODE_KEY + phone;

            await db.StringSetAsync(key, code, TimeSpan.FromMinutes(RedisConstants.LOGIN_CODE_TTL));

            // 4. 模拟发送短信 (在控制台输出)
            Console.WriteLine($"【阿里云短信】验证码已发送。手机号：{phone}，验证码：{code}");

            return null; // 没有错误消息，代表成功
        }

        /// <summary>
        /// 登录功能
        /// </summary>
        public async Task<LoginResult> LoginAsync(string phone, string code)
        {
            // 1. 校验手机号
            if (string.IsNullOrWhiteSpace(phone)) return LoginResult.Fail("手机号不能为空");

            // 2. 从 Redis 获取验证码
            var db = _redis.GetDatabase();
            var cacheCodeKey = RedisConstants.LOGIN_CODE_KEY + phone;
            string? cacheCode = await db.StringGetAsync(cacheCodeKey);

            // 3. 校验验证码
            if (string.IsNullOrEmpty(cacheCode) || code != cacheCode)
            {
                return LoginResult.Fail("验证码错误或已过期");
            }

            // 4. 一致，根据手机号查询用户
            var user = _context.Users.FirstOrDefault(u => u.Phone == phone);

            // 5. 如果用户不存在，则创建新用户
            if (user == null)
            {
                user = await CreateUserWithPhone(phone);
            }

            // 6. 生成随机 Token (UUID)
            var token = Guid.NewGuid().ToString("N");

            // 7. 将 User 对象转为 Hash 存储到 Redis
            // Key: login:token:xxxxxxx
            var tokenKey = RedisConstants.LOGIN_USER_KEY + token;

            // 使用 Hash 结构存储，方便单独修改某个字段 (例如改昵称)
            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", user.Id),
                new HashEntry("nickName", user.NickName ?? ""),
                new HashEntry("icon", user.Icon ?? "")
            };

            await db.HashSetAsync(tokenKey, hashEntries);

            // 8. 设置 Token 有效期 (30分钟)
            await db.KeyExpireAsync(tokenKey, TimeSpan.FromMinutes(RedisConstants.LOGIN_USER_TTL));

            // 9. 登录成功后，删除 Redis 里的验证码（防止重复使用）
            await db.KeyDeleteAsync(cacheCodeKey);

            return LoginResult.Success(token, user);

        }

        public async Task<User> CreateUserWithPhone(string phone)
        {
            var user = new User
            {
                Phone = phone,
                // 随机生成一个昵称：user_8a2c
                NickName = "user_" + RandomString(6),
                Icon = "https://placehold.co/100x100/png?text=User" // 默认头像
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }
    }
}
