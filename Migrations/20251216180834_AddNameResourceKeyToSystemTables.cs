using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddNameResourceKeyToSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameResourceKey",
                table: "VendorCapabilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameResourceKey",
                table: "CompanyRoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameResourceKey",
                table: "CapabilityModules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameResourceKey",
                table: "VendorCapabilities");

            migrationBuilder.DropColumn(
                name: "NameResourceKey",
                table: "CompanyRoles");

            migrationBuilder.DropColumn(
                name: "NameResourceKey",
                table: "CapabilityModules");
        }
    }
}
