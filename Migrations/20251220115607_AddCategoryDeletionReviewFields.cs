using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDeletionReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeletionStatus",
                table: "ProductCategories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MigratedProductIds",
                table: "ProductCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "ProductCategories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ProductCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "ProductCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionStatus",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "MigratedProductIds",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "ProductCategories");
        }
    }
}
