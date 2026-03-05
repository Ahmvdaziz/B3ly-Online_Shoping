using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _payment;
        private readonly CartService _cart;

        public PaymentController(IPaymentService payment, CartService cart)
        {
            _payment = payment;
            _cart    = cart;
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = _cart.GetCart();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

            var successUrl = Url.Action("CompleteStripeOrder", "Orders", null, Request.Scheme)!;
            var cancelUrl  = Url.Action("Checkout",            "Orders", null, Request.Scheme)!;

            var sessionUrl = await _payment.CreateCheckoutSessionAsync(cart, successUrl, cancelUrl);
            return Redirect(sessionUrl);
        }
    }
}
