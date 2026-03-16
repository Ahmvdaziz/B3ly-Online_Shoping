using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B3ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class lol3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the standalone CategoryId index only if it still exists.
            // It may have been removed implicitly by SQL Server during a previous
            // AlterColumn on Products.Name (nvarchar(max) → nvarchar(450)).
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Products_CategoryId'
                      AND object_id = OBJECT_ID(N'Products')
                )
                    DROP INDEX [IX_Products_CategoryId] ON [Products];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }
    }
}
