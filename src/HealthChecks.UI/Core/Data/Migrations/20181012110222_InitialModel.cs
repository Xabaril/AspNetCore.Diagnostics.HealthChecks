using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthChecks.UI.Core.Data.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(maxLength: 500, nullable: false),
                    Name = table.Column<string>(maxLength: 500, nullable: false),
                    DiscoveryService = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Executions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(nullable: false),
                    OnStateFrom = table.Column<DateTime>(nullable: false),
                    LastExecuted = table.Column<DateTime>(nullable: false),
                    Uri = table.Column<string>(maxLength: 500, nullable: false),
                    Name = table.Column<string>(maxLength: 500, nullable: false),
                    DiscoveryService = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Failures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HealthCheckName = table.Column<string>(maxLength: 500, nullable: false),
                    LastNotified = table.Column<DateTime>(nullable: false),
                    IsUpAndRunning = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Failures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckExecutionEntry",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 500, nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    HealthCheckExecutionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckExecutionEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCheckExecutionEntry_Executions_HealthCheckExecutionId",
                        column: x => x.HealthCheckExecutionId,
                        principalTable: "Executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckExecutionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(maxLength: 50, nullable: false),
                    On = table.Column<DateTime>(nullable: false),
                    HealthCheckExecutionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckExecutionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCheckExecutionHistory_Executions_HealthCheckExecutionId",
                        column: x => x.HealthCheckExecutionId,
                        principalTable: "Executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckExecutionEntry_HealthCheckExecutionId",
                table: "HealthCheckExecutionEntry",
                column: "HealthCheckExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckExecutionHistory_HealthCheckExecutionId",
                table: "HealthCheckExecutionHistory",
                column: "HealthCheckExecutionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Failures");

            migrationBuilder.DropTable(
                name: "HealthCheckExecutionEntry");

            migrationBuilder.DropTable(
                name: "HealthCheckExecutionHistory");

            migrationBuilder.DropTable(
                name: "Executions");
        }
    }
}
