using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddIsMenuSectionToCapabilityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MenuSection",
                table: "CapabilityModules");

            migrationBuilder.AddColumn<bool>(
                name: "IsMenuSection",
                table: "CapabilityModules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMenuSection",
                table: "CapabilityModules");

            migrationBuilder.AddColumn<string>(
                name: "MenuSection",
                table: "CapabilityModules",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
