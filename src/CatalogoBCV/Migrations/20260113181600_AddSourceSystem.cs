using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoBCV.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceSystemId",
                table: "Tables",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SourceSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceSystems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tables_SourceSystemId",
                table: "Tables",
                column: "SourceSystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_SourceSystems_SourceSystemId",
                table: "Tables",
                column: "SourceSystemId",
                principalTable: "SourceSystems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tables_SourceSystems_SourceSystemId",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "SourceSystems");

            migrationBuilder.DropIndex(
                name: "IX_Tables_SourceSystemId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "SourceSystemId",
                table: "Tables");
        }
    }
}
