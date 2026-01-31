using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceProductInquiryAndTeamMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInquiries_Countries_DeliveryCountryId",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "DeliveryCity",
                table: "ProductInquiries");

            migrationBuilder.RenameColumn(
                name: "DeliveryCountryId",
                table: "ProductInquiries",
                newName: "DeliveryAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductInquiries_DeliveryCountryId",
                table: "ProductInquiries",
                newName: "IX_ProductInquiries_DeliveryAddressId");

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferValidUntil",
                table: "ProductInquiries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferedCurrency",
                table: "ProductInquiries",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfferedPrice",
                table: "ProductInquiries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInquiries_Addresses_DeliveryAddressId",
                table: "ProductInquiries",
                column: "DeliveryAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInquiries_Addresses_DeliveryAddressId",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "OfferValidUntil",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "OfferedCurrency",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "OfferedPrice",
                table: "ProductInquiries");

            migrationBuilder.RenameColumn(
                name: "DeliveryAddressId",
                table: "ProductInquiries",
                newName: "DeliveryCountryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductInquiries_DeliveryAddressId",
                table: "ProductInquiries",
                newName: "IX_ProductInquiries_DeliveryCountryId");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "ProductInquiries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "ProductInquiries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "ProductInquiries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "ProductInquiries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCity",
                table: "ProductInquiries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInquiries_Countries_DeliveryCountryId",
                table: "ProductInquiries",
                column: "DeliveryCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
