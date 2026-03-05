using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;

namespace B3ly.BLL.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }

    public interface IProductRepository
    {
        Task<PaginatedList<ProductVM>> GetProductsAsync(int? categoryId, string? search, string? sort, int page, int pageSize);
        Task<ProductVM?> GetByIdAsync(int id);
        Task<Product?> GetEntityByIdAsync(int id);
        Task<IEnumerable<ProductVM>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);
    }

    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryVM>> GetAllAsync();
        Task<CategoryVM?> GetByIdAsync(int id);
        Task<Category?> GetEntityByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }

    public interface IOrderRepository
    {
        Task<IEnumerable<OrderVM>> GetUserOrdersAsync(int userId);
        Task<OrderVM?> GetOrderDetailsAsync(int orderId, int? userId = null);
        Task<IEnumerable<AdminOrderVM>> GetAllOrdersAsync();
        Task AddAsync(Order order);
        Task UpdateStatusAsync(int orderId, OrderStatus status);
    }

    public interface IAddressRepository
    {
        Task<IEnumerable<AddressVM>> GetUserAddressesAsync(int userId);
        Task<Address?> GetByIdAsync(int id, int userId);
        Task AddAsync(Address address);
    }
}
