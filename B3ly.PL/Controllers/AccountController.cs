using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using B3ly.BLL.ViewModels;
using B3ly.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _users;
        private readonly AuthService _auth;

        public AccountController(IUserRepository users, AuthService auth)
        {
            _users = users;
            _auth = auth;
        }

        // ── Register ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            if (_auth.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _users.EmailExistsAsync(vm.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(vm);
            }

            var user = new User
            {
                FullName     = vm.FullName,
                Email        = vm.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Role         = "Customer"
            };
            await _users.AddAsync(user);

            _auth.SignIn(new SessionUserVM { Id = user.Id, FullName = user.FullName, Email = user.Email, Role = user.Role });
            TempData["Success"] = "Account created successfully. Welcome!";
            return RedirectToAction("Index", "Home");
        }

        // ── Login ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_auth.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) { ViewBag.ReturnUrl = returnUrl; return View(vm); }

            var user = await _users.GetByEmailAsync(vm.Email.ToLower().Trim());
            if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            _auth.SignIn(new SessionUserVM { Id = user.Id, FullName = user.FullName, Email = user.Email, Role = user.Role });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Products", new { area = "Admin" })
                : RedirectToAction("Index", "Home");
        }

        // ── Logout ────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _auth.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // ── Access Denied ─────────────────────────────────────────────────────
        public IActionResult AccessDenied() => View();
    }
}
