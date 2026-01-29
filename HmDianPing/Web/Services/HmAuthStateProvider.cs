using HmDianPing.Web.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using StackExchange.Redis;
using System.Security.Claims;

namespace HmDianPing.Web.Services
{
    public class HmAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IConnectionMultiplexer _redis;
        // 缓存一个“匿名用户”状态，避免反复创建
        private readonly AuthenticationState _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public HmAuthStateProvider(IJSRuntime jsRuntime, IConnectionMultiplexer redis)
        {
            _jsRuntime = jsRuntime;
            _redis = redis;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // 1. 尝试从浏览器 LocalStorage 获取 Token
                // 注意：在服务端预渲染(Pre-rendering)阶段，调用 JSInterop 可能会失败，需要 try-catch
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "hmdianping_token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                // 2. 根据 Token 去 Redis 查询用户信息
                var db = _redis.GetDatabase();
                var tokenKey = RedisConstants.LOGIN_USER_KEY + token;

                // 检查 Key 是否存在
                if (!await db.KeyExistsAsync(tokenKey))
                {
                    // Token 过期或伪造，视为未登录
                    return _anonymous;
                }

                // 3. 获取用户数据 (Hash 结构)
                // 我们只需要 ID 和 NickName 用于显示
                var nickName = await db.HashGetAsync(tokenKey, "nickName");
                var userId = await db.HashGetAsync(tokenKey, "id");

                // 4. 构建“通行证” (ClaimsPrincipal)
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nickName.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                // 你还可以加 Role 等其他信息
            };

                var identity = new ClaimsIdentity(claims, "RedisAuth"); // "RedisAuth" 是认证类型，必须填，否则视为未认证
                var user = new ClaimsPrincipal(identity);

                // 5. 每次有效访问，顺便给 Token 续期 (30分钟)
                await db.KeyExpireAsync(tokenKey, TimeSpan.FromMinutes(RedisConstants.LOGIN_USER_TTL));

                return new AuthenticationState(user);
            }
            catch
            {
                // 发生任何异常（如 JS 还没这就绪），都视为未登录
                return _anonymous;
            }
        }

        // 👇 登录成功后调用此方法，通知 UI 更新
        public void NotifyUserLogin(string token)
        {
            // 这里的逻辑其实不需要传 token 进来解析，因为 LocalStorage 已经存了
            // 我们只需要通知 Blazor "状态变了，请重新执行 GetAuthenticationStateAsync"
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // 👇 登出时调用
        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }
    }
}
