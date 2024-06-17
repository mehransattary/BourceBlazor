using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BourceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class changeFieldHajmFormolToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "HajmFormol",
                table: "Formols",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HajmFormol",
                table: "Formols",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
