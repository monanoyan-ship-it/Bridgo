using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyFieldsToServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredSurveyDate",
                table: "OrderServiceRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurveyLocation",
                table: "OrderServiceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurveyTypes",
                table: "OrderServiceRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredSurveyDate",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "SurveyLocation",
                table: "OrderServiceRequests");

            migrationBuilder.DropColumn(
                name: "SurveyTypes",
                table: "OrderServiceRequests");
        }
    }
}
