using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceTeamMemberForVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorizedSignatory",
                table: "VendorTeamMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalRepresentative",
                table: "VendorTeamMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MemberType",
                table: "VendorTeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SharePercentage",
                table: "VendorTeamMembers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNote",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatusId",
                table: "VendorTeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "VendorTeamMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedBy",
                table: "VendorTeamMembers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "IsAuthorizedSignatory",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "IsLegalRepresentative",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "MemberType",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "SharePercentage",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "VerificationNote",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "VerificationStatusId",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "VendorTeamMembers");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "VendorTeamMembers");
        }
    }
}
