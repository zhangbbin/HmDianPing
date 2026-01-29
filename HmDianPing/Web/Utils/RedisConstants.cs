namespace HmDianPing.Web.Utils
{
    public static class RedisConstants
    {
        // 登录验证码 Key 前缀：login:code:138xxxx
        public const string LOGIN_CODE_KEY = "login:code:";

        // 验证码有效期 (分钟)
        public const int LOGIN_CODE_TTL = 2;

        // 登录 Token Key 前缀：login:token:xxxx-xxxx
        public const string LOGIN_USER_KEY = "login:token:";

        // Token 有效期 (分钟)
        public const int LOGIN_USER_TTL = 30;
    }
}
