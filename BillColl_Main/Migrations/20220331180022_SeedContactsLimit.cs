using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class SeedContactsLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContactsLimit",
                columns: new[] { "Id", "Limit" },
                values: new object[] { 1, 5 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContactsLimit",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
