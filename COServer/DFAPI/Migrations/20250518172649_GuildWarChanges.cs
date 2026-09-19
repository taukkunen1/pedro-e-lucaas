using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class GuildWarChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GuildConductor4",
                table: "GuildWars",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "GuildConductor3",
                table: "GuildWars",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "GuildConductor2",
                table: "GuildWars",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "GuildConductor1",
                table: "GuildWars",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "GuildConductor4",
                table: "GuildWars",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<uint>(
                name: "GuildConductor3",
                table: "GuildWars",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<uint>(
                name: "GuildConductor2",
                table: "GuildWars",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<uint>(
                name: "GuildConductor1",
                table: "GuildWars",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
