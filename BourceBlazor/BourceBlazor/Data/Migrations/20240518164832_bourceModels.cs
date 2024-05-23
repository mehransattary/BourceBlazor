using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BourceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class bourceModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nomads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nomads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NomadDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomadId = table.Column<int>(type: "int", nullable: false),
                    NomadId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NomadDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NomadDates_Nomads_NomadId1",
                        column: x => x.NomadId1,
                        principalTable: "Nomads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NomadActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NTran = table.Column<long>(type: "bigint", nullable: false),
                    HEven = table.Column<TimeOnly>(type: "time", nullable: false),
                    QTitTran = table.Column<long>(type: "bigint", nullable: false),
                    PTran = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDisable = table.Column<bool>(type: "bit", nullable: false),
                    NomadDateId = table.Column<int>(type: "int", nullable: false),
                    NomadDateId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NomadActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NomadActions_NomadDates_NomadDateId1",
                        column: x => x.NomadDateId1,
                        principalTable: "NomadDates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NomadActions_NomadDateId1",
                table: "NomadActions",
                column: "NomadDateId1");

            migrationBuilder.CreateIndex(
                name: "IX_NomadDates_NomadId1",
                table: "NomadDates",
                column: "NomadId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NomadActions");

            migrationBuilder.DropTable(
                name: "NomadDates");

            migrationBuilder.DropTable(
                name: "Nomads");
        }
    }
}
