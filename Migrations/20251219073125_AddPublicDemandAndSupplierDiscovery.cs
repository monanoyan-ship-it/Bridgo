using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicDemandAndSupplierDiscovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategorySubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    NotifyByEmail = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyInApp = table.Column<bool>(type: "boolean", nullable: false),
                    MinQuantity = table.Column<int>(type: "integer", nullable: true),
                    CountryFilter = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KeywordFilter = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotificationCount = table.Column<int>(type: "integer", nullable: false),
                    LastNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySubscriptions_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategorySubscriptions_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicDemands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceProductId = table.Column<int>(type: "integer", nullable: true),
                    ModificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DesiredLeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    DesiredDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BudgetMin = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    BudgetMax = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    BudgetCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsIndexable = table.Column<bool>(type: "boolean", nullable: false),
                    MetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    ResponseCount = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicDemands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicDemands_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PublicDemands_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PublicDemands_Products_ReferenceProductId",
                        column: x => x.ReferenceProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PublicDemands_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Capabilities = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProductionCapacity = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MinimumOrderValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LeadTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoryIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Certifications = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ServiceRegions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PublicPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicWebsite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GalleryImages = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierProfiles_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplierProfiles_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandAttachments_PublicDemands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "PublicDemands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<int>(type: "integer", nullable: false),
                    PropertyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginalValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DesiredValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandModifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandModifications_PublicDemands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "PublicDemands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<int>(type: "integer", nullable: false),
                    SupplierVendorId = table.Column<int>(type: "integer", nullable: true),
                    ExternalCompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalWebsite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandResponses_PublicDemands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "PublicDemands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandResponses_Vendors_SupplierVendorId",
                        column: x => x.SupplierVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DemandResponseAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponseId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandResponseAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandResponseAttachments_DemandResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "DemandResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySubscriptions_CategoryId",
                table: "CategorySubscriptions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySubscriptions_VendorId_CategoryId",
                table: "CategorySubscriptions",
                columns: new[] { "VendorId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandAttachments_DemandId",
                table: "DemandAttachments",
                column: "DemandId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandModifications_DemandId",
                table: "DemandModifications",
                column: "DemandId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandResponseAttachments_ResponseId",
                table: "DemandResponseAttachments",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandResponses_DemandId_SupplierVendorId",
                table: "DemandResponses",
                columns: new[] { "DemandId", "SupplierVendorId" });

            migrationBuilder.CreateIndex(
                name: "IX_DemandResponses_Status",
                table: "DemandResponses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DemandResponses_SupplierVendorId",
                table: "DemandResponses",
                column: "SupplierVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicDemands_CategoryId",
                table: "PublicDemands",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicDemands_CountryId",
                table: "PublicDemands",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicDemands_ReferenceProductId",
                table: "PublicDemands",
                column: "ReferenceProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicDemands_Slug",
                table: "PublicDemands",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicDemands_Status_Visibility",
                table: "PublicDemands",
                columns: new[] { "Status", "Visibility" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicDemands_VendorId_Status",
                table: "PublicDemands",
                columns: new[] { "VendorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProfiles_CountryId",
                table: "SupplierProfiles",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProfiles_IsPubliclyVisible",
                table: "SupplierProfiles",
                column: "IsPubliclyVisible");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProfiles_Slug",
                table: "SupplierProfiles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProfiles_VendorId",
                table: "SupplierProfiles",
                column: "VendorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorySubscriptions");

            migrationBuilder.DropTable(
                name: "DemandAttachments");

            migrationBuilder.DropTable(
                name: "DemandModifications");

            migrationBuilder.DropTable(
                name: "DemandResponseAttachments");

            migrationBuilder.DropTable(
                name: "SupplierProfiles");

            migrationBuilder.DropTable(
                name: "DemandResponses");

            migrationBuilder.DropTable(
                name: "PublicDemands");
        }
    }
}
