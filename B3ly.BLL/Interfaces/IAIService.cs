namespace B3ly.BLL.Interfaces
{
    public interface IAIService
    {
        Task<string> AskAsync(string question, string userId, string userRole);

        /// <summary>
        /// Sends a trivial prompt to Ollama so the model is loaded into memory
        /// before the first real user request. Call once on application startup.
        /// </summary>
        Task WarmUpAsync();
    }

    public interface IAdminAnalyticsService
    {
        Task<AdminAnalytics> GetTodaySalesAsync();
        Task<AdminAnalytics> GetWeeklySalesAsync();
        Task<AdminAnalytics> GetMonthlySalesAsync();
        Task<List<TopProductDto>> GetTopSellingProductsAsync(int limit = 5);
        Task<StockSummary> GetStockSummaryAsync();
        Task<int> GetTotalOrdersAsync(DateTime? from = null, DateTime? to = null);
    }

    public class AdminAnalytics
    {
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
        public int ItemsSold { get; set; }
        public DateTime Period { get; set; }
    }

    public class TopProductDto
    {
        public string Name { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class StockSummary
    {
        public int TotalProducts { get; set; }
        public int InStock { get; set; }
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
        public decimal StockValue { get; set; }
    }
}

