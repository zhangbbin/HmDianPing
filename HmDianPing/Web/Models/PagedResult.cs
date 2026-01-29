namespace HmDianPing.Web.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();  // 当前页的数据
        public int TotalCount { get; set; }          // 总记录数
    }
}
