using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class UpdateBankDetailsv31 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: "3691-0000-27 - Current account-Peso <br> 3694-0001-25(Dollar account)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: "3691-0000-27 - Current account-Peso \n 3694-0001-25(Dollar account)");
        }
    }
}
