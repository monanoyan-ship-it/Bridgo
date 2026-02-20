using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddFaz4SocialFeedAdvanced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialPostHashtags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialPostId = table.Column<int>(type: "integer", nullable: false),
                    Tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_SocialPostHashtags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPostHashtags_SocialPosts_SocialPostId",
                        column: x => x.SocialPostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialPostReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialPostId = table.Column<int>(type: "integer", nullable: false),
                    ReporterUserId = table.Column<int>(type: "integer", nullable: false),
                    ReporterVendorId = table.Column<int>(type: "integer", nullable: false),
                    ReasonId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    AdminNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_SocialPostReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPostReports_SocialPosts_SocialPostId",
                        column: x => x.SocialPostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocialPostReports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPostReports_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPostReports_Vendors_ReporterVendorId",
                        column: x => x.ReporterVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SponsoredPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialPostId = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SpentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TargetImpressions = table.Column<int>(type: "integer", nullable: false),
                    CurrentImpressions = table.Column<int>(type: "integer", nullable: false),
                    CurrentClicks = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_SponsoredPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsoredPosts_SocialPosts_SocialPostId",
                        column: x => x.SocialPostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SponsoredPosts_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostHashtags_SocialPostId",
                table: "SocialPostHashtags",
                column: "SocialPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostHashtags_Tag",
                table: "SocialPostHashtags",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostHashtags_Tag_CreatedAt",
                table: "SocialPostHashtags",
                columns: new[] { "Tag", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostReports_ReporterUserId",
                table: "SocialPostReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostReports_ReporterVendorId",
                table: "SocialPostReports",
                column: "ReporterVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostReports_ReviewedByUserId",
                table: "SocialPostReports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostReports_SocialPostId",
                table: "SocialPostReports",
                column: "SocialPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostReports_StatusId_CreatedAt",
                table: "SocialPostReports",
                columns: new[] { "StatusId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsoredPosts_SocialPostId",
                table: "SponsoredPosts",
                column: "SocialPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsoredPosts_StatusId",
                table: "SponsoredPosts",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsoredPosts_StatusId_EndDate",
                table: "SponsoredPosts",
                columns: new[] { "StatusId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsoredPosts_VendorId",
                table: "SponsoredPosts",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialPostHashtags");

            migrationBuilder.DropTable(
                name: "SocialPostReports");

            migrationBuilder.DropTable(
                name: "SponsoredPosts");
        }
    }
}
