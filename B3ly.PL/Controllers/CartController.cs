using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cart;
        private readonly IProductRepository _products;

        public CartController(CartService cart, IProductRepository products)
        {
            _cart     = cart;
            _products = products;
        }

        public IActionResult Index() => View(_cart.GetCart());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int qty = 1)
        {
            var product = await _products.GetByIdAsync(productId);
            if (product == null) return NotFound();

            _cart.AddItem(new CartItemVM
            {
                ProductId   = product.ProductId,
                ProductName = product.Name,
                UnitPrice   = product.Price,
                ImageUrl    = product.ImageUrl,
                Quantity    = qty
            });

            TempData["Success"] = $"\"{product.Name}\" added to cart!";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int qty)
        {
            _cart.UpdateItem(productId, qty);
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            _cart.RemoveItem(productId);
            return RedirectToAction("Index");
        }
    }
}
