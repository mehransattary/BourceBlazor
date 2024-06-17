using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BourceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class changeFormol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CalculationPrice",
                table: "Formols",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MultiStage",
                table: "Formols",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculationPrice",
                table: "Formols");

            migrationBuilder.DropColumn(
                name: "MultiStage",
                table: "Formols");
        }
    }
}
