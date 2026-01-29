using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HmDianPing.Web.Models;

[Table("tb_shop")] // 映射到数据库表名
public class Shop
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty; // 店铺名称

    [MaxLength(255)]
    public string? TypeId { get; set; } // 店铺类型 ID (简化起见先存字符串或暂留)

    [MaxLength(1024)]
    public string? Images { get; set; } // 店铺图片链接

    [MaxLength(255)]
    public string? Area { get; set; } // 商圈

    [MaxLength(255)]
    public string? Address { get; set; } // 地址

    [Column(TypeName = "decimal(10, 2)")] // 比如 4.5 分
    public decimal Score { get; set; }

    public long AvgPrice { get; set; } // 人均价格 (单位：分，或者直接用元，这里我们用简单的整型表示元)

    public int Comments { get; set; } // 评论数量

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime UpdateTime { get; set; } = DateTime.Now;
}

