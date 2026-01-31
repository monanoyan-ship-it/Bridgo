using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RenameCodeToSystemName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "WarehouseTypes",
                newName: "SystemName");

            migrationBuilder.RenameIndex(
                name: "IX_WarehouseTypes_Code",
                table: "WarehouseTypes",
                newName: "IX_WarehouseTypes_SystemName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "VendorStatuses",
                newName: "SystemName");

            migrationBuilder.RenameIndex(
                name: "IX_VendorStatuses_Code",
                table: "VendorStatuses",
                newName: "IX_VendorStatuses_SystemName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "TeamMemberStatuses",
                newName: "SystemName");

            migrationBuilder.RenameIndex(
                name: "IX_TeamMemberStatuses_Code",
                table: "TeamMemberStatuses",
                newName: "IX_TeamMemberStatuses_SystemName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "ProductStatuses",
                newName: "SystemName");

            migrationBuilder.RenameIndex(
                name: "IX_ProductStatuses_Code",
                table: "ProductStatuses",
                newName: "IX_ProductStatuses_SystemName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "AddressTypes",
                newName: "SystemName");

            migrationBuilder.RenameIndex(
                name: "IX_AddressTypes_Code",
                table: "AddressTypes",
                newName: "IX_AddressTypes_SystemName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SystemName",
                table: "WarehouseTypes",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_WarehouseTypes_SystemName",
                table: "WarehouseTypes",
                newName: "IX_WarehouseTypes_Code");

            migrationBuilder.RenameColumn(
                name: "SystemName",
                table: "VendorStatuses",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_VendorStatuses_SystemName",
                table: "VendorStatuses",
                newName: "IX_VendorStatuses_Code");

            migrationBuilder.RenameColumn(
                name: "SystemName",
                table: "TeamMemberStatuses",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_TeamMemberStatuses_SystemName",
                table: "TeamMemberStatuses",
                newName: "IX_TeamMemberStatuses_Code");

            migrationBuilder.RenameColumn(
                name: "SystemName",
                table: "ProductStatuses",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_ProductStatuses_SystemName",
                table: "ProductStatuses",
                newName: "IX_ProductStatuses_Code");

            migrationBuilder.RenameColumn(
                name: "SystemName",
                table: "AddressTypes",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_AddressTypes_SystemName",
                table: "AddressTypes",
                newName: "IX_AddressTypes_Code");
        }
    }
}
