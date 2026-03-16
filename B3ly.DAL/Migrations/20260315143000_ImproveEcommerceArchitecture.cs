using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B3ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ImproveEcommerceArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Resize Products.Name to nvarchar(450) so it can participate in a unique index
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 2) Composite unique index: product name must be unique within a category
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Name",
                table: "Products",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            // 3) Snapshot ProductName on OrderItem for order history safety
            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Back-fill existing rows with the current product name
            migrationBuilder.Sql(
                @"UPDATE oi SET oi.ProductName = p.Name
                  FROM OrderItems oi
                  INNER JOIN Products p ON p.ProductId = oi.ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Name",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}
