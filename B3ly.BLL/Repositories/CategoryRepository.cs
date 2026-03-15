using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace B3ly.BLL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<CategoryVM>> GetAllAsync() =>
            await _db.Categories
                .Select(c => new CategoryVM
                {
                    CategoryId         = c.CategoryId,
                    Name               = c.Name,
                    ParentCategoryId   = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    ProductCount       = c.Products.Count
                }).OrderBy(c => c.Name).ToListAsync();

        public async Task<CategoryVM?> GetByIdAsync(int id) =>
            await _db.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => new CategoryVM
                {
                    CategoryId         = c.CategoryId,
                    Name               = c.Name,
                    ParentCategoryId   = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    ProductCount       = c.Products.Count
                }).FirstOrDefaultAsync();

        public async Task<Category?> GetEntityByIdAsync(int id) => await _db.Categories.FindAsync(id);

        public async Task AddAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c != null) { _db.Categories.Remove(c); await _db.SaveChangesAsync(); }
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var q = _db.Categories.Where(c => c.Name == name);
            if (excludeId.HasValue) q = q.Where(c => c.CategoryId != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<bool> HasProductsAsync(int id) =>
            await _db.Products.AnyAsync(p => p.CategoryId == id);

        public async Task<bool> HasSubCategoriesAsync(int id) =>
            await _db.Categories.AnyAsync(c => c.ParentCategoryId == id);

        public async Task<IEnumerable<int>> GetDescendantIdsAsync(int id)
        {
            // Load all parent→child pairs once, then walk in memory
            var parentChildPairs = await _db.Categories
                .Select(c => new { c.CategoryId, c.ParentCategoryId })
                .ToListAsync();

            var result = new HashSet<int>();
            var queue  = new Queue<int>();
            queue.Enqueue(id);

            while (queue.Count > 0)
            {
                var current  = queue.Dequeue();
                var children = parentChildPairs.Where(c => c.ParentCategoryId == current);
                foreach (var child in children)
                    if (result.Add(child.CategoryId))
                        queue.Enqueue(child.CategoryId);
            }
            return result;
        }
    }
}
