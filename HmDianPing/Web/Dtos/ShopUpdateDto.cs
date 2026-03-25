namespace HmDianPing.Web.Dtos
{
    public class ShopUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Area { get; set; }
        public string? Address { get; set; }
        public decimal Score { get; set; }
        public long AvgPrice { get; set; }
        public string? BusinessHours { get; set; }
        public string? Phone { get; set; }
        public string? ReviewSummary { get; set; }
    }
}
