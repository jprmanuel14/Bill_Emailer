using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class UpdateBankdetailsv3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1,
                column: "AccountNumber",
                value: "3691-0000-27 - Current account-Peso \n 3694-0001-25(Dollar account)");

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

            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "Id", "AccountCode", "AccountName", "AccountNumber", "BankName", "Branch", "SwiftCode" },
                values: new object[,]
                {
                    { 8, "040", "PRICEWATERHOUSECOOPERS WMS PTE. LTD.", "PHP (C/A)   0031-0744-01 \n USD (S/A)	0034-0547-62", "BANK OF THE PHILIPPINE ISLANDS", "AYALA-PASEO", "BOPIPHMM" },
                    { 9, "090", "PricewaterhouseCoopers Business Services Philippines Co., Ltd.  ", "8074000968-Current account - Peso \n 8071000371 (Dollar account)", "Bank of the Philippine Islands", "BPI St. Francis Square", "BOPIPHMM" },
                    { 10, "060", "PRICEWATERHOUSECOOPERS SERVICES", "00-001-01-0048872", "Bank Islam Brunei Darussalam", "", "BIBDBNBB" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 10);

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
    }
}
