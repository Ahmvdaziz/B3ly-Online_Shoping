using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;

namespace B3ly.BLL.Interfaces
{
    public interface IProductRepository
    {
        Task<PaginatedList<ProductVM>> GetProductsAsync(int? categoryId, string? search, string? sort, int page, int pageSize,
            decimal? minPrice = null, decimal? maxPrice = null, bool includeInactive = false);
        Task<ProductVM?> GetByIdAsync(int id);
        Task<Product?> GetEntityByIdAsync(int id);
        Task<IEnumerable<ProductVM>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);
        Task<bool> NameExistsInCategoryAsync(string name, int categoryId, int? excludeId = null);

        /// <summary>
        /// Returns a compact product list for AI context building (RAG).
        /// When <paramref name="keyword"/> is provided, results are filtered by name, category, or description.
        /// </summary>
        Task<IEnumerable<ProductContextVM>> GetForAIContextAsync(string? keyword = null, int limit = 20);
    }

    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryVM>> GetAllAsync();
        Task<CategoryVM?> GetByIdAsync(int id);
        Task<Category?> GetEntityByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        /// <summary>Returns true if the category has products (safe-delete guard).</summary>
        Task<bool> HasProductsAsync(int id);
    }

    public interface IOrderRepository
    {
        Task<IEnumerable<OrderVM>> GetUserOrdersAsync(string userId);
        Task<OrderVM?> GetOrderDetailsAsync(int orderId, string? userId = null);
        Task<IEnumerable<AdminOrderVM>> GetAllOrdersAsync(string? search = null, DateTime? from = null, DateTime? to = null);
        Task AddAsync(Order order);
        Task UpdateStatusAsync(int orderId, OrderStatus status);
    }

    public interface IAddressRepository
    {
        Task<IEnumerable<AddressVM>> GetUserAddressesAsync(string userId);
        Task<Address?> GetByIdAsync(int id, string userId);
        Task AddAsync(Address address);
    }
}
