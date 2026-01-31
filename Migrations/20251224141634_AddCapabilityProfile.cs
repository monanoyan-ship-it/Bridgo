using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCapabilityProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapabilityProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    CapabilityId = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Services = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Capabilities = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Certifications = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ServiceRegions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CategoryIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProductionCapacity = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MinimumOrderValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LeadTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Industry = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BusinessType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreferredCategories = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AnnualPurchaseVolume = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchaseVolumeCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FleetInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransportModes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Routes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InsuranceTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverageTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaxCoverageAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxCoverageCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CustomsServices = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LicenseNumbers = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomsOffices = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InspectionTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InspectionStandards = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InvestmentTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InvestmentFocus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MinInvestmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxInvestmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    InvestmentCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    InterestRateRange = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PublicPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicWebsite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GalleryImages = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPubliclyVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptingNewRequests = table.Column<bool>(type: "boolean", nullable: false),
                    ShowContactInfo = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    ContactRequestCount = table.Column<int>(type: "integer", nullable: false),
                    ResponseCount = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    RatingCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CapabilityProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapabilityProfiles_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CapabilityProfiles_VendorCapabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "VendorCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapabilityProfiles_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityProfiles_CapabilityId",
                table: "CapabilityProfiles",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityProfiles_CountryId",
                table: "CapabilityProfiles",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityProfiles_Slug",
                table: "CapabilityProfiles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityProfiles_VendorId_CapabilityId",
                table: "CapabilityProfiles",
                columns: new[] { "VendorId", "CapabilityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapabilityProfiles");
        }
    }
}
