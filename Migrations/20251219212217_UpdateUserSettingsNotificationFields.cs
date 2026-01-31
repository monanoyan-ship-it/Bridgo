using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSettingsNotificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EmailOnNewOrder = table.Column<bool>(type: "boolean", nullable: false),
                    EmailOnOrderStatusChange = table.Column<bool>(type: "boolean", nullable: false),
                    EmailOnNewInquiry = table.Column<bool>(type: "boolean", nullable: false),
                    EmailOnNewDemand = table.Column<bool>(type: "boolean", nullable: false),
                    EmailOnNewOffer = table.Column<bool>(type: "boolean", nullable: false),
                    EmailOnNewMessage = table.Column<bool>(type: "boolean", nullable: false),
                    EmailMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    EmailWeeklyDigest = table.Column<bool>(type: "boolean", nullable: false),
                    InAppOrders = table.Column<bool>(type: "boolean", nullable: false),
                    InAppMessages = table.Column<bool>(type: "boolean", nullable: false),
                    InAppSystem = table.Column<bool>(type: "boolean", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    DateFormat = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
