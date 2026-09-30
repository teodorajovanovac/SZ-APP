using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SzApp.Data;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <summary>
    /// Partner e-invoicing flags from the legacy Partner table (IsSefUser, IsCrfUser, SkipAutoCheckSef).
    /// </summary>
    [DbContext(typeof(SzAppDbContext))]
    [Migration("20260930090000_AddPartnerSefFlags")]
    public partial class AddPartnerSefFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCrfUser",
                schema: "core",
                table: "Partner",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSefUser",
                schema: "core",
                table: "Partner",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SkipAutoCheckSef",
                schema: "core",
                table: "Partner",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsCrfUser", schema: "core", table: "Partner");
            migrationBuilder.DropColumn(name: "IsSefUser", schema: "core", table: "Partner");
            migrationBuilder.DropColumn(name: "SkipAutoCheckSef", schema: "core", table: "Partner");
        }
    }
}
