using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddLockingMechanism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "OrderServiceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "OrderServiceRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LockedByDocumentId",
                table: "OrderServiceRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_LockedByDocumentId",
                table: "OrderServiceRequests",
                column: "LockedByDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceRequests_GeneratedDocuments_LockedByDocumentId",
                table: "OrderServiceRequests",
                column: "LockedByDocumentId",
                principalTable: "GeneratedDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderServiceRequests_GeneratedDocuments_LockedByDocumentId",
                table: "OrderServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_OrderServiceRequests_LockedByDocumentId",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "LockedByDocumentId",
                table: "OrderServiceRequests");
        }
    }
}
