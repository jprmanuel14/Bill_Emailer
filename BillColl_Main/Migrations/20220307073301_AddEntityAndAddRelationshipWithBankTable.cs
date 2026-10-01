using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class AddEntityAndAddRelationshipWithBankTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PHEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankPHEntity",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "int", nullable: false),
                    PHEntityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankPHEntity", x => new { x.BankId, x.PHEntityId });
                    table.ForeignKey(
                        name: "FK_BankPHEntity_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankPHEntity_PHEntity_PHEntityId",
                        column: x => x.PHEntityId,
                        principalTable: "PHEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankPHEntity_PHEntityId",
                table: "BankPHEntity",
                column: "PHEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankPHEntity");

            migrationBuilder.DropTable(
                name: "PHEntity");
        }
    }
}
