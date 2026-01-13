using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoBCV.Migrations
{
    /// <inheritdoc />
    public partial class AddTableMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AffectedReports",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfidentialityLevel",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DataSteward",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DependentDashboards",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffectedReports",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "ConfidentialityLevel",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "DataSteward",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "DependentDashboards",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Tables");
        }
    }
}
