using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;

        public CatalogController(IProductRepository products, ICategoryRepository categories)
        {
            _products   = products;
            _categories = categories;
        }

        public async Task<IActionResult> Index(int? categoryId, string? q, string? sort, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            const int pageSize = 12;
            var result     = await _products.GetProductsAsync(categoryId, q, sort, page, pageSize, minPrice, maxPrice);
            var categories = await _categories.GetAllAsync();

            return View(new CatalogIndexVM
            {
                Products          = result.Items,
                Categories        = categories,
                SelectedCategoryId= categoryId,
                SearchQuery       = q,
                Sort              = sort,
                MinPrice          = minPrice,
                MaxPrice          = maxPrice,
                CurrentPage       = result.CurrentPage,
                TotalPages        = result.TotalPages,
                TotalCount        = result.TotalCount,
                PageSize          = pageSize
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _products.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
