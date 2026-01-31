using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedDocumentAndSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    OrderServiceRequestId = table.Column<int>(type: "integer", nullable: true),
                    FinancingRequestId = table.Column<int>(type: "integer", nullable: true),
                    DocumentTypeId = table.Column<int>(type: "integer", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedByVendorId = table.Column<int>(type: "integer", nullable: false),
                    CounterpartyVendorId = table.Column<int>(type: "integer", nullable: true),
                    RequiredSignatures = table.Column<int>(type: "integer", nullable: false),
                    CompletedSignatures = table.Column<int>(type: "integer", nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_GeneratedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_FinancingRequests_FinancingRequestId",
                        column: x => x.FinancingRequestId,
                        principalTable: "FinancingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_OrderServiceRequests_OrderServiceRequest~",
                        column: x => x.OrderServiceRequestId,
                        principalTable: "OrderServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_Vendors_CounterpartyVendorId",
                        column: x => x.CounterpartyVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_Vendors_CreatedByVendorId",
                        column: x => x.CreatedByVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSignatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneratedDocumentId = table.Column<int>(type: "integer", nullable: false),
                    SignerVendorId = table.Column<int>(type: "integer", nullable: false),
                    SignerUserId = table.Column<int>(type: "integer", nullable: false),
                    SignatureType = table.Column<int>(type: "integer", nullable: false),
                    SignatureData = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CertificateInfo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VerificationCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_DocumentSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSignatures_GeneratedDocuments_GeneratedDocumentId",
                        column: x => x.GeneratedDocumentId,
                        principalTable: "GeneratedDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentSignatures_Users_SignerUserId",
                        column: x => x.SignerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentSignatures_Vendors_SignerVendorId",
                        column: x => x.SignerVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignatures_GeneratedDocumentId",
                table: "DocumentSignatures",
                column: "GeneratedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignatures_GeneratedDocumentId_SignerVendorId",
                table: "DocumentSignatures",
                columns: new[] { "GeneratedDocumentId", "SignerVendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignatures_SignerUserId",
                table: "DocumentSignatures",
                column: "SignerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignatures_SignerVendorId",
                table: "DocumentSignatures",
                column: "SignerVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignatures_VerificationCode",
                table: "DocumentSignatures",
                column: "VerificationCode",
                unique: true,
                filter: "\"VerificationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_CounterpartyVendorId",
                table: "GeneratedDocuments",
                column: "CounterpartyVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_CreatedByVendorId",
                table: "GeneratedDocuments",
                column: "CreatedByVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_DocumentNumber",
                table: "GeneratedDocuments",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_DocumentTypeId",
                table: "GeneratedDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_FinancingRequestId",
                table: "GeneratedDocuments",
                column: "FinancingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_OrderId",
                table: "GeneratedDocuments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_OrderId_DocumentTypeId",
                table: "GeneratedDocuments",
                columns: new[] { "OrderId", "DocumentTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_OrderServiceRequestId",
                table: "GeneratedDocuments",
                column: "OrderServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_Status",
                table: "GeneratedDocuments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentSignatures");

            migrationBuilder.DropTable(
                name: "GeneratedDocuments");
        }
    }
}
