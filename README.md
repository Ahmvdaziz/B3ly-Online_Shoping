# B3ly E-Commerce Store

A complete **ASP.NET Core MVC 8** e-commerce solution with **Clean Architecture (3 layers)**.  
**No ASP.NET Core Identity** — uses custom `User` table + manual session-based authentication.

---

## Architecture

```
B3ly/
├── B3ly.DAL/    Data Access Layer (EF Core, Models, Migrations, Seeder)
├── B3ly.BLL/    Business Logic Layer (Repositories, ViewModels, Services)
└── B3ly.PL/     Presentation Layer (MVC Controllers, Views, Areas, Filters)
```

---

## Database Tables (6 custom tables, zero Identity tables)

| Table        | Description                       |
|--------------|-----------------------------------|
| Users        | Custom user with Role field       |
| Addresses    | Shipping addresses per user       |
| Categories   | Self-referencing category tree    |
| Products     | Store products with stock         |
| Orders       | Customer orders                   |
| OrderItems   | Line items per order              |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server or SQL Server LocalDB (built into Visual Studio)
- Visual Studio 2022 or VS Code

---

## How to Run

### Visual Studio 2022
1. Open `B3ly.sln`
2. Right-click `B3ly.PL` → **Set as Startup Project**
3. Update connection string in `B3ly.PL/appsettings.json` if needed
4. Press **F5** — migrations run automatically on first start

### Command Line
```bash
cd B3ly/B3ly.PL
dotnet run
```

---

## Connection String

`B3ly.PL/appsettings.json`:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=B3lyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

Change to match your SQL Server instance.

---

## Apply Migrations Manually

```bash
dotnet ef database update --project B3ly.DAL --startup-project B3ly.PL
```

---

## Default Admin Login

| Field    | Value             |
|----------|-------------------|
| Email    | admin@b3ly.com    |
| Password | Admin@123456      |

Seeded automatically on first run.

---

## Features

### Customer
- Product catalog with search, category filter, sort, pagination
- Session-based shopping cart
- Transactional checkout (validates stock → creates order → deducts stock atomically)
- Order history and order details

### Admin Area (`/Admin`)
- Category CRUD with self-referencing parent hierarchy
- Product CRUD with stock management
- Orders list + status management

### Authentication (Manual — No Identity)
- Custom `User` table with `Role` field (`Admin` / `Customer`)
- BCrypt password hashing
- Session-based login stored as JSON
- Custom `[RequireLogin]` and `[RequireAdmin]` ActionFilter attributes
- No `AspNetUsers`, `AspNetRoles`, or any Identity tables created

---

## How Authentication Works

1. User submits login form → `AccountController.Login()`
2. Password verified with `BCrypt.Net.BCrypt.Verify()`
3. `AuthService.SignIn()` serializes a `SessionUserVM` into the session
4. `AuthService.GetCurrentUser()` reads it back on every request
5. `[RequireLogin]` filter redirects to `/Account/Login` if not authenticated
6. `[RequireAdmin]` filter checks `Role == "Admin"` and redirects to `/Account/AccessDenied` otherwise
