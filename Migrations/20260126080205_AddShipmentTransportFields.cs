using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentTransportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BorderCrossDate",
                table: "OrderShipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BorderVehicleCountryCode",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BorderVehicleIdentity",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarrierVendorId",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContainerCount",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContainerNumbers",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContainerType",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DischargePortCode",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DischargePortName",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasContainer",
                table: "OrderShipments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LoadingDate",
                table: "OrderShipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadingPortCode",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadingPortName",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageType",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SealNumbers",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceQuoteId",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGrossWeight",
                table: "OrderShipments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNetWeight",
                table: "OrderShipments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPackageCount",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVolume",
                table: "OrderShipments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransportDocumentDate",
                table: "OrderShipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportDocumentNumber",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportDocumentType",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportModeCode",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleCountryCode",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleIdentity",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleTypeCode",
                table: "OrderShipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipments_CarrierVendorId",
                table: "OrderShipments",
                column: "CarrierVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipments_ServiceQuoteId",
                table: "OrderShipments",
                column: "ServiceQuoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderShipments_OrderServiceQuotes_ServiceQuoteId",
                table: "OrderShipments",
                column: "ServiceQuoteId",
                principalTable: "OrderServiceQuotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderShipments_Vendors_CarrierVendorId",
                table: "OrderShipments",
                column: "CarrierVendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderShipments_OrderServiceQuotes_ServiceQuoteId",
                table: "OrderShipments");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderShipments_Vendors_CarrierVendorId",
                table: "OrderShipments");

            migrationBuilder.DropIndex(
                name: "IX_OrderShipments_CarrierVendorId",
                table: "OrderShipments");

            migrationBuilder.DropIndex(
                name: "IX_OrderShipments_ServiceQuoteId",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "BorderCrossDate",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "BorderVehicleCountryCode",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "BorderVehicleIdentity",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "CarrierVendorId",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "ContainerCount",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "ContainerNumbers",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "ContainerType",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "DischargePortCode",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "DischargePortName",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "HasContainer",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "LoadingDate",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "LoadingPortCode",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "LoadingPortName",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "PackageType",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "SealNumbers",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "ServiceQuoteId",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TotalGrossWeight",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TotalNetWeight",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TotalPackageCount",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TotalVolume",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TransportDocumentDate",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TransportDocumentNumber",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TransportDocumentType",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "TransportModeCode",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "VehicleCountryCode",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "VehicleIdentity",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "VehicleTypeCode",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "OrderShipments");
        }
    }
}
