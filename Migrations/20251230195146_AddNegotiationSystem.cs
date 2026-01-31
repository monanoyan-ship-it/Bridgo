using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddNegotiationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentRoundNumber",
                table: "DemandResponses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentTurnVendorId",
                table: "DemandResponses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNegotiationActive",
                table: "DemandResponses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NegotiationExpiresAt",
                table: "DemandResponses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NegotiationRounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandResponseId = table.Column<int>(type: "integer", nullable: false),
                    InitiatorVendorId = table.Column<int>(type: "integer", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NegotiationRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NegotiationRounds_DemandResponses_DemandResponseId",
                        column: x => x.DemandResponseId,
                        principalTable: "DemandResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NegotiationRounds_Vendors_InitiatorVendorId",
                        column: x => x.InitiatorVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemandResponses_CurrentTurnVendorId",
                table: "DemandResponses",
                column: "CurrentTurnVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_NegotiationRounds_DemandResponseId",
                table: "NegotiationRounds",
                column: "DemandResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_NegotiationRounds_DemandResponseId_RoundNumber",
                table: "NegotiationRounds",
                columns: new[] { "DemandResponseId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NegotiationRounds_InitiatorVendorId",
                table: "NegotiationRounds",
                column: "InitiatorVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_NegotiationRounds_Status_ExpiresAt",
                table: "NegotiationRounds",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_DemandResponses_Vendors_CurrentTurnVendorId",
                table: "DemandResponses",
                column: "CurrentTurnVendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandResponses_Vendors_CurrentTurnVendorId",
                table: "DemandResponses");

            migrationBuilder.DropTable(
                name: "NegotiationRounds");

            migrationBuilder.DropIndex(
                name: "IX_DemandResponses_CurrentTurnVendorId",
                table: "DemandResponses");

            migrationBuilder.DropColumn(
                name: "CurrentRoundNumber",
                table: "DemandResponses");

            migrationBuilder.DropColumn(
                name: "CurrentTurnVendorId",
                table: "DemandResponses");

            migrationBuilder.DropColumn(
                name: "IsNegotiationActive",
                table: "DemandResponses");

            migrationBuilder.DropColumn(
                name: "NegotiationExpiresAt",
                table: "DemandResponses");
        }
    }
}
