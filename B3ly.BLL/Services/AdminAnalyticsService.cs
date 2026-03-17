using B3ly.BLL.Interfaces;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace B3ly.BLL.Services
{
    public class AdminAnalyticsService : IAdminAnalyticsService
    {
        private readonly ApplicationDbContext _db;

        public AdminAnalyticsService(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// Validates that the user is an admin. Throws UnauthorizedAccessException if not.
        /// </summary>
        private void ValidateAdminAccess(string userRole)
        {
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("You are not authorized to access this data.");
        }

        public async Task<AdminAnalytics> GetTodaySalesAsync(string userRole)
        {
            ValidateAdminAccess(userRole);
            var today = DateTime.Today; // Use DateTime.Today for accurate date filtering
            return await GetSalesAsync(today, today.AddDays(1));
        }

        public async Task<AdminAnalytics> GetWeeklySalesAsync(string userRole)
        {
            ValidateAdminAccess(userRole);
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            return await GetSalesAsync(weekStart, today.AddDays(1));
        }

        public async Task<AdminAnalytics> GetMonthlySalesAsync(string userRole)
        {
            ValidateAdminAccess(userRole);
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            return await GetSalesAsync(monthStart, monthEnd);
        }

        private async Task<AdminAnalytics> GetSalesAsync(DateTime from, DateTime to)
        {
            var orders = await _db.Orders
                .Where(o => o.OrderDate.Date >= from && o.OrderDate.Date < to)
                .ToListAsync();

            var items = await _db.OrderItems
                .Where(oi => oi.Order.OrderDate.Date >= from && oi.Order.OrderDate.Date < to)
                .ToListAsync();

            return new AdminAnalytics
            {
                TotalSales  = orders.Sum(o => o.TotalAmount),
                OrderCount  = orders.Count,
                ItemsSold   = items.Sum(oi => oi.Quantity),
                Period      = from
            };
        }

        public async Task<List<TopProductDto>> GetTopSellingProductsAsync(int limit, string userRole)
        {
            ValidateAdminAccess(userRole);

            return await _db.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new TopProductDto
                {
                    Name       = g.Key.ProductName,
                    TotalSold  = g.Sum(oi => oi.Quantity),
                    Revenue    = g.Sum(oi => oi.LineTotal)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<StockSummary> GetStockSummaryAsync(string userRole)
        {
            ValidateAdminAccess(userRole);

            var products = await _db.Products.Where(p => p.IsActive).ToListAsync();

            return new StockSummary
            {
                TotalProducts = products.Count,
                InStock       = products.Count(p => p.StockQuantity > 10),
                LowStock      = products.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 10),
                OutOfStock    = products.Count(p => p.StockQuantity == 0),
                StockValue    = products.Sum(p => p.Price * p.StockQuantity)
            };
        }

        public async Task<int> GetTotalOrdersAsync(DateTime? from, DateTime? to, string userRole)
        {
            ValidateAdminAccess(userRole);

            var query = _db.Orders.AsQueryable();

            if (from.HasValue) query = query.Where(o => o.OrderDate.Date >= from.Value.Date);
            if (to.HasValue) query = query.Where(o => o.OrderDate.Date < to.Value.Date);

            return await query.CountAsync();
        }
    }
}
