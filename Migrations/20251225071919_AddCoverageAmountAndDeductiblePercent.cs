using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverageAmountAndDeductiblePercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CoverageAmount",
                table: "OrderServiceQuotes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeductiblePercent",
                table: "OrderServiceQuotes",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverageAmount",
                table: "OrderServiceQuotes");

            migrationBuilder.DropColumn(
                name: "DeductiblePercent",
                table: "OrderServiceQuotes");
        }
    }
}
