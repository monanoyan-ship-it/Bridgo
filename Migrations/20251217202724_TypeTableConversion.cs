using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class TypeTableConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "VendorTeamMembers",
                newName: "TeamMemberStatusId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Vendors",
                newName: "VendorStatusId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Products",
                newName: "ProductStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_VendorId_Status",
                table: "Products",
                newName: "IX_Products_VendorId_ProductStatusId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Addresses",
                newName: "AddressTypeId");

            migrationBuilder.AddColumn<string>(
                name: "AddressDescription",
                table: "Addresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "Addresses",
                type: "character varying(90)",
                maxLength: 90,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AddressTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false)
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
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false)
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
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false)
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
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false)
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
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WarehouseTypeId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AddressId = table.Column<int>(type: "integer", nullable: true),
                    TotalCapacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CapacityUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ManagerUserId = table.Column<int>(type: "integer", nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperatingHours = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Warehouses_Users_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Warehouses_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warehouses_WarehouseTypes_WarehouseTypeId",
                        column: x => x.WarehouseTypeId,
                        principalTable: "WarehouseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductWarehouseStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MinStockLevel = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxStockLevel = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ReorderPoint = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ReorderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    BinLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastStockCheckAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMovementAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarehouseStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductWarehouseStocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductWarehouseStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_AddressTypes_Code",
                table: "AddressTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatuses_Code",
                table: "ProductStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarehouseStocks_ProductId_WarehouseId",
                table: "ProductWarehouseStocks",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarehouseStocks_WarehouseId",
                table: "ProductWarehouseStocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberStatuses_Code",
                table: "TeamMemberStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorStatuses_Code",
                table: "VendorStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_AddressId",
                table: "Warehouses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_ManagerUserId",
                table: "Warehouses",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_VendorId_Code",
                table: "Warehouses",
                columns: new[] { "VendorId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses",
                column: "WarehouseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTypes_Code",
                table: "WarehouseTypes",
                column: "Code",
                unique: true);

            // Tip tablolarina seed data ekle
            // AddressTypes
            migrationBuilder.Sql(@"
                INSERT INTO ""AddressTypes"" (""Id"", ""Code"", ""NameResourceKey"", ""Description"", ""DisplayOrder"", ""IsActive"", ""IsSystem"") VALUES
                (1, 'Billing', 'AddressType.Billing', 'Fatura adresi', 1, true, true),
                (2, 'Shipping', 'AddressType.Shipping', 'Teslimat adresi', 2, true, true),
                (3, 'Headquarters', 'AddressType.Headquarters', 'Sirket merkezi', 3, true, true),
                (4, 'Warehouse', 'AddressType.Warehouse', 'Depo adresi', 4, true, true),
                (5, 'Branch', 'AddressType.Branch', 'Sube adresi', 5, true, true),
                (6, 'Return', 'AddressType.Return', 'Iade adresi', 6, true, true);
            ");

            // WarehouseTypes
            migrationBuilder.Sql(@"
                INSERT INTO ""WarehouseTypes"" (""Id"", ""Code"", ""NameResourceKey"", ""Description"", ""DisplayOrder"", ""IsActive"", ""IsSystem"") VALUES
                (1, 'Main', 'WarehouseType.Main', 'Ana depo', 1, true, true),
                (2, 'Distribution', 'WarehouseType.Distribution', 'Dagitim deposu', 2, true, true),
                (3, 'Transit', 'WarehouseType.Transit', 'Transit depo', 3, true, true),
                (4, 'Virtual', 'WarehouseType.Virtual', 'Sanal depo (dropship)', 4, true, true),
                (5, 'ReturnWarehouse', 'WarehouseType.ReturnWarehouse', 'Iade deposu', 5, true, true);
            ");

            // VendorStatuses
            migrationBuilder.Sql(@"
                INSERT INTO ""VendorStatuses"" (""Id"", ""Code"", ""NameResourceKey"", ""Description"", ""DisplayOrder"", ""IsActive"", ""IsSystem"") VALUES
                (1, 'PendingProfile', 'VendorStatus.PendingProfile', 'Profil tamamlanmayi bekliyor', 1, true, true),
                (2, 'PendingVerification', 'VendorStatus.PendingVerification', 'Dogrulama bekliyor', 2, true, true),
                (3, 'Active', 'VendorStatus.Active', 'Aktif', 3, true, true),
                (4, 'Suspended', 'VendorStatus.Suspended', 'Askiya alinmis', 4, true, true),
                (5, 'Rejected', 'VendorStatus.Rejected', 'Reddedilmis', 5, true, true);
            ");

            // ProductStatuses
            migrationBuilder.Sql(@"
                INSERT INTO ""ProductStatuses"" (""Id"", ""Code"", ""NameResourceKey"", ""Description"", ""DisplayOrder"", ""IsActive"", ""IsSystem"") VALUES
                (1, 'Draft', 'ProductStatus.Draft', 'Taslak', 1, true, true),
                (2, 'Active', 'ProductStatus.Active', 'Aktif', 2, true, true),
                (3, 'Inactive', 'ProductStatus.Inactive', 'Pasif', 3, true, true),
                (4, 'OutOfStock', 'ProductStatus.OutOfStock', 'Stokta yok', 4, true, true),
                (5, 'Discontinued', 'ProductStatus.Discontinued', 'Uretimden kalkti', 5, true, true);
            ");

            // TeamMemberStatuses
            migrationBuilder.Sql(@"
                INSERT INTO ""TeamMemberStatuses"" (""Id"", ""Code"", ""NameResourceKey"", ""Description"", ""DisplayOrder"", ""IsActive"", ""IsSystem"") VALUES
                (1, 'Pending', 'TeamMemberStatus.Pending', 'Beklemede (davet/istek)', 1, true, true),
                (2, 'Active', 'TeamMemberStatus.Active', 'Aktif ekip uyesi', 2, true, true),
                (3, 'Rejected', 'TeamMemberStatus.Rejected', 'Reddedildi', 3, true, true),
                (4, 'Cancelled', 'TeamMemberStatus.Cancelled', 'Iptal edildi', 4, true, true),
                (5, 'Expired', 'TeamMemberStatus.Expired', 'Suresi doldu (davet)', 5, true, true),
                (6, 'Left', 'TeamMemberStatus.Left', 'Ayrildi', 6, true, true);
            ");

            // Mevcut enum degerlerini (0-based) yeni ID'lere (1-based) cevir
            // AddressType: 0=Billing->1, 1=Shipping->2, 2=Headquarters->3, 3=Warehouse->4
            migrationBuilder.Sql(@"UPDATE ""Addresses"" SET ""AddressTypeId"" = ""AddressTypeId"" + 1;");

            // VendorStatus: 0=PendingProfile->1, 1=PendingVerification->2, 2=Active->3, 3=Suspended->4, 4=Rejected->5
            migrationBuilder.Sql(@"UPDATE ""Vendors"" SET ""VendorStatusId"" = ""VendorStatusId"" + 1;");

            // ProductStatus: 0=Draft->1, 1=Active->2, 2=Inactive->3, 3=OutOfStock->4, 4=Discontinued->5
            migrationBuilder.Sql(@"UPDATE ""Products"" SET ""ProductStatusId"" = ""ProductStatusId"" + 1;");

            // TeamMemberStatus: 0=Pending->1, 1=Active->2, 2=Rejected->3, 3=Cancelled->4, 4=Expired->5, 5=Left->6
            migrationBuilder.Sql(@"UPDATE ""VendorTeamMembers"" SET ""TeamMemberStatusId"" = ""TeamMemberStatusId"" + 1;");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropTable(
                name: "AddressTypes");

            migrationBuilder.DropTable(
                name: "ProductStatuses");

            migrationBuilder.DropTable(
                name: "ProductWarehouseStocks");

            migrationBuilder.DropTable(
                name: "TeamMemberStatuses");

            migrationBuilder.DropTable(
                name: "VendorStatuses");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "WarehouseTypes");

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

            migrationBuilder.DropColumn(
                name: "AddressDescription",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "TeamMemberStatusId",
                table: "VendorTeamMembers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "VendorStatusId",
                table: "Vendors",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ProductStatusId",
                table: "Products",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Products_VendorId_ProductStatusId",
                table: "Products",
                newName: "IX_Products_VendorId_Status");

            migrationBuilder.RenameColumn(
                name: "AddressTypeId",
                table: "Addresses",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Addresses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Addresses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Addresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
