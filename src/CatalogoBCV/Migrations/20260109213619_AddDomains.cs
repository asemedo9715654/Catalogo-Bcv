using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoBCV.Migrations
{
    /// <inheritdoc />
    public partial class AddDomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DomainId",
                table: "Tables",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tables_DomainId",
                table: "Tables",
                column: "DomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Domains_DomainId",
                table: "Tables",
                column: "DomainId",
                principalTable: "Domains",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Domains_DomainId",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropIndex(
                name: "IX_Tables_DomainId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "Tables");
        }
    }
}
