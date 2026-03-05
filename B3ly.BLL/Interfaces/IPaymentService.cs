using B3ly.BLL.ViewModels;

namespace B3ly.BLL.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(CartVM cart, string successUrl, string cancelUrl);
    }
}
