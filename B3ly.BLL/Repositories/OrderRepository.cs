using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace B3ly.BLL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<OrderVM>> GetUserOrdersAsync(string userId) =>
            await _db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => ToOrderVM(o))
                .ToListAsync();

        public async Task<OrderVM?> GetOrderDetailsAsync(int orderId, string? userId = null)
        {
            var q = _db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.OrderId == orderId);

            if (userId != null) q = q.Where(o => o.UserId == userId);
            var o = await q.FirstOrDefaultAsync();
            return o == null ? null : ToOrderVM(o);
        }

        public async Task<IEnumerable<AdminOrderVM>> GetAllOrdersAsync(string? search = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.OrderNumber.Contains(search) ||
                                         o.User.Email!.Contains(search) ||
                                         o.User.FullName.Contains(search));

            if (from.HasValue)
                query = query.Where(o => o.OrderDate >= from.Value);

            if (to.HasValue)
                query = query.Where(o => o.OrderDate <= to.Value.AddDays(1));

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderVM
                {
                    OrderId        = o.OrderId,
                    OrderNumber    = o.OrderNumber,
                    CustomerName   = o.User.FullName,
                    CustomerEmail  = o.User.Email ?? string.Empty,
                    Status         = o.Status.ToString(),
                    StatusValue    = (int)o.Status,
                    OrderDate      = o.OrderDate,
                    TotalAmount    = o.TotalAmount,
                    ShippingAddress= o.ShippingAddress.Street + ", " + o.ShippingAddress.City + ", " + o.ShippingAddress.Country,
                    Items = o.OrderItems.Select(oi => new OrderItemVM
                    {
                        ProductId   = oi.ProductId,
                        ProductName = oi.ProductName,
                        UnitPrice   = oi.UnitPrice,
                        Quantity    = oi.Quantity,
                        LineTotal   = oi.LineTotal
                    }).ToList()
                }).ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order != null)
            {
                // Prevent status updates on cancelled orders
                if (order.Status == OrderStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot update status of a cancelled order.");
                }

                order.Status = status;
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Cancels an order if allowed and restores stock.
        /// Can only cancel Pending or Processing orders.
        /// </summary>
        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
                return (false, "Order not found.");

            // Only allow cancellation for Pending or Processing orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                return (false, $"Cannot cancel order with status '{order.Status}'. Only Pending or Processing orders can be cancelled.");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Update order status to Cancelled
                order.Status = OrderStatus.Cancelled;

                // Restore stock for all items in the order
                foreach (var item in order.OrderItems)
                {
                    var product = await _db.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _db.Products.Update(product);
                    }
                }

                _db.Orders.Update(order);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return different message based on payment method
                var refundMessage = order.PaymentMethod == B3ly.DAL.Models.PaymentMethod.Card
                    ? "Order cancelled successfully. Your refund will be processed within 5–7 business days."
                    : "Order cancelled successfully. Stock has been restored.";

                return (true, refundMessage);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error cancelling order: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets total sales for today (UTC).
        /// </summary>
        public async Task<decimal> GetTodaysSalesAsync()
        {
            var today = DateTime.Today;
            return await _db.Orders
                .Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);
        }

        /// <summary>
        /// Gets total orders count (excluding cancelled).
        /// </summary>
        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _db.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .CountAsync();
        }

        private static OrderVM ToOrderVM(Order o) => new()
        {
            OrderId        = o.OrderId,
            OrderNumber    = o.OrderNumber,
            Status         = o.Status.ToString(),
            OrderDate      = o.OrderDate,
            TotalAmount    = o.TotalAmount,
            ShippingAddress= $"{o.ShippingAddress.Street}, {o.ShippingAddress.City}, {o.ShippingAddress.Country} {o.ShippingAddress.Zip}",
            Items = o.OrderItems.Select(oi => new OrderItemVM
            {
                ProductId   = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice   = oi.UnitPrice,
                Quantity    = oi.Quantity,
                LineTotal   = oi.LineTotal
            }).ToList()
        };
    }
}
