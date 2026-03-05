using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace B3ly.BLL.Services
{
    public class CartService
    {
        private const string CartKey = "B3ly_Cart";
        private readonly IHttpContextAccessor _http;
        public CartService(IHttpContextAccessor http) => _http = http;

        private ISession Session => _http.HttpContext!.Session;

        public CartVM GetCart()
        {
            var json = Session.GetString(CartKey);
            return json == null ? new CartVM() : JsonSerializer.Deserialize<CartVM>(json) ?? new CartVM();
        }

        private void Save(CartVM cart) =>
            Session.SetString(CartKey, JsonSerializer.Serialize(cart));

        public void AddItem(CartItemVM item)
        {
            var cart = GetCart();
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null) existing.Quantity += item.Quantity;
            else cart.Items.Add(item);
            Save(cart);
        }

        public void UpdateItem(int productId, int qty)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;
            if (qty <= 0) cart.Items.Remove(item);
            else item.Quantity = qty;
            Save(cart);
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i => i.ProductId == productId);
            Save(cart);
        }

        public void Clear() => Session.Remove(CartKey);
    }
}
