using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVendorCapabilitiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FK'leri kaldir (VendorCapabilities tablosuna referans veren)
            migrationBuilder.DropForeignKey(
                name: "FK_CapabilityModules_VendorCapabilities_CapabilityId",
                table: "CapabilityModules");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyRoles_VendorCapabilities_CapabilityId",
                table: "CompanyRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorCapabilityMappings_VendorCapabilities_CapabilityId",
                table: "VendorCapabilityMappings");

            // VendorCapabilities tablosunu kaldir
            migrationBuilder.DropTable(
                name: "VendorCapabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // VendorCapabilities tablosunu yeniden olustur
            migrationBuilder.CreateTable(
                name: "VendorCapabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorCapabilities", x => x.Id);
                });

            // FK'leri yeniden ekle
            migrationBuilder.AddForeignKey(
                name: "FK_CapabilityModules_VendorCapabilities_CapabilityId",
                table: "CapabilityModules",
                column: "CapabilityId",
                principalTable: "VendorCapabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyRoles_VendorCapabilities_CapabilityId",
                table: "CompanyRoles",
                column: "CapabilityId",
                principalTable: "VendorCapabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorCapabilityMappings_VendorCapabilities_CapabilityId",
                table: "VendorCapabilityMappings",
                column: "CapabilityId",
                principalTable: "VendorCapabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
