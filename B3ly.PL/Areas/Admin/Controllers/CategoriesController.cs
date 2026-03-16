using B3ly.BLL.Interfaces;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;
using B3ly.PL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequireAdmin]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categories;
        public CategoriesController(ICategoryRepository categories) => _categories = categories;

        public async Task<IActionResult> Index() => View(await _categories.GetAllAsync());

        public IActionResult Create() => View(new CreateCategoryVM());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (await _categories.NameExistsAsync(vm.Name))
            {
                ModelState.AddModelError(nameof(vm.Name), "A category with this name already exists.");
                return View(vm);
            }
            await _categories.AddAsync(new Category { Name = vm.Name });
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _categories.GetEntityByIdAsync(id);
            if (cat == null) return NotFound();
            return View(new CreateCategoryVM { Name = cat.Name });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCategoryVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var cat = await _categories.GetEntityByIdAsync(id);
            if (cat == null) return NotFound();
            if (await _categories.NameExistsAsync(vm.Name, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Name), "A category with this name already exists.");
                return View(vm);
            }
            cat.Name = vm.Name;
            await _categories.UpdateAsync(cat);
            TempData["Success"] = "Category updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _categories.HasProductsAsync(id))
            {
                TempData["Error"] = "Cannot delete this category because it contains products.";
                return RedirectToAction("Index");
            }
            await _categories.DeleteAsync(id);
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }
    }
}
