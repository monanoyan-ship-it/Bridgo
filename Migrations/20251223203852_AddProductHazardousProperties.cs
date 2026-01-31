using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddProductHazardousProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DangerClassId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HandlingInstructions",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompressedGas",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrosive",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDangerous",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnvironmentalHazard",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExplosive",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlammable",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOxidizing",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRadioactive",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsToxic",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxStackingLayers",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackingGroupId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresColdChain",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresFragileHandling",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresStackingLimit",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UNNumber",
                table: "Products",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DangerClassId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "HandlingInstructions",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsCompressedGas",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsCorrosive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDangerous",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsEnvironmentalHazard",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsExplosive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFlammable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsOxidizing",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsRadioactive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsToxic",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaxStackingLayers",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackingGroupId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RequiresColdChain",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RequiresFragileHandling",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RequiresStackingLimit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UNNumber",
                table: "Products");
        }
    }
}
