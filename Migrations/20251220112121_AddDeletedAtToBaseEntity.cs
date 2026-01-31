using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Warehouses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorTeamMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorServiceConnections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorCapabilityMappings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorCapabilities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VendorBankAccounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SupplierProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SubscriptionPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SubscriptionInvoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StripePayments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StateTranslations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "States",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PublicDemands",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductWarehouseStocks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductPriceTiers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductInquiryResponses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductInquiries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductImages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PlatformModules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderStatusHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderShipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderShipmentItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderServiceRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderServiceQuotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderParticipants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderInvestments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Languages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DemandResponses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DemandResponseAttachments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DemandModifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DemandAttachments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CountryTranslations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Countries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CompanyRoleUserMappings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CompanyRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CompanyRoleModulePermissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CategorySubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CategoryRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Branches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Addresses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorServiceConnections");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorCapabilityMappings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorCapabilities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VendorBankAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SupplierProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SubscriptionInvoices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StateTranslations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "States");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PublicDemands");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductWarehouseStocks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductPriceTiers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductInquiryResponses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PlatformModules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderStatusHistory");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderShipmentItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderServiceQuotes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderParticipants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderInvestments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DemandResponses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DemandResponseAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DemandModifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DemandAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CountryTranslations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CompanyRoleUserMappings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CompanyRoles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CompanyRoleModulePermissions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CategorySubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CategoryRequests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Addresses");
        }
    }
}
