using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IOrderRepository _orders;
        private readonly IProductRepository _products;
        private readonly IAdminAnalyticsService _analytics;

        public DashboardController(IOrderRepository orders, IProductRepository products, IAdminAnalyticsService analytics)
        {
            _orders = orders;
            _products = products;
            _analytics = analytics;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var adminRole = "Admin";

                // Get sales and analytics data
                var todaysSales = await _orders.GetTodaysSalesAsync();
                var todaysOrders = await _analytics.GetTotalOrdersAsync(DateTime.Today, DateTime.Today.AddDays(1), adminRole);
                var totalOrders = await _orders.GetTotalOrdersCountAsync();
                var topProducts = await _analytics.GetTopSellingProductsAsync(3, adminRole);
                var stockSummary = await _analytics.GetStockSummaryAsync(adminRole);

                // Build dashboard view model
                var dashboard = new AdminDashboardVM
                {
                    TodaysSales = todaysSales,
                    TodaysOrderCount = todaysOrders,
                    TotalOrdersAllTime = totalOrders,
                    TotalProducts = stockSummary.TotalProducts,
                    InStockCount = stockSummary.InStock,
                    LowStockCount = stockSummary.LowStock,
                    OutOfStockCount = stockSummary.OutOfStock,
                    TotalInventoryValue = stockSummary.StockValue,
                    TopSellingProducts = topProducts,
                    LowStockProducts = await GetLowStockProductsAsync()
                };

                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading dashboard: {ex.Message}";
                return View(new AdminDashboardVM());
            }
        }

        /// <summary>
        /// Gets products with low stock (less than 5 units).
        /// </summary>
        private async Task<List<LowStockProductVM>> GetLowStockProductsAsync()
        {
            var allProducts = await _products.GetAllAsync();
            return allProducts
                .Where(p => p.StockQuantity > 0 && p.StockQuantity < 5)
                .Take(5)
                .Select(p => new LowStockProductVM
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    CurrentStock = p.StockQuantity,
                    Price = p.Price,
                    CategoryName = p.CategoryName
                })
                .ToList();
        }
    }
}
