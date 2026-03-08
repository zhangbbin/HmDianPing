using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HmDianPing.Web.Models;

[Table("tb_shop_dish")]
public class ShopDish
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopId { get; set; }

    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    [ForeignKey(nameof(ShopId))]
    public Shop? Shop { get; set; }
}
