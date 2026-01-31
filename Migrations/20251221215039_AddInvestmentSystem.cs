using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequesterVendorId = table.Column<int>(type: "integer", nullable: false),
                    FinancingType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxInterestRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    RepaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CollateralType = table.Column<int>(type: "integer", nullable: true),
                    CollateralDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RelatedOrderId = table.Column<int>(type: "integer", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DebtorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DebtorTaxNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RiskScore = table.Column<int>(type: "integer", nullable: true),
                    RiskNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OfferDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FundedAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_FinancingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancingRequests_Orders_RelatedOrderId",
                        column: x => x.RelatedOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinancingRequests_Vendors_RequesterVendorId",
                        column: x => x.RequesterVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FinancingRequestId = table.Column<int>(type: "integer", nullable: false),
                    InvestorVendorId = table.Column<int>(type: "integer", nullable: false),
                    InvestorUserId = table.Column<int>(type: "integer", nullable: false),
                    OfferedAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalRepaymentAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseByUserId = table.Column<int>(type: "integer", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsFundTransferred = table.Column<bool>(type: "boolean", nullable: false),
                    FundTransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransferReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRepaid = table.Column<bool>(type: "boolean", nullable: false),
                    RepaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RepaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_InvestmentOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentOffers_FinancingRequests_FinancingRequestId",
                        column: x => x.FinancingRequestId,
                        principalTable: "FinancingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestmentOffers_Users_InvestorUserId",
                        column: x => x.InvestorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentOffers_Users_ResponseByUserId",
                        column: x => x.ResponseByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InvestmentOffers_Vendors_InvestorVendorId",
                        column: x => x.InvestorVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancingRequests_FinancingType",
                table: "FinancingRequests",
                column: "FinancingType");

            migrationBuilder.CreateIndex(
                name: "IX_FinancingRequests_RelatedOrderId",
                table: "FinancingRequests",
                column: "RelatedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancingRequests_RequesterVendorId",
                table: "FinancingRequests",
                column: "RequesterVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancingRequests_Status",
                table: "FinancingRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinancingRequests_Status_CreatedAt",
                table: "FinancingRequests",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_FinancingRequestId",
                table: "InvestmentOffers",
                column: "FinancingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_FinancingRequestId_InvestorVendorId",
                table: "InvestmentOffers",
                columns: new[] { "FinancingRequestId", "InvestorVendorId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_InvestorUserId",
                table: "InvestmentOffers",
                column: "InvestorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_InvestorVendorId",
                table: "InvestmentOffers",
                column: "InvestorVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_InvestorVendorId_Status",
                table: "InvestmentOffers",
                columns: new[] { "InvestorVendorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_ResponseByUserId",
                table: "InvestmentOffers",
                column: "ResponseByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOffers_Status",
                table: "InvestmentOffers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentOffers");

            migrationBuilder.DropTable(
                name: "FinancingRequests");
        }
    }
}
