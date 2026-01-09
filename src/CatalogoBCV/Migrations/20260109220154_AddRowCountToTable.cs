using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoBCV.Migrations
{
    /// <inheritdoc />
    public partial class AddRowCountToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RowCount",
                table: "Tables",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowCount",
                table: "Tables");
        }
    }
}
