using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToCapabilityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameResourceKey",
                table: "CapabilityModules",
                newName: "DisplayNameResourceKey");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "CapabilityModules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "CapabilityModules");

            migrationBuilder.RenameColumn(
                name: "DisplayNameResourceKey",
                table: "CapabilityModules",
                newName: "NameResourceKey");
        }
    }
}
