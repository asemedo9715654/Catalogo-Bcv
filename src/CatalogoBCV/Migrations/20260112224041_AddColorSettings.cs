using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CatalogoBCV.Migrations
{
    /// <inheritdoc />
    public partial class AddColorSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "Description", "Group", "Type", "Value" },
                values: new object[,]
                {
                    { "HeaderColor", "Header Background Color", "Appearance", "color", "#ffffff" },
                    { "PrimaryColor", "Primary Color", "Appearance", "color", "#0d6efd" },
                    { "SidebarColor", "Sidebar Background Color", "Appearance", "color", "#f8f9fa" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "HeaderColor");

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "PrimaryColor");

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "SidebarColor");
        }
    }
}
