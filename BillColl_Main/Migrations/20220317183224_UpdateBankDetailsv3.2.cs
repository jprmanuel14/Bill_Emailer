using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class UpdateBankDetailsv32 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: "3691-0000-27 - Current account-Peso <br><br> 3694-0001-25(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3,
                column: "AccountNumber",
                value: "99196741877 - Current account-Peso <br><br> 9010196927767 - SA-(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 4,
                column: "AccountNumber",
                value: "0031-0681-69 - Current account-Peso <br><br> 0034-0280-95 - (Dollar Account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 8,
                column: "AccountNumber",
                value: "PHP (C/A)   0031-0744-01 <br><br> USD (S/A)	0034-0547-62");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 9,
                column: "AccountNumber",
                value: "8074000968-Current account - Peso <br><br> 8071000371 (Dollar account)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: "3691-0000-27 - Current account-Peso <br> 3694-0001-25(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3,
                column: "AccountNumber",
                value: "99196741877 - Current account-Peso \n 9010196927767 - SA-(Dollar account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 4,
                column: "AccountNumber",
                value: "0031-0681-69 - Current account-Peso \n 0034-0280-95 - (Dollar Account)");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 8,
                column: "AccountNumber",
                value: "PHP (C/A)   0031-0744-01 \n USD (S/A)	0034-0547-62");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 9,
                column: "AccountNumber",
                value: "8074000968-Current account - Peso \n 8071000371 (Dollar account)");
        }
    }
}
