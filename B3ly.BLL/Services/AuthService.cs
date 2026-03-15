using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace B3ly.BLL.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor _http;
        public AuthService(IHttpContextAccessor http) => _http = http;

        private ClaimsPrincipal? Principal => _http.HttpContext?.User;

        public SessionUserVM? GetCurrentUser()
        {
            var principal = Principal;
            if (principal?.Identity?.IsAuthenticated != true) return null;

            return new SessionUserVM
            {
                Id       = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                FullName = principal.FindFirstValue("FullName") ?? string.Empty,
                Email    = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                Role     = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty
            };
        }

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
        public bool IsAdmin         => Principal?.IsInRole("Admin") == true;
        public bool IsCustomer      => Principal?.IsInRole("Customer") == true;
    }
}
