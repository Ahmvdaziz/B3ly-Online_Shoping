using B3ly.BLL.ViewModels;
using System.Text;

namespace B3ly.BLL.Services
{
    public class AIPromptBuilder
    {
        private const string AdminSystemPrompt =
            "You are a business intelligence AI assistant for B3ly online store. " +
            "You will receive structured business data (sales, inventory, orders). " +
            "Analyze this data and provide actionable insights in a professional, concise manner. " +
            "Always use exact numbers. Format responses with clear sections and bullet points.";

        private const string CustomerSystemPrompt =
            "You are a helpful shopping assistant for B3ly online store. " +
            "You will be given the current product catalog. " +
            "Use ONLY the products listed in the context to answer customer questions. " +
            "Always mention exact prices and stock availability. " +
            "Be friendly, concise, and focus on helping customers find what they need.";

        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a","an","the","is","are","do","does","did","have","has","can","i",
            "me","my","you","your","we","our","what","which","who","how","much",
            "many","some","any","from","store","buy","get","find","show","tell",
            "about","please","suggest","recommend","want","need","looking","for",
            "sell","selling","available","currently","product","products","item","items"
        };

        public static string GetSystemPrompt(string userRole) =>
            userRole == "Admin" ? AdminSystemPrompt : CustomerSystemPrompt;

        public static string BuildAdminPrompt(string question, AdminAnalyticsContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== B3ly Business Intelligence ===\n");

            if (!string.IsNullOrEmpty(context.SalesData))
                sb.AppendLine($"📊 Sales Data:\n{context.SalesData}\n");

            if (!string.IsNullOrEmpty(context.ProductData))
                sb.AppendLine($"📦 Product Data:\n{context.ProductData}\n");

            if (!string.IsNullOrEmpty(context.StockData))
                sb.AppendLine($"📈 Inventory Data:\n{context.StockData}\n");

            sb.AppendLine("=== Question ===");
            sb.AppendLine(question);

            return sb.ToString();
        }

        public static string BuildCustomerPrompt(string question, IReadOnlyList<ProductContextVM> products)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== B3ly Store — Current Product Catalog ===");

            if (products.Count == 0)
            {
                sb.AppendLine("  (No products are currently available)");
            }
            else
            {
                var byCategory = products.GroupBy(p => p.CategoryName);
                foreach (var group in byCategory)
                {
                    sb.AppendLine($"\n[{group.Key}]");
                    foreach (var p in group)
                    {
                        var stock = p.StockQuantity > 0
                            ? $"in stock: {p.StockQuantity}"
                            : "out of stock";

                        var desc = string.IsNullOrWhiteSpace(p.Description)
                            ? string.Empty
                            : $" — {p.Description.Trim()}";

                        sb.AppendLine($"  • {p.Name} | ${p.Price:F2} | {stock}{desc}");
                    }
                }
            }

            sb.AppendLine("\n=== Question ===");
            sb.AppendLine(question);

            return sb.ToString();
        }

        public static string? ExtractKeyword(string question)
        {
            var words = question.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !Stopwords.Contains(w) && w.Length > 2)
                .OrderByDescending(w => w.Length)
                .FirstOrDefault();

            return words?.ToLower();
        }

        public static bool IsAnalyticsQuestion(string question)
        {
            var lowerQ = question.ToLower();
            var keywords = new[] { "total", "sales", "revenue", "stock", "inventory", "selling", "top", "order", "count", "percentage", "low", "out of stock" };
            return keywords.Any(k => lowerQ.Contains(k));
        }
    }

    public class AdminAnalyticsContext
    {
        public string? SalesData { get; set; }
        public string? ProductData { get; set; }
        public string? StockData { get; set; }
    }
}
