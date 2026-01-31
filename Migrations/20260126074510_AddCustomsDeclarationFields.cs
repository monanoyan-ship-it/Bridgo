using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomsDeclarationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonTaxNo",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsBrokerCode",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceId",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EoriNumber",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KepAddress",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CifValue",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsOfficeCode",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsRegimeCode",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeclarationDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclarationNumber",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExchangeRateDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExportType",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FobValue",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightAmount",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceAmount",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StatisticalValue",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportIdentity",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportModeCode",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CifTotalPrice",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfOrigin",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExemptionCode",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FobTotalPrice",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FobUnitPrice",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HSCode",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductDescription",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StatisticalValue",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupplementaryQuantity",
                table: "OrderItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplementaryUnit",
                table: "OrderItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizedPersonTaxNo",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CustomsBrokerCode",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "EInvoiceId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "EoriNumber",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "KepAddress",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CifValue",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomsOfficeCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomsRegimeCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeclarationDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeclarationNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExchangeRateDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExportType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FobValue",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FreightAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InsuranceAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StatisticalValue",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TransportIdentity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TransportModeCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CifTotalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CountryOfOrigin",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ExemptionCode",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FobTotalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FobUnitPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "HSCode",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductDescription",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "StatisticalValue",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SupplementaryQuantity",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SupplementaryUnit",
                table: "OrderItems");
        }
    }
}
