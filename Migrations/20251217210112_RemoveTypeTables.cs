using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTypeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_AddressTypes_AddressTypeId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductStatuses_ProductStatusId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_VendorStatuses_VendorStatusId",
                table: "Vendors");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorTeamMembers_TeamMemberStatuses_TeamMemberStatusId",
                table: "VendorTeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_WarehouseTypes_WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.DropTable(
                name: "AddressTypes");

            migrationBuilder.DropTable(
                name: "ProductStatuses");

            migrationBuilder.DropTable(
                name: "TeamMemberStatuses");

            migrationBuilder.DropTable(
                name: "VendorStatuses");

            migrationBuilder.DropTable(
                name: "WarehouseTypes");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_VendorTeamMembers_TeamMemberStatusId",
                table: "VendorTeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_VendorStatusId",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductStatusId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_AddressTypeId",
                table: "Addresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamMemberStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMemberStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses",
                column: "WarehouseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorTeamMembers_TeamMemberStatusId",
                table: "VendorTeamMembers",
                column: "TeamMemberStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorStatusId",
                table: "Vendors",
                column: "VendorStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductStatusId",
                table: "Products",
                column: "ProductStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressTypeId",
                table: "Addresses",
                column: "AddressTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressTypes_SystemName",
                table: "AddressTypes",
                column: "SystemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatuses_SystemName",
                table: "ProductStatuses",
                column: "SystemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberStatuses_SystemName",
                table: "TeamMemberStatuses",
                column: "SystemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorStatuses_SystemName",
                table: "VendorStatuses",
                column: "SystemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTypes_SystemName",
                table: "WarehouseTypes",
                column: "SystemName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_AddressTypes_AddressTypeId",
                table: "Addresses",
                column: "AddressTypeId",
                principalTable: "AddressTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductStatuses_ProductStatusId",
                table: "Products",
                column: "ProductStatusId",
                principalTable: "ProductStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_VendorStatuses_VendorStatusId",
                table: "Vendors",
                column: "VendorStatusId",
                principalTable: "VendorStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorTeamMembers_TeamMemberStatuses_TeamMemberStatusId",
                table: "VendorTeamMembers",
                column: "TeamMemberStatusId",
                principalTable: "TeamMemberStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_WarehouseTypes_WarehouseTypeId",
                table: "Warehouses",
                column: "WarehouseTypeId",
                principalTable: "WarehouseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
