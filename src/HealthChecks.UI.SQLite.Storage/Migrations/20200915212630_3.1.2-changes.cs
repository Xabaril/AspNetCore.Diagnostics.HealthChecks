using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthChecks.UI.SQLite.Storage.Migrations
{
    public partial class _312changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
