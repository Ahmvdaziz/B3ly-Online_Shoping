using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace B3ly.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(IConfiguration config)
        {
            Stripe.StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        }

        public async Task<string> CreateCheckoutSessionAsync(CartVM cart, string successUrl, string cancelUrl)
        {
            var lineItems = cart.Items.Select(i => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency   = "usd",
                    UnitAmount = (long)(i.UnitPrice * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = i.ProductName
                    }
                },
                Quantity = i.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems          = lineItems,
                Mode               = "payment",
                SuccessUrl         = successUrl,
                CancelUrl          = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url!;
        }
    }
}
