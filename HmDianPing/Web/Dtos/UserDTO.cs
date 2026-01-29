using HmDianPing.Web.Models;

namespace HmDianPing.Web.Dtos
{
    public class UserDTO
    {
        public long Id { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        public UserDTO(User user)
        {
            Id = user.Id;
            NickName = user.NickName ?? "用户";
            Icon = user.Icon ?? "";
        }
    }
}
