using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDeletionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MigratedProductCount",
                table: "ProductCategories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MigratedToCategoryId",
                table: "ProductCategories",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MigratedProductCount",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "MigratedToCategoryId",
                table: "ProductCategories");
        }
    }
}
