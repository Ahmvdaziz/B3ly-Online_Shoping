namespace B3ly.BLL.Interfaces
{
    public interface IAIService
    {
        /// <summary>
        /// Ask the AI assistant a question with role-based access control.
        /// </summary>
        /// <param name="question">The user's question</param>
        /// <param name="userId">The user's ID</param>
        /// <param name="userRole">The user's role (Admin or Customer)</param>
        /// <returns>The AI response or an error message</returns>
        Task<string> AskAsync(string question, string userId, string userRole);

        /// <summary>
        /// Sends a trivial prompt to Ollama so the model is loaded into memory
        /// before the first real user request. Call once on application startup.
        /// </summary>
        Task WarmUpAsync();
    }

    public interface IAdminAnalyticsService
    {
        Task<AdminAnalytics> GetTodaySalesAsync(string userRole);
        Task<AdminAnalytics> GetWeeklySalesAsync(string userRole);
        Task<AdminAnalytics> GetMonthlySalesAsync(string userRole);
        Task<List<TopProductDto>> GetTopSellingProductsAsync(int limit, string userRole);
        Task<StockSummary> GetStockSummaryAsync(string userRole);
        Task<int> GetTotalOrdersAsync(DateTime? from, DateTime? to, string userRole);
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

