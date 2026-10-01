using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class SeedBankAndEntityTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "Id", "AccountCode", "AccountName", "AccountNumber", "BankName", "Branch", "SwiftCode" },
                values: new object[,]
                {
                    { 1, null, "Isla Lipana and Co.", "3691-0000-27 - Current account-Peso    3694-0001-25(Dollar account)", "Bank of the Philippine Islands", "Malate, Metro Manila, Philippines", "BOPIPHMM" },
                    { 2, null, "Isla Lipana and Co.", "001688052218 - Current Account Peso", "BDO Unibank, Inc.", "BDO Philam Tower- Valero Branch", "BNORPHMM" },
                    { 3, null, "PricewaterhouseCoopers Consulting Services Philippines Co.Ltd..", "99196741877 - Current account-Peso     9010196927767 - SA-(Dollar account)", "Standard Chartered Bank", "Ayala, Makati City", "SCBLPHMM" },
                    { 4, null, "Cabrera & Company", "0031-0681-69 - Current account-Peso     0034-0280-95 - (Dollar Account)", "Bank of the Philippine Islands", "Paseo de Roxas, Makati City", "BOPIPHMM" }
                });

            migrationBuilder.InsertData(
                table: "PHEntity",
                columns: new[] { "Id", "EntityCode", "EntityName" },
                values: new object[,]
                {
                    { 1, "", "Isla Lipana & Co." },
                    { 2, "", "Cabrera & Company" },
                    { 3, "", "PwC WMS Pte Ltd." },
                    { 4, "", "PwC CONS Ltd." },
                    { 5, "", "PwC Services" },
                    { 6, "", "PwC BSP Co. Ltd." }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
