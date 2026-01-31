using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountInternationalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankCountryId",
                table: "VendorBankAccounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "VendorBankAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankCountryId",
                table: "VendorBankAccounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "VendorBankAccounts");
        }
    }
}
