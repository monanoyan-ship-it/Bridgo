using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomsDeclaration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomsDeclarations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    DeclarationTypeId = table.Column<int>(type: "integer", nullable: false),
                    EvrimDosyaNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EvrimDosyaTipi = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DeclarationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomsOfficeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CustomsOfficeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegimeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DeliveryPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxDue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TaxPaid = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    EvrimResponse = table.Column<string>(type: "text", nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_CustomsDeclarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomsDeclarations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarations_DeclarationNumber",
                table: "CustomsDeclarations",
                column: "DeclarationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarations_EvrimDosyaNo",
                table: "CustomsDeclarations",
                column: "EvrimDosyaNo");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarations_OrderId",
                table: "CustomsDeclarations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarations_OrderId_DeclarationTypeId",
                table: "CustomsDeclarations",
                columns: new[] { "OrderId", "DeclarationTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarations_StatusId",
                table: "CustomsDeclarations",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomsDeclarations");
        }
    }
}
