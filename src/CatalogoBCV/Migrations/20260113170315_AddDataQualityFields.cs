using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoBCV.Migrations
{
    /// <inheritdoc />
    public partial class AddDataQualityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSuccessfulLoad",
                table: "Tables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadFrequency",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationRules",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NullPercentage",
                table: "Columns",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSuccessfulLoad",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "LoadFrequency",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "ValidationRules",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "NullPercentage",
                table: "Columns");
        }
    }
}
