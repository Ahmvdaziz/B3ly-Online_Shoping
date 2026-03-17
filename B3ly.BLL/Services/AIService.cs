using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace B3ly.BLL.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly IProductRepository _products;
        private readonly IAdminAnalyticsService _analytics;
        private readonly string _model;
        private readonly string _endpoint;

        public AIService(HttpClient http, IProductRepository products, IAdminAnalyticsService analytics, IConfiguration config)
        {
            _http      = http;
            _products  = products;
            _analytics = analytics;
            _model     = config["Ollama:Model"] ?? "phi3";
            _endpoint  = (config["Ollama:BaseUrl"] ?? "http://localhost:11434").TrimEnd('/') + "/api/generate";
        }

        public async Task<string> AskAsync(string question, string userId, string userRole)
        {
            // Route based on role and question type
            if (userRole == "Admin" && AIPromptBuilder.IsAnalyticsQuestion(question))
            {
                return await HandleAdminAnalyticsAsync(question);
            }

            // Customer or general question → use product RAG
            return await HandleCustomerProductAsync(question, userRole);
        }

        private async Task<string> HandleAdminAnalyticsAsync(string question)
        {
            try
            {
                // Query database for business data
                var context = await BuildAnalyticsContextAsync(question);

                // Build admin-specific prompt
                var prompt = AIPromptBuilder.BuildAdminPrompt(question, context);
                var systemPrompt = AIPromptBuilder.GetSystemPrompt("Admin");

                return await CallOllamaAsync(prompt, systemPrompt);
            }
            catch (Exception ex)
            {
                return $"Error processing analytics: {ex.Message}";
            }
        }

        private async Task<string> HandleCustomerProductAsync(string question, string userRole)
        {
            try
            {
                // Extract keyword and fetch relevant products
                var keyword = AIPromptBuilder.ExtractKeyword(question);
                var relevant = (await _products.GetForAIContextAsync(keyword, limit: 5)).ToList();

                // Supplement with general products if needed
                if (relevant.Count < 3)
                {
                    var general = await _products.GetForAIContextAsync(keyword: null, limit: 6);
                    relevant = relevant
                        .Concat(general)
                        .DistinctBy(p => p.Name)
                        .Take(6)
                        .ToList();
                }

                // Build customer prompt
                var prompt = AIPromptBuilder.BuildCustomerPrompt(question, relevant);
                var systemPrompt = AIPromptBuilder.GetSystemPrompt(userRole);

                return await CallOllamaAsync(prompt, systemPrompt);
            }
            catch (HttpRequestException)
            {
                return "⚠️ Could not connect to Ollama. Make sure it is running: run 'ollama serve' in a terminal.";
            }
            catch (TaskCanceledException)
            {
                return "⚠️ Request timed out. The model is loading — please try again.";
            }
        }

        private async Task<AdminAnalyticsContext> BuildAnalyticsContextAsync(string question)
        {
            var lowerQ = question.ToLower();
            var context = new AdminAnalyticsContext();

            // Determine which analytics to fetch based on keywords
            if (lowerQ.Contains("today"))
            {
                var sales = await _analytics.GetTodaySalesAsync();
                context.SalesData = FormatSalesData("Today's Sales", sales);
            }
            else if (lowerQ.Contains("week"))
            {
                var sales = await _analytics.GetWeeklySalesAsync();
                context.SalesData = FormatSalesData("Weekly Sales", sales);
            }
            else if (lowerQ.Contains("month"))
            {
                var sales = await _analytics.GetMonthlySalesAsync();
                context.SalesData = FormatSalesData("Monthly Sales", sales);
            }

            if (lowerQ.Contains("top") || lowerQ.Contains("best selling"))
            {
                var topProducts = await _analytics.GetTopSellingProductsAsync(5);
                context.ProductData = FormatTopProducts(topProducts);
            }

            if (lowerQ.Contains("stock") || lowerQ.Contains("inventory"))
            {
                var stock = await _analytics.GetStockSummaryAsync();
                context.StockData = FormatStockSummary(stock);
            }

            if (lowerQ.Contains("order"))
            {
                var orderCount = await _analytics.GetTotalOrdersAsync();
                context.SalesData = $"Total Orders: {orderCount}";
            }

            return context;
        }

        private static string FormatSalesData(string period, AdminAnalytics analytics)
        {
            return $@"{period}:
  • Total Sales: ${analytics.TotalSales:F2}
  • Orders: {analytics.OrderCount}
  • Items Sold: {analytics.ItemsSold}";
        }

        private static string FormatTopProducts(List<TopProductDto> products)
        {
            if (products.Count == 0) return "No sales data available.";

            var lines = new[] { "Top Selling Products:" }
                .Concat(products.Select((p, i) => $"  {i + 1}. {p.Name}: {p.TotalSold} sold (${p.Revenue:F2})"))
                .ToArray();

            return string.Join("\n", lines);
        }

        private static string FormatStockSummary(StockSummary stock)
        {
            return $@"Inventory Status:
  • Total Products: {stock.TotalProducts}
  • In Stock: {stock.InStock}
  • Low Stock: {stock.LowStock}
  • Out of Stock: {stock.OutOfStock}
  • Total Value: ${stock.StockValue:F2}
  • Stock Health: {(stock.OutOfStock == 0 ? "✅ Good" : $"⚠️ {stock.OutOfStock} items unavailable")}";
        }

        private async Task<string> CallOllamaAsync(string prompt, string systemPrompt)
        {
            var payload = new { model = _model, system = systemPrompt, prompt, stream = false };

            var response = await _http.PostAsJsonAsync(_endpoint, payload);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("response").GetString()
                   ?? "I could not generate a response. Please try again.";
        }

        public async Task WarmUpAsync()
        {
            try
            {
                var payload = new { model = _model, prompt = "hello", stream = false };
                await _http.PostAsJsonAsync(_endpoint, payload);
            }
            catch
            {
                // Non-fatal
            }
        }
    }
}
