using HmDianPing.Web.Dtos;

namespace HmDianPing.Web.Models
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? Token { get; set; }
        
        public UserDTO User { get; set; }

        public static LoginResult Success(string token, User user)
        {
            return new LoginResult
            {
                IsSuccess = true,
                Token = token,
                User = new UserDTO(user)
            };
        }

        public static LoginResult Fail(string message)
        {
            return new LoginResult { IsSuccess = false, Message = message };
        }
    }
}
