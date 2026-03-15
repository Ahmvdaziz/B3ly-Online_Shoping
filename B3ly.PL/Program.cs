using B3ly.BLL.Identity;
using B3ly.BLL.Interfaces;
using B3ly.BLL.Repositories;
using B3ly.BLL.Services;
using B3ly.DAL.Data;
using B3ly.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = true;
    options.Password.RequireLowercase       = true;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan   = TimeSpan.FromHours(2);
});

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

// Services
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpContextAccessor();

// AI Service — typed HttpClient; timeout is read from Ollama:TimeoutSeconds (default 5 min)
var ollamaTimeout = builder.Configuration.GetValue<int>("Ollama:TimeoutSeconds", 300);
builder.Services.AddHttpClient<IAIService, AIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(ollamaTimeout);
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.Name = ".B3ly.Session";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Products}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Fire-and-forget: pre-load the Ollama model into memory so the first user
// request does not hit the cold-start delay.
app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var ai = scope.ServiceProvider.GetRequiredService<IAIService>();
            await ai.WarmUpAsync();
        }
        catch { /* non-fatal — model will load on first real request */ }
    });
});

app.Run();