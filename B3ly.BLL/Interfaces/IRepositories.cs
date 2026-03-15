using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;

namespace B3ly.BLL.Interfaces
{
    public interface IProductRepository
    {
        Task<PaginatedList<ProductVM>> GetProductsAsync(int? categoryId, string? search, string? sort, int page, int pageSize);
        Task<ProductVM?> GetByIdAsync(int id);
        Task<Product?> GetEntityByIdAsync(int id);
        Task<IEnumerable<ProductVM>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        /// <summary>
        /// Soft-deletes (IsActive=false) if the product is referenced by orders; otherwise hard-deletes.
        /// Returns true if soft-deleted, false if hard-deleted.
        /// </summary>
        Task<bool> DeleteAsync(int id);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);
        Task<bool> ProductNameExistsInCategoryAsync(string name, int categoryId, int? excludeId = null);
        Task<PaginatedList<ProductVM>> GetAdminProductsAsync(int? categoryId, string? search, int page, int pageSize);

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
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        /// <summary>Returns true if the category has any direct products.</summary>
        Task<bool> HasProductsAsync(int id);
        /// <summary>Returns true if the category has any direct subcategories.</summary>
        Task<bool> HasSubCategoriesAsync(int id);
        /// <summary>Returns all descendant category IDs (recursive), not including <paramref name="id"/> itself.</summary>
        Task<IEnumerable<int>> GetDescendantIdsAsync(int id);
    }

    public interface IOrderRepository
    {
        Task<IEnumerable<OrderVM>> GetUserOrdersAsync(string userId);
        Task<OrderVM?> GetOrderDetailsAsync(int orderId, string? userId = null);
        Task<IEnumerable<AdminOrderVM>> GetAllOrdersAsync(string? statusFilter = null, string? search = null);
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
