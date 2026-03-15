using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }

        // ── Register ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new AppUser
            {
                FullName = vm.FullName,
                UserName = vm.Email.ToLower().Trim(),
                Email    = vm.Email.ToLower().Trim()
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Account created successfully. Welcome!";
            return RedirectToAction("Index", "Home");
        }

        // ── Login ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) { ViewBag.ReturnUrl = returnUrl; return View(vm); }

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email.ToLower().Trim(), vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var user = await _userManager.FindByNameAsync(vm.Email.ToLower().Trim());
            return user != null && await _userManager.IsInRoleAsync(user, "Admin")
                ? RedirectToAction("Index", "Products", new { area = "Admin" })
                : RedirectToAction("Index", "Home");
        }

        // ── Logout ────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── Access Denied ─────────────────────────────────────────────────────
        public IActionResult AccessDenied() => View();
    }
}
