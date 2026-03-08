using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HmDianPing.Web.Models
{
    [Table("tb_voucher")]
    public class Voucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long ShopId { get; set; } // 所属店铺

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty; // 代金券标题 (e.g. "100元代金券")

        [MaxLength(1024)]
        public string SubTitle { get; set; } = string.Empty; // 副标题 (e.g. "周一至周五可用")

        [MaxLength(2048)]
        public string Rules { get; set; } = string.Empty; // 使用规则

        [Column(TypeName = "bigint")]
        public long PayValue { get; set; } // 支付金额 (单位：分)

        [Column(TypeName = "bigint")]
        public long ActualValue { get; set; } // 抵扣金额 (单位：分)

        public int Type { get; set; } = 0; // 0:普通券, 1:秒杀券

        public int Status { get; set; } = 1; // 1:上架, 2:下架

        // ==== 秒杀专用字段 ====
        public int Stock { get; set; } // 库存
        public DateTime? BeginTime { get; set; } // 秒杀开始时间
        public DateTime? EndTime { get; set; }   // 秒杀结束时间

        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
