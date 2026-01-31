using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInquiryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductInquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BuyerVendorId = table.Column<int>(type: "integer", nullable: false),
                    SellerVendorId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SpecialRequirements = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DeliveryCountryId = table.Column<int>(type: "integer", nullable: true),
                    DeliveryCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DesiredDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsReadBySeller = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductInquiries_Countries_DeliveryCountryId",
                        column: x => x.DeliveryCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductInquiries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductInquiries_Vendors_BuyerVendorId",
                        column: x => x.BuyerVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductInquiries_Vendors_SellerVendorId",
                        column: x => x.SellerVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductInquiryResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InquiryId = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "TRY"),
                    OfferedQuantity = table.Column<int>(type: "integer", nullable: true),
                    OfferedUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ProductInquiryResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductInquiryResponses_ProductInquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalTable: "ProductInquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiries_BuyerVendorId",
                table: "ProductInquiries",
                column: "BuyerVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiries_CreatedAt",
                table: "ProductInquiries",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiries_DeliveryCountryId",
                table: "ProductInquiries",
                column: "DeliveryCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiries_ProductId",
                table: "ProductInquiries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiries_SellerVendorId",
                table: "ProductInquiries",
                column: "SellerVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiries_Status",
                table: "ProductInquiries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiryResponses_InquiryId",
                table: "ProductInquiryResponses",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInquiryResponses_Status",
                table: "ProductInquiryResponses",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductInquiryResponses");

            migrationBuilder.DropTable(
                name: "ProductInquiries");
        }
    }
}
