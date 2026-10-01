using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class Seedbankentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BankPHEntity",
                columns: new[] { "BankId", "PHEntityId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 4 },
                    { 4, 2 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BankPHEntity",
                keyColumns: new[] { "BankId", "PHEntityId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "BankPHEntity",
                keyColumns: new[] { "BankId", "PHEntityId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "BankPHEntity",
                keyColumns: new[] { "BankId", "PHEntityId" },
                keyValues: new object[] { 3, 4 });

            migrationBuilder.DeleteData(
                table: "BankPHEntity",
                keyColumns: new[] { "BankId", "PHEntityId" },
                keyValues: new object[] { 4, 2 });
        }
    }
}
