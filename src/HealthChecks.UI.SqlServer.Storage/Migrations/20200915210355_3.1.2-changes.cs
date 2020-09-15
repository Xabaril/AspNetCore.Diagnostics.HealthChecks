using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthChecks.UI.SqlServer.Storage.Migrations
{
    public partial class _312changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HealthCheckExecutionHistories",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "HealthCheckExecutionEntries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "HealthCheckExecutionEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HealthCheckExecutionHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
