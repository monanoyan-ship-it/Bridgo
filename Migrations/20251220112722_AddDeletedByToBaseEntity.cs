using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Warehouses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorServiceConnections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorCapabilityMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorCapabilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VendorBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "UserSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SupplierProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SubscriptionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SubscriptionInvoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "StripePayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "StateTranslations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "States",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PublicDemands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductWarehouseStocks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductPriceTiers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductInquiryResponses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductInquiries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PlatformModules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderStatusHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderShipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderShipmentItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderServiceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderServiceQuotes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderParticipants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderInvestments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Languages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DemandResponses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DemandResponseAttachments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DemandModifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DemandAttachments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CountryTranslations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Countries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CompanyRoleUserMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CompanyRoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CompanyRoleModulePermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CategorySubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CategoryRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Branches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Addresses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorServiceConnections");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorCapabilityMappings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorCapabilities");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VendorBankAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SupplierProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SubscriptionInvoices");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StateTranslations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "States");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PublicDemands");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductWarehouseStocks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductPriceTiers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductInquiryResponses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductInquiries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PlatformModules");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderTasks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderStatusHistory");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderShipments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderShipmentItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderServiceQuotes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderParticipants");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderInvestments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DemandResponses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DemandResponseAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DemandModifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DemandAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CountryTranslations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CompanyRoleUserMappings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CompanyRoles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CompanyRoleModulePermissions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CategorySubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CategoryRequests");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Addresses");
        }
    }
}
