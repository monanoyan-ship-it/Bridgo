using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderServiceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractAcceptedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsContractAccepted",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OrderInvestments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    InvestorVendorId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PercentageOfTotal = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    InvestmentType = table.Column<int>(type: "integer", nullable: false),
                    ReturnRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ExpectedReturn = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    RepaymentDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsFunded = table.Column<bool>(type: "boolean", nullable: false),
                    FundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRepaid = table.Column<bool>(type: "boolean", nullable: false),
                    RepaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RepaidAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderInvestments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderInvestments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderInvestments_Vendors_InvestorVendorId",
                        column: x => x.InvestorVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ServiceQuoteId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsTaskCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    TaskCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderParticipants_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderParticipants_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DependsOnTaskId = table.Column<int>(type: "integer", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "integer", nullable: true),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReferenceData = table.Column<string>(type: "jsonb", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTasks_OrderParticipants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "OrderParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTasks_OrderTasks_DependsOnTaskId",
                        column: x => x.DependsOnTaskId,
                        principalTable: "OrderTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderTasks_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderServiceQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceRequestId = table.Column<int>(type: "integer", nullable: false),
                    ProviderVendorId = table.Column<int>(type: "integer", nullable: false),
                    QuoteAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IncludedServices = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AdditionalCosts = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedDays = table.Column<int>(type: "integer", nullable: true),
                    EstimatedPickupDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransportMode = table.Column<int>(type: "integer", nullable: true),
                    CarrierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransitStops = table.Column<int>(type: "integer", nullable: true),
                    CoverageDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Deductible = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsReadByBuyer = table.Column<bool>(type: "boolean", nullable: false),
                    ReadByBuyerAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderServiceQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderServiceQuotes_Vendors_ProviderVendorId",
                        column: x => x.ProviderVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ServiceType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    VolumeM3 = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    PackageCount = table.Column<int>(type: "integer", nullable: true),
                    CargoValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OriginCountryId = table.Column<int>(type: "integer", nullable: true),
                    OriginCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OriginAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DestinationCountryId = table.Column<int>(type: "integer", nullable: true),
                    DestinationCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransportMode = table.Column<int>(type: "integer", nullable: true),
                    Incoterms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CustomsOperationType = table.Column<int>(type: "integer", nullable: true),
                    HsCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InsuranceType = table.Column<int>(type: "integer", nullable: true),
                    DesiredPickupDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DesiredDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuoteDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SelectedQuoteId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderServiceRequests_Countries_DestinationCountryId",
                        column: x => x.DestinationCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderServiceRequests_Countries_OriginCountryId",
                        column: x => x.OriginCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderServiceRequests_OrderServiceQuotes_SelectedQuoteId",
                        column: x => x.SelectedQuoteId,
                        principalTable: "OrderServiceQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderServiceRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderInvestments_InvestorVendorId",
                table: "OrderInvestments",
                column: "InvestorVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderInvestments_OrderId",
                table: "OrderInvestments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderInvestments_OrderId_InvestorVendorId",
                table: "OrderInvestments",
                columns: new[] { "OrderId", "InvestorVendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderInvestments_Status",
                table: "OrderInvestments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderParticipants_OrderId",
                table: "OrderParticipants",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderParticipants_OrderId_VendorId_Role",
                table: "OrderParticipants",
                columns: new[] { "OrderId", "VendorId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderParticipants_ServiceQuoteId",
                table: "OrderParticipants",
                column: "ServiceQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderParticipants_Status",
                table: "OrderParticipants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderParticipants_VendorId",
                table: "OrderParticipants",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceQuotes_ProviderVendorId",
                table: "OrderServiceQuotes",
                column: "ProviderVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceQuotes_ServiceRequestId",
                table: "OrderServiceQuotes",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceQuotes_ServiceRequestId_ProviderVendorId",
                table: "OrderServiceQuotes",
                columns: new[] { "ServiceRequestId", "ProviderVendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceQuotes_Status",
                table: "OrderServiceQuotes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_DestinationCountryId",
                table: "OrderServiceRequests",
                column: "DestinationCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_OrderId",
                table: "OrderServiceRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_OrderId_ServiceType",
                table: "OrderServiceRequests",
                columns: new[] { "OrderId", "ServiceType" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_OriginCountryId",
                table: "OrderServiceRequests",
                column: "OriginCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_SelectedQuoteId",
                table: "OrderServiceRequests",
                column: "SelectedQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceRequests_Status",
                table: "OrderServiceRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTasks_DependsOnTaskId",
                table: "OrderTasks",
                column: "DependsOnTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTasks_OrderId",
                table: "OrderTasks",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTasks_OrderId_SortOrder",
                table: "OrderTasks",
                columns: new[] { "OrderId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderTasks_ParticipantId",
                table: "OrderTasks",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTasks_Status",
                table: "OrderTasks",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderParticipants_OrderServiceQuotes_ServiceQuoteId",
                table: "OrderParticipants",
                column: "ServiceQuoteId",
                principalTable: "OrderServiceQuotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceQuotes_OrderServiceRequests_ServiceRequestId",
                table: "OrderServiceQuotes",
                column: "ServiceRequestId",
                principalTable: "OrderServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderServiceRequests_OrderServiceQuotes_SelectedQuoteId",
                table: "OrderServiceRequests");

            migrationBuilder.DropTable(
                name: "OrderInvestments");

            migrationBuilder.DropTable(
                name: "OrderTasks");

            migrationBuilder.DropTable(
                name: "OrderParticipants");

            migrationBuilder.DropTable(
                name: "OrderServiceQuotes");

            migrationBuilder.DropTable(
                name: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "ContractAcceptedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsContractAccepted",
                table: "Orders");
        }
    }
}
