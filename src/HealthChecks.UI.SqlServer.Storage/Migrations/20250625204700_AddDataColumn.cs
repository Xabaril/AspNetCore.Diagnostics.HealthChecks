using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthChecks.UI.SqlServer.Storage.Migrations;

public partial class AddDataColumnToHealthCheckExecutionEntry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "HealthCheckExecutionEntries",
                type: "nvarchar(max)",
                nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
                name: "Data",
                table: "HealthCheckExecutionEntries");
    }
}
