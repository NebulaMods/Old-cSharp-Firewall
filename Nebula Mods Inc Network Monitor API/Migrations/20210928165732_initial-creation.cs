using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NebulaMods.Migrations
{
    public partial class initialcreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttackLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Server = table.Column<string>(type: "TEXT", nullable: true),
                    IP = table.Column<string>(type: "TEXT", nullable: true),
                    InitialPPS = table.Column<long>(type: "INTEGER", nullable: false),
                    InitialBPS = table.Column<long>(type: "INTEGER", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", nullable: true),
                    PeakPPS = table.Column<long>(type: "INTEGER", nullable: false),
                    PeakBPS = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalPackets = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalUniqueIPs = table.Column<long>(type: "INTEGER", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndingTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PcapFile = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackLogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Errors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Application = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IPs",
                columns: table => new
                {
                    IP = table.Column<string>(type: "TEXT", nullable: false),
                    Geolocation = table.Column<string>(type: "TEXT", nullable: true),
                    Server = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    EdgeRules = table.Column<string>(type: "TEXT", nullable: true),
                    FlagLink = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPs", x => x.IP);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttackLogs");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "IPs");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
