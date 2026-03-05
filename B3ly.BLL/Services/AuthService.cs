using B3ly.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace B3ly.BLL.Services
{
    public class AuthService
    {
        private const string SessionKey = "B3ly_User";
        private readonly IHttpContextAccessor _http;
        public AuthService(IHttpContextAccessor http) => _http = http;

        private ISession Session => _http.HttpContext!.Session;

        public void SignIn(SessionUserVM user) =>
            Session.SetString(SessionKey, JsonSerializer.Serialize(user));

        public void SignOut() => Session.Remove(SessionKey);

        public SessionUserVM? GetCurrentUser()
        {
            var json = Session.GetString(SessionKey);
            return json == null ? null : JsonSerializer.Deserialize<SessionUserVM>(json);
        }

        public bool IsAuthenticated => GetCurrentUser() != null;
        public bool IsAdmin => GetCurrentUser()?.Role == "Admin";
        public bool IsCustomer => GetCurrentUser()?.Role == "Customer";
    }
}
