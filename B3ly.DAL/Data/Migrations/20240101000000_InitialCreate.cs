using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B3ly.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Users ────────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id           = table.Column<int>(type: "int", nullable: false)
                                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName     = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email        = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role         = table.Column<string>(type: "nvarchar(20)",  maxLength: 20,  nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);

            // ── Categories ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId       = table.Column<int>(type: "int", nullable: false)
                                           .Annotation("SqlServer:Identity", "1, 1"),
                    Name             = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Categories_Name",             table: "Categories", column: "Name", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Categories_ParentCategoryId", table: "Categories", column: "ParentCategoryId");

            // ── Addresses ────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                                     .Annotation("SqlServer:Identity", "1, 1"),
                    UserId    = table.Column<int>(type: "int", nullable: false),
                    Country   = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City      = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Street    = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zip       = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Addresses_UserId", table: "Addresses", column: "UserId");

            // ── Products ─────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId     = table.Column<int>(type: "int", nullable: false)
                                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId    = table.Column<int>(type: "int", nullable: false),
                    Name          = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SKU           = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Price         = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive      = table.Column<bool>(type: "bit", nullable: false),
                    Description   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt     = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Products_CategoryId", table: "Products", column: "CategoryId");
            migrationBuilder.CreateIndex(name: "IX_Products_SKU",        table: "Products", column: "SKU", unique: true);

            // ── Orders ───────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId           = table.Column<int>(type: "int", nullable: false)
                                           .Annotation("SqlServer:Identity", "1, 1"),
                    UserId            = table.Column<int>(type: "int", nullable: false),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber       = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status            = table.Column<int>(type: "int", nullable: false),
                    OrderDate         = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount       = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Orders_OrderNumber",       table: "Orders", column: "OrderNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Orders_UserId",            table: "Orders", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_Orders_ShippingAddressId", table: "Orders", column: "ShippingAddressId");

            // ── OrderItems ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                                       .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId     = table.Column<int>(type: "int", nullable: false),
                    ProductId   = table.Column<int>(type: "int", nullable: false),
                    UnitPrice   = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity    = table.Column<int>(type: "int", nullable: false),
                    LineTotal   = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_OrderItems_OrderId",   table: "OrderItems", column: "OrderId");
            migrationBuilder.CreateIndex(name: "IX_OrderItems_ProductId", table: "OrderItems", column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrderItems");
            migrationBuilder.DropTable(name: "Orders");
            migrationBuilder.DropTable(name: "Products");
            migrationBuilder.DropTable(name: "Addresses");
            migrationBuilder.DropTable(name: "Categories");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
