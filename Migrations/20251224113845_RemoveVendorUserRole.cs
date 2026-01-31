using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVendorUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Önce IsVendorOwner kolonunu ekle
            migrationBuilder.AddColumn<bool>(
                name: "IsVendorOwner",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // VendorRole=0 olan kullanicilari IsVendorOwner=true yap (Owner)
            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""IsVendorOwner"" = true WHERE ""VendorRole"" = 0;");

            // Artik VendorRole kolonunu silebiliriz
            migrationBuilder.DropColumn(
                name: "VendorRole",
                table: "Users");

            // VendorTeamMembers'dan Role kolonunu sil
            migrationBuilder.DropColumn(
                name: "Role",
                table: "VendorTeamMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVendorOwner",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "VendorTeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VendorRole",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
