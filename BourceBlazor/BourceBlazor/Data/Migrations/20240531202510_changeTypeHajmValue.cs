using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BourceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class changeTypeHajmValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HajmName",
                table: "Hajm");

            migrationBuilder.AddColumn<int>(
                name: "HajmValue",
                table: "Hajm",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HajmValue",
                table: "Hajm");

            migrationBuilder.AddColumn<string>(
                name: "HajmName",
                table: "Hajm",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: false,
                defaultValue: "");
        }
    }
}
