using Microsoft.EntityFrameworkCore.Migrations;

namespace BillColl_Main.Migrations
{
    public partial class UpdatePhEntityRHierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "ReportHierarchy",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "ReportHierarchy",
                value: 2);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "ReportHierarchy",
                value: 5);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 4,
                column: "ReportHierarchy",
                value: 3);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 5,
                column: "ReportHierarchy",
                value: 6);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 6,
                column: "ReportHierarchy",
                value: 4);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "ReportHierarchy",
                value: 0);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "ReportHierarchy",
                value: 0);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "ReportHierarchy",
                value: 0);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 4,
                column: "ReportHierarchy",
                value: 0);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 5,
                column: "ReportHierarchy",
                value: 0);

            migrationBuilder.UpdateData(
                table: "PHEntity",
                keyColumn: "Id",
                keyValue: 6,
                column: "ReportHierarchy",
                value: 0);
        }
    }
}
