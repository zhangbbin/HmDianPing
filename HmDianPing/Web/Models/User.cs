using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HmDianPing.Web.Models
{
    [Table("tb_user")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(11)]
        public string? Phone { get; set; }  // 手机号

        [MaxLength(128)]
        public string? Password { get; set; }  // 密码

        [MaxLength(32)]
        public string? NickName { get; set; } // 昵称

        [MaxLength(255)]
        public string? Icon { get; set; } = ""; // 头像，默认空字符串

        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
