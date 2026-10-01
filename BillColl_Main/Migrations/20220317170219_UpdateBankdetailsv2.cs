using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class UpdateBankdetailsv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: " 3691-0000-27 - Current account-Peso \n 3694-0001-25(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3,
                column: "AccountNumber",
                value: " 99196741877 - Current account-Peso \n 9010196927767 - SA-(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 4,
                column: "AccountNumber",
                value: " 0031-0681-69 - Current account-Peso \n 0034-0280-95 - (Dollar Account)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: "3691-0000-27 - Current account-Peso    3694-0001-25(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3,
                column: "AccountNumber",
                value: "99196741877 - Current account-Peso     9010196927767 - SA-(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 4,
                column: "AccountNumber",
                value: "0031-0681-69 - Current account-Peso     0034-0280-95 - (Dollar Account)");
        }
    }
}
