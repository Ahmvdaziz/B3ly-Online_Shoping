using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _payment;
        private readonly CartService _cart;
        private readonly IProductRepository _products;

        public PaymentController(IPaymentService payment, CartService cart, IProductRepository products)
        {
            _payment  = payment;
            _cart     = cart;
            _products = products;
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = _cart.GetCart();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

            // Refresh cart prices from DB so Stripe charges the current price
            foreach (var item in cart.Items)
            {
                var product = await _products.GetByIdAsync(item.ProductId);
                if (product != null)
                    item.UnitPrice = product.Price;
            }

            var successUrl = Url.Action("CompleteStripeOrder", "Orders", null, Request.Scheme)!;
            var cancelUrl  = Url.Action("Checkout",            "Orders", null, Request.Scheme)!;

            var sessionUrl = await _payment.CreateCheckoutSessionAsync(cart, successUrl, cancelUrl);
            return Redirect(sessionUrl);
        }
    }
}
