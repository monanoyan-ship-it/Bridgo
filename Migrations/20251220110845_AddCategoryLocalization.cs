using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionResourceKey",
                table: "ProductCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ProductCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameResourceKey",
                table: "ProductCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionResourceKey",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "NameResourceKey",
                table: "ProductCategories");
        }
    }
}
