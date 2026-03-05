using System.ComponentModel.DataAnnotations;

namespace B3ly.BLL.ViewModels
{
    // ── Auth ─────────────────────────────────────────────────────────────────
    public class LoginVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterVM
    {
        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class SessionUserVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // ── Catalog ──────────────────────────────────────────────────────────────
    public class ProductVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryVM
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    public class CatalogIndexVM
    {
        public IEnumerable<ProductVM> Products { get; set; } = new List<ProductVM>();
        public IEnumerable<CategoryVM> Categories { get; set; } = new List<CategoryVM>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
        public string? Sort { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 12;
    }

    // ── Cart ─────────────────────────────────────────────────────────────────
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.LineTotal);
        public int ItemCount => Items.Sum(i => i.Quantity);
    }

    // ── Checkout ─────────────────────────────────────────────────────────────
    public enum PaymentMethod { Cash, Card }

    public class CheckoutVM
    {
        public CartVM Cart { get; set; } = new();

        [Required] public string Country { get; set; } = string.Empty;
        [Required] public string City { get; set; } = string.Empty;
        [Required] public string Street { get; set; } = string.Empty;
        [Required] public string Zip { get; set; } = string.Empty;

        public int? ExistingAddressId { get; set; }
        public IEnumerable<AddressVM> SavedAddresses { get; set; } = new List<AddressVM>();
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }

    public class AddressVM
    {
        public int AddressId { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public string Display => $"{Street}, {City}, {Country} {Zip}";
    }

    // ── Orders ───────────────────────────────────────────────────────────────
    public class OrderVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemVM> Items { get; set; } = new();
    }

    public class OrderItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ── Admin ─────────────────────────────────────────────────────────────────
    public class CreateProductVM
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string SKU { get; set; } = string.Empty;
        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Required, Range(0, int.MaxValue)] public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        [Required] public int CategoryId { get; set; }
    }

    public class CreateCategoryVM
    {
        [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }

    public class AdminOrderVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int StatusValue { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemVM> Items { get; set; } = new();
    }

    public class UpdateOrderStatusVM
    {
        public int OrderId { get; set; }
        public int Status { get; set; }
    }

    // ── Pagination ───────────────────────────────────────────────────────────
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
