using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformModuleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CapabilityModules_CapabilityId_Code",
                table: "CapabilityModules");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayNameResourceKey",
                table: "CapabilityModules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "CapabilityModules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CapabilityModules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "PlatformModuleId",
                table: "CapabilityModules",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModules_CapabilityId_PlatformModuleId",
                table: "CapabilityModules",
                columns: new[] { "CapabilityId", "PlatformModuleId" },
                unique: true,
                filter: "\"PlatformModuleId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModules_PlatformModuleId",
                table: "CapabilityModules",
                column: "PlatformModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CapabilityModules_CapabilityModules_PlatformModuleId",
                table: "CapabilityModules",
                column: "PlatformModuleId",
                principalTable: "CapabilityModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapabilityModules_CapabilityModules_PlatformModuleId",
                table: "CapabilityModules");

            migrationBuilder.DropIndex(
                name: "IX_CapabilityModules_CapabilityId_PlatformModuleId",
                table: "CapabilityModules");

            migrationBuilder.DropIndex(
                name: "IX_CapabilityModules_PlatformModuleId",
                table: "CapabilityModules");

            migrationBuilder.DropColumn(
                name: "PlatformModuleId",
                table: "CapabilityModules");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayNameResourceKey",
                table: "CapabilityModules",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "CapabilityModules",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CapabilityModules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityModules_CapabilityId_Code",
                table: "CapabilityModules",
                columns: new[] { "CapabilityId", "Code" },
                unique: true);
        }
    }
}
