using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace B3ly.BLL.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient        _http;
        private readonly IProductRepository _products;
        private readonly string            _model;
        private readonly string            _endpoint;

        // System prompt: tells the model its role and how to use the injected catalog
        private const string SystemPrompt =
            "You are a helpful shopping assistant for B3ly, a modern online store. " +
            "You will be given the current product catalog before every question. " +
            "Use ONLY the products listed in the context to answer. " +
            "If a product is not in the list, say it is not currently available. " +
            "Always mention the exact price when recommending a product. " +
            "Be concise, friendly, and accurate.";

        // Words that carry no search meaning and should be skipped
        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a","an","the","is","are","do","does","did","have","has","can","i",
            "me","my","you","your","we","our","what","which","who","how","much",
            "many","some","any","from","store","buy","get","find","show","tell",
            "about","please","suggest","recommend","want","need","looking","for",
            "sell","selling","available","currently","product","products","item","items"
        };

        public AIService(HttpClient http, IProductRepository products, IConfiguration config)
        {
            _http     = http;
            _products = products;
            _model    = config["Ollama:Model"] ?? "phi3";
            _endpoint = (config["Ollama:BaseUrl"] ?? "http://localhost:11434").TrimEnd('/') + "/api/generate";
        }

        public async Task<string> AskAsync(string question)
        {
            // ── 1. Retrieve relevant products (RAG context) ────────────────
            var keyword  = ExtractKeyword(question);
            var relevant = (await _products.GetForAIContextAsync(keyword, limit: 5)).ToList();

            // If the keyword search found fewer than 3 results, supplement with
            // general products so the AI always has enough catalog context.
            if (relevant.Count < 3)
            {
                var general = await _products.GetForAIContextAsync(keyword: null, limit: 6);
                relevant = relevant
                    .Concat(general)
                    .DistinctBy(p => p.Name)
                    .Take(6)
                    .ToList();
            }

            // ── 2. Build the enriched prompt ───────────────────────────────
            var prompt = BuildPrompt(question, relevant);

            // ── 3. Call Ollama ─────────────────────────────────────────────
            var payload = new { model = _model, system = SystemPrompt, prompt, stream = false };

            try
            {
                var response = await _http.PostAsJsonAsync(_endpoint, payload);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("response").GetString()
                       ?? "I could not generate a response. Please try again.";
            }
            catch (HttpRequestException)
            {
                return "⚠️ Could not connect to Ollama. Make sure it is running: run 'ollama serve' in a terminal.";
            }
            catch (TaskCanceledException)
            {
                return "⚠️ Request timed out. The phi3 model may still be loading — please wait a moment and try again.";
            }
        }

        public async Task WarmUpAsync()
        {
            // Send a minimal prompt so Ollama loads phi3 into memory.
            // This runs once at startup so the first real user request is instant.
            try
            {
                var payload = new { model = _model, prompt = "hello", stream = false };
                await _http.PostAsJsonAsync(_endpoint, payload);
            }
            catch
            {
                // Warmup failures are non-fatal — the model will load on the first real request.
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the full prompt by prepending the product catalog block to the user question.
        /// </summary>
        private static string BuildPrompt(string question, IReadOnlyList<ProductContextVM> products)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== B3ly Store — Current Product Catalog ===");

            if (products.Count == 0)
            {
                sb.AppendLine("  (No products are currently available)");
            }
            else
            {
                // Group by category for a cleaner, more readable context block
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

            sb.AppendLine("\n=== Customer Question ===");
            sb.AppendLine(question);

            return sb.ToString();
        }

        /// <summary>
        /// Extracts the most meaningful keyword(s) from a natural-language question
        /// to use as a database search hint.
        /// </summary>
        private static string? ExtractKeyword(string question)
        {
            var words = question
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim('.', '?', '!', ',', '"', '\''))
                .Where(w => w.Length > 2 && !Stopwords.Contains(w))
                .Take(3)
                .ToList();

            return words.Count > 0 ? string.Join(" ", words) : null;
        }
    }
}
