using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyRoleVendorIdAndIsSystemRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyRoles_Vendors_VendorId",
                table: "CompanyRoles");

            migrationBuilder.DropIndex(
                name: "IX_CompanyRoles_VendorId",
                table: "CompanyRoles");

            migrationBuilder.DropColumn(
                name: "IsSystemRole",
                table: "CompanyRoles");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "CompanyRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemRole",
                table: "CompanyRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "CompanyRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRoles_VendorId",
                table: "CompanyRoles",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyRoles_Vendors_VendorId",
                table: "CompanyRoles",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
