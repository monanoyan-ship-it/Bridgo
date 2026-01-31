using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceTeamMemberIdentityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Addresses_DeliveryAddressId",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "Citizenship",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryOfBirthId",
                table: "VendorTeamMembers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "VendorTeamMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdDocumentNumber",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdDocumentTypeId",
                table: "VendorTeamMembers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAddressId",
                table: "Carts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellerVendorId",
                table: "Carts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceWarehouseId",
                table: "Carts",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryAddressId",
                table: "CartItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "SourceWarehouseId",
                table: "CartItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorTeamMembers_CountryOfBirthId",
                table: "VendorTeamMembers",
                column: "CountryOfBirthId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_DeliveryAddressId",
                table: "Carts",
                column: "DeliveryAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_SellerVendorId",
                table: "Carts",
                column: "SellerVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_SourceWarehouseId",
                table: "Carts",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_SourceWarehouseId",
                table: "CartItems",
                column: "SourceWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Addresses_DeliveryAddressId",
                table: "CartItems",
                column: "DeliveryAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Warehouses_SourceWarehouseId",
                table: "CartItems",
                column: "SourceWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Addresses_DeliveryAddressId",
                table: "Carts",
                column: "DeliveryAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Vendors_SellerVendorId",
                table: "Carts",
                column: "SellerVendorId",
                principalTable: "Vendors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Warehouses_SourceWarehouseId",
                table: "Carts",
                column: "SourceWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorTeamMembers_Countries_CountryOfBirthId",
                table: "VendorTeamMembers",
                column: "CountryOfBirthId",
                principalTable: "Countries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Addresses_DeliveryAddressId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Warehouses_SourceWarehouseId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Addresses_DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Vendors_SellerVendorId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Warehouses_SourceWarehouseId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorTeamMembers_Countries_CountryOfBirthId",
                table: "VendorTeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_VendorTeamMembers_CountryOfBirthId",
                table: "VendorTeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Carts_DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_SellerVendorId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_SourceWarehouseId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_SourceWarehouseId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Citizenship",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "CountryOfBirthId",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "IdDocumentNumber",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "IdDocumentTypeId",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SellerVendorId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SourceWarehouseId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SourceWarehouseId",
                table: "CartItems");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryAddressId",
                table: "CartItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Addresses_DeliveryAddressId",
                table: "CartItems",
                column: "DeliveryAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
