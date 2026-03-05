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

        public async Task<IEnumerable<OrderVM>> GetUserOrdersAsync(int userId) =>
            await _db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => ToOrderVM(o))
                .ToListAsync();

        public async Task<OrderVM?> GetOrderDetailsAsync(int orderId, int? userId = null)
        {
            var q = _db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.OrderId == orderId);

            if (userId.HasValue) q = q.Where(o => o.UserId == userId.Value);
            var o = await q.FirstOrDefaultAsync();
            return o == null ? null : ToOrderVM(o);
        }

        public async Task<IEnumerable<AdminOrderVM>> GetAllOrdersAsync() =>
            await _db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderVM
                {
                    OrderId        = o.OrderId,
                    OrderNumber    = o.OrderNumber,
                    CustomerName   = o.User.FullName,
                    CustomerEmail  = o.User.Email,
                    Status         = o.Status.ToString(),
                    StatusValue    = (int)o.Status,
                    OrderDate      = o.OrderDate,
                    TotalAmount    = o.TotalAmount,
                    ShippingAddress= o.ShippingAddress.Street + ", " + o.ShippingAddress.City + ", " + o.ShippingAddress.Country,
                    Items = o.OrderItems.Select(oi => new OrderItemVM
                    {
                        ProductId   = oi.ProductId,
                        ProductName = oi.Product.Name,
                        UnitPrice   = oi.UnitPrice,
                        Quantity    = oi.Quantity,
                        LineTotal   = oi.LineTotal
                    }).ToList()
                }).ToListAsync();

        public async Task AddAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order != null) { order.Status = status; await _db.SaveChangesAsync(); }
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
                ProductName = oi.Product?.Name ?? "",
                UnitPrice   = oi.UnitPrice,
                Quantity    = oi.Quantity,
                LineTotal   = oi.LineTotal
            }).ToList()
        };
    }
}
