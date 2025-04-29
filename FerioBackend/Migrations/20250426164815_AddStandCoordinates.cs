using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerioBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddStandCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Stands",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PosX",
                table: "Stands",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PosY",
                table: "Stands",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Stands",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "Stands");

            migrationBuilder.DropColumn(
                name: "PosX",
                table: "Stands");

            migrationBuilder.DropColumn(
                name: "PosY",
                table: "Stands");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Stands");
        }
    }
}
