using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class GameMapsChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "TypeStatus",
                table: "GameMaps",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "TypeStatus",
                table: "GameMaps",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");
        }
    }
}
