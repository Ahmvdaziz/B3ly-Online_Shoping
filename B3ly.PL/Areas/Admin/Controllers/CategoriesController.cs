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
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categories;
        public CategoriesController(ICategoryRepository categories) => _categories = categories;

        public async Task<IActionResult> Index(string? search)
        {
            var categories = await _categories.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(search))
                categories = categories.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            ViewBag.Search = search;
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateParentsAsync();
            return View(new CreateCategoryVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryVM vm)
        {
            if (!ModelState.IsValid) { await PopulateParentsAsync(); return View(vm); }
            if (await _categories.NameExistsAsync(vm.Name))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
                await PopulateParentsAsync();
                return View(vm);
            }
            await _categories.AddAsync(new Category { Name = vm.Name, ParentCategoryId = vm.ParentCategoryId });
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _categories.GetEntityByIdAsync(id);
            if (cat == null) return NotFound();
            await PopulateParentsAsync(id);
            return View(new CreateCategoryVM { Name = cat.Name, ParentCategoryId = cat.ParentCategoryId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCategoryVM vm)
        {
            if (!ModelState.IsValid) { await PopulateParentsAsync(id); return View(vm); }
            if (await _categories.NameExistsAsync(vm.Name, id))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
                await PopulateParentsAsync(id);
                return View(vm);
            }

            // Prevent self-referencing
            if (vm.ParentCategoryId.HasValue && vm.ParentCategoryId.Value == id)
            {
                ModelState.AddModelError("ParentCategoryId", "A category cannot be its own parent.");
                await PopulateParentsAsync(id);
                return View(vm);
            }

            // Prevent circular references (setting a descendant as parent)
            if (vm.ParentCategoryId.HasValue)
            {
                var descendants = await _categories.GetDescendantIdsAsync(id);
                if (descendants.Contains(vm.ParentCategoryId.Value))
                {
                    ModelState.AddModelError("ParentCategoryId", "Cannot set a descendant category as the parent (circular reference).");
                    await PopulateParentsAsync(id);
                    return View(vm);
                }
            }

            var cat = await _categories.GetEntityByIdAsync(id);
            if (cat == null) return NotFound();
            cat.Name             = vm.Name;
            cat.ParentCategoryId = vm.ParentCategoryId;
            await _categories.UpdateAsync(cat);
            TempData["Success"] = "Category updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _categories.HasProductsAsync(id) || await _categories.HasSubCategoriesAsync(id))
            {
                TempData["Error"] = "Cannot delete this category because it contains products or subcategories.";
                return RedirectToAction("Index");
            }
            await _categories.DeleteAsync(id);
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }

        private async Task PopulateParentsAsync(int? excludeId = null)
        {
            IEnumerable<int> descendants = excludeId.HasValue
                ? await _categories.GetDescendantIdsAsync(excludeId.Value)
                : Enumerable.Empty<int>();

            var cats = (await _categories.GetAllAsync())
                       .Where(c => excludeId == null || (c.CategoryId != excludeId.Value && !descendants.Contains(c.CategoryId)))
                       .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name });
            ViewBag.Parents = new SelectList(cats, "Value", "Text");
        }
    }
}
