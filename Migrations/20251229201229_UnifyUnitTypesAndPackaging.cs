using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class UnifyUnitTypesAndPackaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PackagingLevelId",
                table: "ProductPackagings",
                newName: "UnitId");

            migrationBuilder.RenameColumn(
                name: "ContainsQuantity",
                table: "ProductPackagings",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "BaseUnitQuantity",
                table: "ProductPackagings",
                newName: "ContainsCount");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPackagings_ProductId_PackagingLevelId",
                table: "ProductPackagings",
                newName: "IX_ProductPackagings_ProductId_UnitId");

            migrationBuilder.AddColumn<int>(
                name: "SalesUnitId",
                table: "Products",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesUnitId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "ProductPackagings",
                newName: "PackagingLevelId");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ProductPackagings",
                newName: "ContainsQuantity");

            migrationBuilder.RenameColumn(
                name: "ContainsCount",
                table: "ProductPackagings",
                newName: "BaseUnitQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPackagings_ProductId_UnitId",
                table: "ProductPackagings",
                newName: "IX_ProductPackagings_ProductId_PackagingLevelId");
        }
    }
}
