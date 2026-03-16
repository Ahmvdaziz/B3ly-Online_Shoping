using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;
using B3ly.PL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequireAdmin]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        public OrdersController(IOrderRepository orders) => _orders = orders;

        public async Task<IActionResult> Index(string? search, DateTime? from, DateTime? to)
        {
            ViewBag.Search = search;
            ViewBag.From   = from?.ToString("yyyy-MM-dd");
            ViewBag.To     = to?.ToString("yyyy-MM-dd");
            return View(await _orders.GetAllOrdersAsync(search, from, to));
        }

        public async Task<IActionResult> Details(int id)
        {
            var all   = await _orders.GetAllOrdersAsync();
            var order = all.FirstOrDefault(o => o.OrderId == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateOrderStatusVM vm)
        {
            await _orders.UpdateStatusAsync(vm.OrderId, (OrderStatus)vm.Status);
            TempData["Success"] = "Order status updated.";
            return RedirectToAction("Details", new { id = vm.OrderId });
        }
    }
}
