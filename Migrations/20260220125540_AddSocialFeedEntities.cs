using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialFeedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    AuthorUserId = table.Column<int>(type: "integer", nullable: false),
                    PostTypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_SocialPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPosts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SocialPosts_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPosts_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorFollows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerVendorId = table.Column<int>(type: "integer", nullable: false),
                    FollowedVendorId = table.Column<int>(type: "integer", nullable: false),
                    FollowedByUserId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_VendorFollows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorFollows_Users_FollowedByUserId",
                        column: x => x.FollowedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorFollows_Vendors_FollowedVendorId",
                        column: x => x.FollowedVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorFollows_Vendors_FollowerVendorId",
                        column: x => x.FollowerVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialPostComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialPostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_SocialPostComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPostComments_SocialPostComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "SocialPostComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPostComments_SocialPosts_SocialPostId",
                        column: x => x.SocialPostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocialPostComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPostComments_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SocialPostImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialPostId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AltText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_SocialPostImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPostImages_SocialPosts_SocialPostId",
                        column: x => x.SocialPostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialPostLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialPostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_SocialPostLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPostLikes_SocialPosts_SocialPostId",
                        column: x => x.SocialPostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocialPostLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPostLikes_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostComments_ParentCommentId",
                table: "SocialPostComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostComments_SocialPostId",
                table: "SocialPostComments",
                column: "SocialPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostComments_SocialPostId_CreatedAt",
                table: "SocialPostComments",
                columns: new[] { "SocialPostId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostComments_UserId",
                table: "SocialPostComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostComments_VendorId",
                table: "SocialPostComments",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostImages_SocialPostId_DisplayOrder",
                table: "SocialPostImages",
                columns: new[] { "SocialPostId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostLikes_SocialPostId",
                table: "SocialPostLikes",
                column: "SocialPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostLikes_SocialPostId_UserId",
                table: "SocialPostLikes",
                columns: new[] { "SocialPostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostLikes_UserId",
                table: "SocialPostLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostLikes_VendorId",
                table: "SocialPostLikes",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_AuthorUserId",
                table: "SocialPosts",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_ProductId",
                table: "SocialPosts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_PublishedAt",
                table: "SocialPosts",
                column: "PublishedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_StatusId_PublishedAt",
                table: "SocialPosts",
                columns: new[] { "StatusId", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_VendorId_StatusId",
                table: "SocialPosts",
                columns: new[] { "VendorId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendorFollows_FollowedByUserId",
                table: "VendorFollows",
                column: "FollowedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorFollows_FollowedVendorId",
                table: "VendorFollows",
                column: "FollowedVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorFollows_FollowerVendorId",
                table: "VendorFollows",
                column: "FollowerVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorFollows_FollowerVendorId_FollowedVendorId",
                table: "VendorFollows",
                columns: new[] { "FollowerVendorId", "FollowedVendorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialPostComments");

            migrationBuilder.DropTable(
                name: "SocialPostImages");

            migrationBuilder.DropTable(
                name: "SocialPostLikes");

            migrationBuilder.DropTable(
                name: "VendorFollows");

            migrationBuilder.DropTable(
                name: "SocialPosts");
        }
    }
}
