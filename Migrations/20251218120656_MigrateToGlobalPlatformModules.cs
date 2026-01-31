using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToGlobalPlatformModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski izinleri temizle - yeni yapida ID'ler farkli olacak
            // RbacSeeder yeni izinleri olusturacak
            migrationBuilder.Sql("DELETE FROM \"CompanyRoleModulePermissions\"");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyRoleModulePermissions_CapabilityModules_ModuleId",
                table: "CompanyRoleModulePermissions");

            migrationBuilder.DropTable(
                name: "CapabilityModules");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "CompanyRoleModulePermissions",
                newName: "PlatformModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyRoleModulePermissions_ModuleId",
                table: "CompanyRoleModulePermissions",
                newName: "IX_CompanyRoleModulePermissions_PlatformModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyRoleModulePermissions_CompanyRoleId_ModuleId",
                table: "CompanyRoleModulePermissions",
                newName: "IX_CompanyRoleModulePermissions_CompanyRoleId_PlatformModuleId");

            migrationBuilder.CreateTable(
                name: "PlatformModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayNameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Route = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsMenuItem = table.Column<bool>(type: "boolean", nullable: false),
                    IsMenuSection = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformModules_PlatformModules_ParentId",
                        column: x => x.ParentId,
                        principalTable: "PlatformModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CapabilityModuleMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapabilityId = table.Column<int>(type: "integer", nullable: false),
                    PlatformModuleId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapabilityModuleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapabilityModuleMappings_PlatformModules_PlatformModuleId",
                        column: x => x.PlatformModuleId,
                        principalTable: "PlatformModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapabilityModuleMappings_VendorCapabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "VendorCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModuleMappings_CapabilityId_PlatformModuleId",
                table: "CapabilityModuleMappings",
                columns: new[] { "CapabilityId", "PlatformModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModuleMappings_PlatformModuleId",
                table: "CapabilityModuleMappings",
                column: "PlatformModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformModules_DisplayNameResourceKey",
                table: "PlatformModules",
                column: "DisplayNameResourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformModules_ParentId",
                table: "PlatformModules",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyRoleModulePermissions_PlatformModules_PlatformModule~",
                table: "CompanyRoleModulePermissions",
                column: "PlatformModuleId",
                principalTable: "PlatformModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyRoleModulePermissions_PlatformModules_PlatformModule~",
                table: "CompanyRoleModulePermissions");

            migrationBuilder.DropTable(
                name: "CapabilityModuleMappings");

            migrationBuilder.DropTable(
                name: "PlatformModules");

            migrationBuilder.RenameColumn(
                name: "PlatformModuleId",
                table: "CompanyRoleModulePermissions",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyRoleModulePermissions_PlatformModuleId",
                table: "CompanyRoleModulePermissions",
                newName: "IX_CompanyRoleModulePermissions_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyRoleModulePermissions_CompanyRoleId_PlatformModuleId",
                table: "CompanyRoleModulePermissions",
                newName: "IX_CompanyRoleModulePermissions_CompanyRoleId_ModuleId");

            migrationBuilder.CreateTable(
                name: "CapabilityModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapabilityId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    PlatformModuleId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayNameResourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsMenuItem = table.Column<bool>(type: "boolean", nullable: false),
                    IsMenuSection = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Route = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapabilityModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapabilityModules_CapabilityModules_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CapabilityModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapabilityModules_CapabilityModules_PlatformModuleId",
                        column: x => x.PlatformModuleId,
                        principalTable: "CapabilityModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CapabilityModules_VendorCapabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "VendorCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModules_CapabilityId_PlatformModuleId",
                table: "CapabilityModules",
                columns: new[] { "CapabilityId", "PlatformModuleId" },
                unique: true,
                filter: "\"PlatformModuleId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModules_ParentId",
                table: "CapabilityModules",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModules_PlatformModuleId",
                table: "CapabilityModules",
                column: "PlatformModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyRoleModulePermissions_CapabilityModules_ModuleId",
                table: "CompanyRoleModulePermissions",
                column: "ModuleId",
                principalTable: "CapabilityModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
