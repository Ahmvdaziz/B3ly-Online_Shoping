using Microsoft.AspNetCore.Authorization;

namespace B3ly.PL.Filters
{
    /// <summary>Requires user to be logged in (any role).</summary>
    public class RequireLoginAttribute : AuthorizeAttribute { }

    /// <summary>Requires the logged-in user to have the "Admin" role.</summary>
    public class RequireAdminAttribute : AuthorizeAttribute
    {
        public RequireAdminAttribute() { Roles = "Admin"; }
    }
}
