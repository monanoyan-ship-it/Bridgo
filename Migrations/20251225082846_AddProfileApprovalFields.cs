using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationRequestedAt",
                table: "CapabilityProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "CapabilityProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "CapabilityProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByUserId",
                table: "CapabilityProfiles",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationRequestedAt",
                table: "CapabilityProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "CapabilityProfiles");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "CapabilityProfiles");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "CapabilityProfiles");
        }
    }
}
