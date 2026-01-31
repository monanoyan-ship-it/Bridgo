using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderFlowManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoCreated",
                table: "OrderServiceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DependsOnServiceRequestId",
                table: "OrderServiceRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TriggerSource",
                table: "OrderServiceRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AllServicesSelectedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinancingRequestId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresFinancing",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SurveyTriggerStatus",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_DependsOnServiceRequestId",
                table: "OrderServiceRequests",
                column: "DependsOnServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FinancingRequestId",
                table: "Orders",
                column: "FinancingRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_FinancingRequests_FinancingRequestId",
                table: "Orders",
                column: "FinancingRequestId",
                principalTable: "FinancingRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceRequests_OrderServiceRequests_DependsOnServiceR~",
                table: "OrderServiceRequests",
                column: "DependsOnServiceRequestId",
                principalTable: "OrderServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_FinancingRequests_FinancingRequestId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServiceRequests_OrderServiceRequests_DependsOnServiceR~",
                table: "OrderServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_OrderServiceRequests_DependsOnServiceRequestId",
                table: "OrderServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FinancingRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AutoCreated",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "DependsOnServiceRequestId",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "TriggerSource",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "AllServicesSelectedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinancingRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RequiresFinancing",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SurveyTriggerStatus",
                table: "Orders");
        }
    }
}
