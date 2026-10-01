using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class SeedUserTablev2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "GUID" },
                values: new object[,]
                {
                    { 2, "renie.rose.ann.manaois@pwc.com", "" },
                    { 3, "enjelo.marrius.balane@pwc.com", "" },
                    { 4, "john.patrick.manuel@pwc.com", "" },
                    { 5, "jherremy.ivan.roldan@pwc.com", "" },
                    { 6, "edna.c.brimon@pwc.com", "" },
                    { 7, "genpros.sanidad@pwc.com", "" },
                    { 8, "monalisa.m.mamasao@pwc.com", "" },
                    { 9, "carla.v.tanieca@pwc.com", "" },
                    { 10, "kyle.christopher.quiogue@pwc.com", "" },
                    { 11, "christine.maneja@pwc.com", "" },
                    { 12, "alvin.p.mendoza@pwc.com", "" },
                    { 13, "carol.a.barandon@pwc.com", "" },
                    { 14, "sandro.mark.laguerta@pwc.com", "" },
                    { 15, "kenneth.aquino@pwc.com", "" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}
