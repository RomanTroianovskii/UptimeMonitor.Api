using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MonitorTargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Ok = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    LatencyMs = table.Column<long>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Targets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: false),
                    IntervalSec = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeoutSec = table.Column<int>(type: "INTEGER", nullable: false),
                    FailThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TelegramChatId = table.Column<long>(type: "INTEGER", nullable: true),
                    LastCheckedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastOk = table.Column<bool>(type: "INTEGER", nullable: true),
                    ConsecutiveFails = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Targets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_MonitorTargetId_CheckedAt",
                table: "Logs",
                columns: new[] { "MonitorTargetId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Targets_Url",
                table: "Targets",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Targets");
        }
    }
}
