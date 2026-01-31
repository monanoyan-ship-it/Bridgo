using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintFromOrderServiceQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderServiceQuotes_ServiceRequestId_ProviderVendorId",
                table: "OrderServiceQuotes");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceQuotes_ServiceRequestId_ProviderVendorId",
                table: "OrderServiceQuotes",
                columns: new[] { "ServiceRequestId", "ProviderVendorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderServiceQuotes_ServiceRequestId_ProviderVendorId",
                table: "OrderServiceQuotes");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceQuotes_ServiceRequestId_ProviderVendorId",
                table: "OrderServiceQuotes",
                columns: new[] { "ServiceRequestId", "ProviderVendorId" },
                unique: true);
        }
    }
}
