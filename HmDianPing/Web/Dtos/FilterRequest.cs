namespace HmDianPing.Web.Dtos
{
    public enum ShopSortBy
    {
        Distance = 0,
        Rating = 1,
        Popularity = 2
    }

    public class FilterRequest
    {
        public string? SearchText { get; set; }
        public string? Category { get; set; }
        public string? Region { get; set; }
        public long? MinAvgPrice { get; set; }
        public long? MaxAvgPrice { get; set; }
        public decimal? MinRating { get; set; }
        public ShopSortBy SortBy { get; set; } = ShopSortBy.Rating;
    }
}
