using B3ly.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace B3ly.DAL.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            await db.Database.MigrateAsync();

            // Roles
            foreach (var role in new[] { "Admin", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Admin user
            const string adminEmail = "admin@b3ly.com";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new AppUser
                {
                    FullName = "B3ly Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ── Categories ───────────────────────────────────────────────────
            if (!await db.Categories.AnyAsync())
            {
                var electronics = new Category { Name = "Electronics" };
                var clothing    = new Category { Name = "Clothing" };
                var books       = new Category { Name = "Books" };
                db.Categories.AddRange(electronics, clothing, books);
                await db.SaveChangesAsync();

                var phones  = new Category { Name = "Phones",           ParentCategoryId = electronics.CategoryId };
                var laptops = new Category { Name = "Laptops",          ParentCategoryId = electronics.CategoryId };
                var mens    = new Category { Name = "Men's Clothing",   ParentCategoryId = clothing.CategoryId };
                var womens  = new Category { Name = "Women's Clothing", ParentCategoryId = clothing.CategoryId };
                db.Categories.AddRange(phones, laptops, mens, womens);
                await db.SaveChangesAsync();

                // ── Products ─────────────────────────────────────────────────
                db.Products.AddRange(
                    new Product { CategoryId = phones.CategoryId,     Name = "iPhone 15 Pro",        SKU = "APL-IP15P",  Price = 999.99m,  StockQuantity = 50,  Description = "Apple iPhone 15 Pro with titanium design.",      ImageUrl = "https://placehold.co/400x400?text=iPhone+15" },
                    new Product { CategoryId = phones.CategoryId,     Name = "Samsung Galaxy S24",   SKU = "SAM-GS24",   Price = 849.99m,  StockQuantity = 40,  Description = "Samsung Galaxy S24 with AI features.",            ImageUrl = "https://placehold.co/400x400?text=Galaxy+S24" },
                    new Product { CategoryId = laptops.CategoryId,    Name = "MacBook Air M3",       SKU = "APL-MBA-M3", Price = 1299.99m, StockQuantity = 25,  Description = "Apple MacBook Air with M3 chip.",                 ImageUrl = "https://placehold.co/400x400?text=MacBook+Air" },
                    new Product { CategoryId = laptops.CategoryId,    Name = "Dell XPS 15",          SKU = "DEL-XPS15",  Price = 1199.99m, StockQuantity = 20,  Description = "Dell XPS 15-inch laptop.",                       ImageUrl = "https://placehold.co/400x400?text=Dell+XPS" },
                    new Product { CategoryId = mens.CategoryId,       Name = "Classic White T-Shirt",SKU = "CLT-MWT",    Price = 29.99m,   StockQuantity = 100, Description = "Premium cotton white t-shirt.",                   ImageUrl = "https://placehold.co/400x400?text=White+Tshirt" },
                    new Product { CategoryId = mens.CategoryId,       Name = "Slim Fit Jeans",       SKU = "CLT-MJN",    Price = 59.99m,   StockQuantity = 75,  Description = "Slim fit blue denim jeans.",                     ImageUrl = "https://placehold.co/400x400?text=Slim+Jeans" },
                    new Product { CategoryId = womens.CategoryId,     Name = "Floral Summer Dress",  SKU = "CLT-WFD",    Price = 49.99m,   StockQuantity = 60,  Description = "Beautiful floral pattern summer dress.",          ImageUrl = "https://placehold.co/400x400?text=Floral+Dress" },
                    new Product { CategoryId = books.CategoryId,      Name = "Clean Code",           SKU = "BK-CC-001",  Price = 39.99m,   StockQuantity = 150, Description = "A handbook of agile software craftsmanship.",    ImageUrl = "https://placehold.co/400x400?text=Clean+Code" },
                    new Product { CategoryId = books.CategoryId,      Name = "Design Patterns",      SKU = "BK-DP-001",  Price = 44.99m,   StockQuantity = 80,  Description = "Elements of reusable object-oriented software.", ImageUrl = "https://placehold.co/400x400?text=Design+Patterns" },
                    new Product { CategoryId = electronics.CategoryId,Name = "Sony WH-1000XM5",      SKU = "SNY-WH5",    Price = 349.99m,  StockQuantity = 35,  Description = "Industry-leading noise canceling headphones.",    ImageUrl = "https://placehold.co/400x400?text=Sony+WH5" }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}