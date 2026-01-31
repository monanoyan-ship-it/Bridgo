using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuoteEstimatedDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "OrderServiceQuotes");

            migrationBuilder.DropColumn(
                name: "EstimatedPickupDate",
                table: "OrderServiceQuotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "OrderServiceQuotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedPickupDate",
                table: "OrderServiceQuotes",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
