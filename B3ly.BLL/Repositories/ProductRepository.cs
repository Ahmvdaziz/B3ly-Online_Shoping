using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace B3ly.BLL.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) => _db = db;

        public async Task<PaginatedList<ProductVM>> GetProductsAsync(int? categoryId, string? search, string? sort, int page, int pageSize)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value
                                      || p.Category.ParentCategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search)
                                      || (p.Description != null && p.Description.Contains(search)));

            query = sort switch
            {
                "price_asc"  => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc"   => query.OrderBy(p => p.Name),
                "newest"     => query.OrderByDescending(p => p.CreatedAt),
                _            => query.OrderBy(p => p.Name)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductVM
                {
                    ProductId    = p.ProductId,
                    Name         = p.Name,
                    SKU          = p.SKU,
                    Price        = p.Price,
                    StockQuantity= p.StockQuantity,
                    IsActive     = p.IsActive,
                    Description  = p.Description,
                    ImageUrl     = p.ImageUrl,
                    CategoryId   = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CreatedAt    = p.CreatedAt
                }).ToListAsync();

            return new PaginatedList<ProductVM> { Items = items, TotalCount = total, CurrentPage = page, PageSize = pageSize };
        }

        public async Task<ProductVM?> GetByIdAsync(int id) =>
            await _db.Products.Include(p => p.Category)
                .Where(p => p.ProductId == id)
                .Select(p => new ProductVM
                {
                    ProductId    = p.ProductId,
                    Name         = p.Name,
                    SKU          = p.SKU,
                    Price        = p.Price,
                    StockQuantity= p.StockQuantity,
                    IsActive     = p.IsActive,
                    Description  = p.Description,
                    ImageUrl     = p.ImageUrl,
                    CategoryId   = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CreatedAt    = p.CreatedAt
                }).FirstOrDefaultAsync();

        public async Task<Product?> GetEntityByIdAsync(int id) => await _db.Products.FindAsync(id);

        public async Task<IEnumerable<ProductVM>> GetAllAsync() =>
            await _db.Products.Include(p => p.Category)
                .OrderBy(p => p.Name)
                .Select(p => new ProductVM
                {
                    ProductId    = p.ProductId,
                    Name         = p.Name,
                    SKU          = p.SKU,
                    Price        = p.Price,
                    StockQuantity= p.StockQuantity,
                    IsActive     = p.IsActive,
                    Description  = p.Description,
                    ImageUrl     = p.ImageUrl,
                    CategoryId   = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CreatedAt    = p.CreatedAt
                }).ToListAsync();

        public async Task AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null) { _db.Products.Remove(p); await _db.SaveChangesAsync(); }
        }

        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
        {
            var q = _db.Products.Where(p => p.SKU == sku);
            if (excludeId.HasValue) q = q.Where(p => p.ProductId != excludeId.Value);
            return await q.AnyAsync();
        }
    }
}
