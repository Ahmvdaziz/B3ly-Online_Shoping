using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;
using B3ly.PL.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace B3ly.PL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequireAdmin]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;

        public ProductsController(IProductRepository products, ICategoryRepository categories)
        {
            _products   = products;
            _categories = categories;
        }

        public async Task<IActionResult> Index() => View(await _products.GetAllAsync());

        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new CreateProductVM { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductVM vm)
        {
            if (await _products.SKUExistsAsync(vm.SKU))
                ModelState.AddModelError("SKU", "SKU already exists.");

            if (!ModelState.IsValid) { await PopulateCategoriesAsync(); return View(vm); }

            await _products.AddAsync(new Product
            {
                Name          = vm.Name,
                SKU           = vm.SKU,
                Price         = vm.Price,
                StockQuantity = vm.StockQuantity,
                IsActive      = vm.IsActive,
                Description   = vm.Description,
                ImageUrl      = vm.ImageUrl,
                CategoryId    = vm.CategoryId,
                CreatedAt     = DateTime.UtcNow
            });
            TempData["Success"] = "Product created.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var p = await _products.GetEntityByIdAsync(id);
            if (p == null) return NotFound();
            await PopulateCategoriesAsync();
            return View(new CreateProductVM
            {
                Name          = p.Name,
                SKU           = p.SKU,
                Price         = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive      = p.IsActive,
                Description   = p.Description,
                ImageUrl      = p.ImageUrl,
                CategoryId    = p.CategoryId
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductVM vm)
        {
            if (await _products.SKUExistsAsync(vm.SKU, id))
                ModelState.AddModelError("SKU", "SKU already exists.");

            if (!ModelState.IsValid) { await PopulateCategoriesAsync(); return View(vm); }

            var p = await _products.GetEntityByIdAsync(id);
            if (p == null) return NotFound();
            p.Name = vm.Name; p.SKU = vm.SKU; p.Price = vm.Price;
            p.StockQuantity = vm.StockQuantity; p.IsActive = vm.IsActive;
            p.Description = vm.Description; p.ImageUrl = vm.ImageUrl; p.CategoryId = vm.CategoryId;
            await _products.UpdateAsync(p);
            TempData["Success"] = "Product updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _products.DeleteAsync(id);
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Index");
        }

        private async Task PopulateCategoriesAsync()
        {
            var cats = (await _categories.GetAllAsync())
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name });
            ViewBag.Categories = new SelectList(cats, "Value", "Text");
        }
    }
}
