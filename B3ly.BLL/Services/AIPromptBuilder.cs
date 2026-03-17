using B3ly.BLL.ViewModels;
using System.Text;

namespace B3ly.BLL.Services
{
    public class AIPromptBuilder
    {
        private const string AdminSystemPrompt =
            "You are a business intelligence AI assistant for B3ly online store. " +
            "Analyze the provided business data and give concise, actionable insights. " +
            "Always use exact numbers. Keep responses brief and professional.";

        private const string CustomerSystemPrompt =
            "You are a helpful shopping assistant for B3ly online store. " +
            "Answer questions based ONLY on the products listed. " +
            "Keep responses friendly and concise. Mention exact prices and availability.";

        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a","an","the","is","are","do","does","did","have","has","can","i",
            "me","my","you","your","we","our","what","which","who","how","much",
            "many","some","any","from","store","buy","get","find","show","tell",
            "about","please","suggest","recommend","want","need","looking","for",
            "sell","selling","available","currently","product","products","item","items"
        };

        /// <summary>
        /// Returns the appropriate system prompt based on user role.
        /// </summary>
        public static string GetSystemPrompt(string userRole) =>
            userRole == "Admin" ? AdminSystemPrompt : CustomerSystemPrompt;

        /// <summary>
        /// Validates if a question is allowed for the user's role.
        /// Returns error message if unauthorized, null if authorized.
        /// </summary>
        public static string? ValidateQuestionForRole(string question, string userRole)
        {
            if (userRole == "Admin") return null; // Admins can ask anything

            // Customers cannot ask about sales, revenue, orders, or analytics
            var blockedKeywords = new[] { "sales", "revenue", "income", "profit", "earnings", "orders count", "order count", "total orders", "analytics", "statistics" };
            var lowerQ = question.ToLower();

            foreach (var keyword in blockedKeywords)
            {
                if (lowerQ.Contains(keyword))
                    return "You are not authorized to access this data.";
            }

            return null; // Authorized
        }

        /// <summary>
        /// Detects if a question is about analytics/sales (should be handled by backend, not LLM).
        /// </summary>
        public static bool IsAnalyticsQuestion(string question)
        {
            var lowerQ = question.ToLower();
            var keywords = new[] { "total", "sales", "revenue", "stock", "inventory", "selling", "top", "order", "count", "percentage", "low", "out of stock" };
            return keywords.Any(k => lowerQ.Contains(k));
        }

        /// <summary>
        /// Builds an admin prompt with minimal, focused data (no full database dump).
        /// </summary>
        public static string BuildAdminPrompt(string question, AdminAnalyticsContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BUSINESS DATA:");

            if (!string.IsNullOrEmpty(context.SalesData))
                sb.AppendLine(context.SalesData);

            if (!string.IsNullOrEmpty(context.ProductData))
                sb.AppendLine(context.ProductData);

            if (!string.IsNullOrEmpty(context.StockData))
                sb.AppendLine(context.StockData);

            sb.AppendLine("\nQUESTION: " + question);
            sb.AppendLine("INSTRUCTIONS: Provide a concise, data-driven answer with exact numbers. Keep it brief (2-3 sentences max).");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a customer prompt with filtered products only (NOT full database).
        /// Limits to 3-5 most relevant products for performance.
        /// </summary>
        public static string BuildCustomerPrompt(string question, IReadOnlyList<ProductContextVM> products)
        {
            // Only send top 3-5 products, not all
            var filteredProducts = products.Take(5).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("AVAILABLE PRODUCTS:");

            if (filteredProducts.Count == 0)
            {
                sb.AppendLine("(No matching products)");
            }
            else
            {
                foreach (var p in filteredProducts)
                {
                    var stock = p.StockQuantity > 0 ? "✓ In stock" : "✗ Out of stock";
                    sb.AppendLine($"• {p.Name} | ${p.Price:F2} | {stock}");
                }
            }

            sb.AppendLine("\nQUESTION: " + question);
            sb.AppendLine("INSTRUCTIONS: Answer based ONLY on the products listed above. Keep response brief (2-3 sentences). Mention exact prices if recommending.");

            return sb.ToString();
        }

        /// <summary>
        /// Extracts the most meaningful keyword for database searches.
        /// </summary>
        public static string? ExtractKeyword(string question)
        {
            var words = question
                .Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !Stopwords.Contains(w) && w.Length > 2)
                .OrderByDescending(w => w.Length)
                .FirstOrDefault();

            return words?.ToLower();
        }
    }

    public class AdminAnalyticsContext
    {
        public string? SalesData { get; set; }
        public string? ProductData { get; set; }
        public string? StockData { get; set; }
    }
}
