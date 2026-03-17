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
            _model     = config["Ollama:Model"] ?? "phi3:mini";
            _endpoint  = (config["Ollama:BaseUrl"] ?? "http://localhost:11434").TrimEnd('/') + "/api/generate";
        }

        public async Task<string> AskAsync(string question, string userId, string userRole)
        {
            try
            {
                // ── SECURITY CHECK: Validate question is allowed for user role
                var securityError = AIPromptBuilder.ValidateQuestionForRole(question, userRole);
                if (securityError != null)
                    return securityError;

                // ── SMART ROUTING: Admin analytics questions handled in backend
                if (userRole == "Admin" && AIPromptBuilder.IsAnalyticsQuestion(question))
                {
                    return await HandleAdminAnalyticsAsync(question, userRole);
                }

                // ── Customer product discovery or general questions
                return await HandleCustomerProductAsync(question, userRole);
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"❌ {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"⚠️ Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Handles admin analytics questions - NO LLM CALL for pure data queries.
        /// Only sends formatted summary to LLM if explanation is needed.
        /// </summary>
        private async Task<string> HandleAdminAnalyticsAsync(string question, string userRole)
        {
            // Query database for business data (not LLM)
            var context = await BuildAnalyticsContextAsync(question, userRole);

            // If we got meaningful analytics data, optionally explain via LLM or return direct
            // For pure data questions, we can return directly without LLM
            if (IsDirectDataQuery(question))
            {
                return FormatDirectAnalyticsResponse(context, question);
            }

            // For questions seeking insight/analysis, use LLM to explain the data
            var prompt = AIPromptBuilder.BuildAdminPrompt(question, context);
            var systemPrompt = AIPromptBuilder.GetSystemPrompt("Admin");

            return await CallOllamaAsync(prompt, systemPrompt);
        }

        /// <summary>
        /// Handles customer product discovery - uses LLM with minimal product context.
        /// Only sends 3-5 most relevant products (NOT full database).
        /// </summary>
        private async Task<string> HandleCustomerProductAsync(string question, string userRole)
        {
            // Extract keyword and fetch ONLY relevant products
            var keyword = AIPromptBuilder.ExtractKeyword(question);
            var relevant = (await _products.GetForAIContextAsync(keyword, limit: 5)).ToList();

            // Supplement only if necessary (max 5 total)
            if (relevant.Count < 3)
            {
                var general = await _products.GetForAIContextAsync(keyword: null, limit: 3);
                relevant = relevant
                    .Concat(general)
                    .DistinctBy(p => p.Name)
                    .Take(5)
                    .ToList();
            }

            // Build minimal prompt with only filtered products
            var prompt = AIPromptBuilder.BuildCustomerPrompt(question, relevant);
            var systemPrompt = AIPromptBuilder.GetSystemPrompt(userRole);

            return await CallOllamaAsync(prompt, systemPrompt);
        }

        /// <summary>
        /// Builds analytics context by querying database based on question keywords.
        /// </summary>
        private async Task<AdminAnalyticsContext> BuildAnalyticsContextAsync(string question, string userRole)
        {
            var lowerQ = question.ToLower();
            var context = new AdminAnalyticsContext();

            try
            {
                // Query sales data based on keywords
                if (lowerQ.Contains("today"))
                {
                    var sales = await _analytics.GetTodaySalesAsync(userRole);
                    context.SalesData = $"TODAY'S SALES\nRevenue: ${sales.TotalSales:F2}\nOrders: {sales.OrderCount}\nItems Sold: {sales.ItemsSold}";
                }
                else if (lowerQ.Contains("week"))
                {
                    var sales = await _analytics.GetWeeklySalesAsync(userRole);
                    context.SalesData = $"THIS WEEK'S SALES\nRevenue: ${sales.TotalSales:F2}\nOrders: {sales.OrderCount}\nItems Sold: {sales.ItemsSold}";
                }
                else if (lowerQ.Contains("month"))
                {
                    var sales = await _analytics.GetMonthlySalesAsync(userRole);
                    context.SalesData = $"THIS MONTH'S SALES\nRevenue: ${sales.TotalSales:F2}\nOrders: {sales.OrderCount}\nItems Sold: {sales.ItemsSold}";
                }

                // Top products data
                if (lowerQ.Contains("top") || lowerQ.Contains("best selling"))
                {
                    var topProducts = await _analytics.GetTopSellingProductsAsync(5, userRole);
                    if (topProducts.Any())
                    {
                        context.ProductData = "TOP 5 PRODUCTS\n" + string.Join("\n", 
                            topProducts.Select(p => $"• {p.Name}: {p.TotalSold} sold (${p.Revenue:F2})"));
                    }
                }

                // Stock/inventory data
                if (lowerQ.Contains("stock") || lowerQ.Contains("inventory"))
                {
                    var stock = await _analytics.GetStockSummaryAsync(userRole);
                    context.StockData = $"INVENTORY STATUS\nTotal Products: {stock.TotalProducts}\nIn Stock: {stock.InStock}\nLow Stock (<= 10): {stock.LowStock}\nOut of Stock: {stock.OutOfStock}\nTotal Value: ${stock.StockValue:F2}";
                }

                // Order count data
                if (lowerQ.Contains("order count") || lowerQ.Contains("total order"))
                {
                    var orderCount = await _analytics.GetTotalOrdersAsync(null, null, userRole);
                    context.SalesData = $"TOTAL ORDERS (ALL TIME): {orderCount}";
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw security exceptions
            }

            return context;
        }

        /// <summary>
        /// Determines if a question is asking for raw data (not interpretation).
        /// </summary>
        private static bool IsDirectDataQuery(string question)
        {
            var lowerQ = question.ToLower();
            // Questions that just want numbers, not interpretation
            return lowerQ.Contains("how much") || lowerQ.Contains("how many") || 
                   lowerQ.Contains("count") || lowerQ.Contains("total");
        }

        /// <summary>
        /// Returns formatted analytics data directly without LLM call.
        /// </summary>
        private static string FormatDirectAnalyticsResponse(AdminAnalyticsContext context, string question)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(context.SalesData))
                parts.Add(context.SalesData);

            if (!string.IsNullOrEmpty(context.ProductData))
                parts.Add(context.ProductData);

            if (!string.IsNullOrEmpty(context.StockData))
                parts.Add(context.StockData);

            return parts.Count > 0 
                ? string.Join("\n\n", parts)
                : "No data available for your query.";
        }

        /// <summary>
        /// Calls Ollama with strict timeout and minimal token usage.
        /// </summary>
        private async Task<string> CallOllamaAsync(string prompt, string systemPrompt)
        {
            var payload = new { model = _model, system = systemPrompt, prompt, stream = false };

            try
            {
                var response = await _http.PostAsJsonAsync(_endpoint, payload);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("response").GetString()
                       ?? "Could not generate response.";
            }
            catch (HttpRequestException)
            {
                return "⚠️ Cannot connect to Ollama. Ensure 'ollama serve' is running.";
            }
            catch (TaskCanceledException)
            {
                return "⚠️ Request timed out. Model may be loading.";
            }
        }

        public async Task WarmUpAsync()
        {
            try
            {
                var payload = new { model = _model, prompt = ".", stream = false };
                await _http.PostAsJsonAsync(_endpoint, payload);
            }
            catch
            {
                // Warm-up failures are non-fatal
            }
        }
    }
}

