using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransportModeToTransportModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransportMode",
                table: "OrderServiceQuotes");

            migrationBuilder.AddColumn<string>(
                name: "TransportModes",
                table: "OrderServiceQuotes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransportModes",
                table: "OrderServiceQuotes");

            migrationBuilder.AddColumn<int>(
                name: "TransportMode",
                table: "OrderServiceQuotes",
                type: "integer",
                nullable: true);
        }
    }
}
