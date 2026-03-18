using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using B3ly.PL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    [RequireLogin]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        private readonly IAddressRepository _addresses;
        private readonly IProductRepository _products;
        private readonly CartService _cart;
        private readonly AuthService _auth;
        private readonly ApplicationDbContext _db;

        public OrdersController(IOrderRepository orders, IAddressRepository addresses,
            IProductRepository products, CartService cart, AuthService auth,
            ApplicationDbContext db)
        {
            _orders    = orders;
            _addresses = addresses;
            _products  = products;
            _cart      = cart;
            _auth      = auth;
            _db        = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _auth.GetCurrentUser()!.Id;
            return View(await _orders.GetUserOrdersAsync(userId));
        }

        public async Task<IActionResult> Details(int id)
        {
            var user  = _auth.GetCurrentUser()!;
            var order = await _orders.GetOrderDetailsAsync(id, user.Role == "Admin" ? null : user.Id);
            if (order == null) return NotFound();
            return View(order);
        }

        // ── Checkout GET ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (_auth.IsAdmin) return Forbid();

            var cart = _cart.GetCart();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

            var userId        = _auth.GetCurrentUser()!.Id;
            var savedAddresses= await _addresses.GetUserAddressesAsync(userId);

            return View(new CheckoutVM { Cart = cart, SavedAddresses = savedAddresses });
        }

        // ── Checkout POST ─────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM vm)
        {
            if (_auth.IsAdmin) return Forbid();

            var cart = _cart.GetCart();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

            vm.Cart = cart;
            var userId = _auth.GetCurrentUser()!.Id;

            // Fix: skip address field validation when an existing address is selected
            if (vm.ExistingAddressId.HasValue)
            {
                ModelState.Remove(nameof(CheckoutVM.Country));
                ModelState.Remove(nameof(CheckoutVM.City));
                ModelState.Remove(nameof(CheckoutVM.Street));
                ModelState.Remove(nameof(CheckoutVM.Zip));
            }

            if (!ModelState.IsValid)
            {
                vm.SavedAddresses = await _addresses.GetUserAddressesAsync(userId);
                return View(vm);
            }

            // 1) Validate stock
            var productEntities = new Dictionary<int, Product>();
            foreach (var item in cart.Items)
            {
                var p = await _products.GetEntityByIdAsync(item.ProductId);
                if (p == null || !p.IsActive)
                {
                    ModelState.AddModelError("", $"'{item.ProductName}' is no longer available.");
                    vm.SavedAddresses = await _addresses.GetUserAddressesAsync(userId);
                    return View(vm);
                }
                if (p.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for '{item.ProductName}'. Available: {p.StockQuantity}");
                    vm.SavedAddresses = await _addresses.GetUserAddressesAsync(userId);
                    return View(vm);
                }
                productEntities[item.ProductId] = p;
            }

            // 2) Resolve or create address
            int addressId;
            if (vm.ExistingAddressId.HasValue)
            {
                var addr = await _addresses.GetByIdAsync(vm.ExistingAddressId.Value, userId);
                if (addr == null)
                {
                    ModelState.AddModelError("", "Selected address not found.");
                    vm.SavedAddresses = await _addresses.GetUserAddressesAsync(userId);
                    return View(vm);
                }
                addressId = addr.AddressId;
            }
            else
            {
                var address = new Address
                {
                    UserId  = userId,
                    Country = vm.Country,
                    City    = vm.City,
                    Street  = vm.Street,
                    Zip     = vm.Zip
                };
                _db.Addresses.Add(address);
                await _db.SaveChangesAsync();
                addressId = address.AddressId;
            }

            // 3) Card payment → store pending address in session and redirect to Stripe
            if (vm.PaymentMethod == PaymentMethod.Card)
            {
                HttpContext.Session.SetInt32("B3ly_PendingAddr", addressId);
                return RedirectToAction("Checkout", "Payment");
            }

            // 4) Cash payment → create order
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Recalculate total using live prices from DB
                var liveTotal = cart.Items.Sum(i => productEntities[i.ProductId].Price * i.Quantity);

                var order = new Order
                {
                    UserId            = userId,
                    ShippingAddressId = addressId,
                    OrderNumber       = $"B3LY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Status            = OrderStatus.Pending,
                    OrderDate         = DateTime.UtcNow,
                    TotalAmount       = liveTotal
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var item in cart.Items)
                {
                    var p = productEntities[item.ProductId];
                    var livePrice = p.Price; // Always use current DB price
                    _db.OrderItems.Add(new OrderItem
                    {
                        OrderId     = order.OrderId,
                        ProductId   = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice   = livePrice,
                        Quantity    = item.Quantity,
                        LineTotal   = livePrice * item.Quantity
                    });
                    p.StockQuantity -= item.Quantity;
                    _db.Products.Update(p);
                }
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                _cart.Clear();
                return RedirectToAction("Confirmation", new { id = order.OrderId });
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "An error occurred. Please try again.");
                vm.SavedAddresses = await _addresses.GetUserAddressesAsync(userId);
                return View(vm);
            }
        }

        // ── Complete Stripe Order (called after Stripe redirects back) ────────
        [HttpGet]
        public async Task<IActionResult> CompleteStripeOrder()
        {
            if (_auth.IsAdmin) return Forbid();

            var userId    = _auth.GetCurrentUser()?.Id;
            var addressId = HttpContext.Session.GetInt32("B3ly_PendingAddr");
            var cart      = _cart.GetCart();

            if (userId == null || addressId == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var productEntities = new Dictionary<int, Product>();
            foreach (var item in cart.Items)
            {
                var p = await _products.GetEntityByIdAsync(item.ProductId);
                if (p == null || !p.IsActive || p.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"Stock issue with '{item.ProductName}'. Please review your cart.";
                    return RedirectToAction("Index", "Cart");
                }
                productEntities[item.ProductId] = p;
            }

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Recalculate total using live prices from DB
                var liveTotal = cart.Items.Sum(i => productEntities[i.ProductId].Price * i.Quantity);

                var order = new Order
                {
                    UserId            = userId!,
                    ShippingAddressId = addressId!.Value,
                    OrderNumber       = $"B3LY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Status            = OrderStatus.Pending,
                    OrderDate         = DateTime.UtcNow,
                    TotalAmount       = liveTotal
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var item in cart.Items)
                {
                    var p = productEntities[item.ProductId];
                    var livePrice = p.Price; // Always use current DB price
                    _db.OrderItems.Add(new OrderItem
                    {
                        OrderId     = order.OrderId,
                        ProductId   = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice   = livePrice,
                        Quantity    = item.Quantity,
                        LineTotal   = livePrice * item.Quantity
                    });
                    p.StockQuantity -= item.Quantity;
                    _db.Products.Update(p);
                }
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                _cart.Clear();
                HttpContext.Session.Remove("B3ly_PendingAddr");
                return RedirectToAction("Confirmation", new { id = order.OrderId });
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "An error occurred completing your order. Please try again.";
                return RedirectToAction("Index", "Cart");
            }
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = _auth.GetCurrentUser()!.Id;
            var order  = await _orders.GetOrderDetailsAsync(id, userId);
            if (order == null) return NotFound();
            return View(order);
        }

        /// <summary>
        /// Allows customer to cancel their order (only if Pending or Processing).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = _auth.GetCurrentUser()!.Id;
            var (success, message) = await _orders.CancelOrderAsync(id, userId);

            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("Details", new { id });
        }

        public IActionResult Success() => View();
    }
}

