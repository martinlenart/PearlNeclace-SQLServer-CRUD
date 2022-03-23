using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NecklaceDB.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Necklaces",
                columns: table => new
                {
                    NecklaceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Necklaces", x => x.NecklaceID);
                });

            migrationBuilder.CreateTable(
                name: "Pearls",
                columns: table => new
                {
                    PearlID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NecklaceID = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<int>(type: "int", nullable: false),
                    Shape = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pearls", x => x.PearlID);
                    table.ForeignKey(
                        name: "FK_Pearls_Necklaces_NecklaceID",
                        column: x => x.NecklaceID,
                        principalTable: "Necklaces",
                        principalColumn: "NecklaceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pearls_NecklaceID",
                table: "Pearls",
                column: "NecklaceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pearls");

            migrationBuilder.DropTable(
                name: "Necklaces");
        }
    }
}
