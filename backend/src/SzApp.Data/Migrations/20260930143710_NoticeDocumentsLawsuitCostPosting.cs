using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class NoticeDocumentsLawsuitCostPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Closing",
                schema: "billing",
                table: "NoticeTemplate",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DecisionDate",
                schema: "billing",
                table: "NoticeTemplate",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                schema: "billing",
                table: "NoticeTemplate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                schema: "billing",
                table: "NoticeTemplate",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CostsJournalEntryId",
                schema: "billing",
                table: "NoticeBatch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForLawsuit",
                schema: "billing",
                table: "Notice",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LawyerCost",
                schema: "billing",
                table: "Notice",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Closing",
                schema: "billing",
                table: "NoticeTemplate");

            migrationBuilder.DropColumn(
                name: "DecisionDate",
                schema: "billing",
                table: "NoticeTemplate");

            migrationBuilder.DropColumn(
                name: "Signature",
                schema: "billing",
                table: "NoticeTemplate");

            migrationBuilder.DropColumn(
                name: "Subject",
                schema: "billing",
                table: "NoticeTemplate");

            migrationBuilder.DropColumn(
                name: "CostsJournalEntryId",
                schema: "billing",
                table: "NoticeBatch");

            migrationBuilder.DropColumn(
                name: "IsForLawsuit",
                schema: "billing",
                table: "Notice");

            migrationBuilder.DropColumn(
                name: "LawyerCost",
                schema: "billing",
                table: "Notice");
        }
    }
}
