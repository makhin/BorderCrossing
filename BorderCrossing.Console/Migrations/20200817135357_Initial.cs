using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Console.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Sqlite:InitSpatialMetaData", true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Region = table.Column<short>(nullable: false),
                    Geom = table.Column<Geometry>(type: "geometry", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(nullable: false),
                    IpAddress = table.Column<string>(nullable: true),
                    UserAgent = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "CheckPoints",
                columns: table => new
                {
                    CheckPointId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<Guid>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Point = table.Column<Geometry>(type: "geometry", nullable: true),
                    CountryName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckPoints", x => x.CheckPointId);
                    table.ForeignKey(
                        name: "FK_CheckPoints_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationHistoryFiles",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    File = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHistoryFiles", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_LocationHistoryFiles_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckPoints_RequestId",
                table: "CheckPoints",
                column: "RequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckPoints");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "LocationHistoryFiles");

            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
