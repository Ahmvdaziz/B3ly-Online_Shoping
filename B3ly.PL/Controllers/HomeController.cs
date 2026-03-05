using B3ly.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;

        public HomeController(IProductRepository products, ICategoryRepository categories)
        {
            _products = products;
            _categories = categories;
        }

        public async Task<IActionResult> Index()
        {
            var featured   = await _products.GetProductsAsync(null, null, "newest", 1, 8);
            var categories = await _categories.GetAllAsync();
            ViewBag.FeaturedProducts = featured.Items;
            ViewBag.Categories       = categories;
            return View();
        }
    }
}
