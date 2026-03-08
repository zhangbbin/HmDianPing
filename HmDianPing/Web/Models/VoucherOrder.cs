using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HmDianPing.Web.Models
{
    [Table("tb_voucher_order")]
    public class VoucherOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // 不用自增
        public long Id { get; set; }

        public long UserId { get; set; } // 下单用户 ID

        public long VoucherId { get; set; } // 代金券 ID

        public int PayType { get; set; } = 1; // 1:余额支付, 2:支付宝, 3:微信

        public int Status { get; set; } = 1; // 1:未支付, 2:已支付, 3:已取消, 4:已完成

        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? PayTime { get; set; }
        public DateTime? UseTime { get; set; }
        public DateTime? RefundTime { get; set; }
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
