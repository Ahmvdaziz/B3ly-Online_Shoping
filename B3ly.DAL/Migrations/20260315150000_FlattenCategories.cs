using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B3ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FlattenCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Null out any existing parent references before dropping the FK
            migrationBuilder.Sql("UPDATE [Categories] SET [ParentCategoryId] = NULL WHERE [ParentCategoryId] IS NOT NULL");

            // Drop FK constraint (named by EF convention)
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            // Drop the index on ParentCategoryId (if it exists)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Categories_ParentCategoryId'
                      AND object_id = OBJECT_ID(N'Categories')
                )
                    DROP INDEX [IX_Categories_ParentCategoryId] ON [Categories];
            ");

            // Drop the column
            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
