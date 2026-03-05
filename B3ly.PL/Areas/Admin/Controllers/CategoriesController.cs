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

        public async Task<IActionResult> Index() => View(await _categories.GetAllAsync());

        public async Task<IActionResult> Create()
        {
            await PopulateParentsAsync();
            return View(new CreateCategoryVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryVM vm)
        {
            if (!ModelState.IsValid) { await PopulateParentsAsync(); return View(vm); }
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
            await _categories.DeleteAsync(id);
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }

        private async Task PopulateParentsAsync(int? excludeId = null)
        {
            var cats = (await _categories.GetAllAsync())
                       .Where(c => excludeId == null || c.CategoryId != excludeId)
                       .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name });
            ViewBag.Parents = new SelectList(cats, "Value", "Text");
        }
    }
}
