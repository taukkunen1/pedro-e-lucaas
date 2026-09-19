using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class GameMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameMaps",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MapDoc = table.Column<uint>(type: "int unsigned", nullable: false),
                    TypeStatus = table.Column<uint>(type: "int unsigned", nullable: false),
                    RebornMap = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    RebornX = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    RebornY = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    RecordSteedRace = table.Column<uint>(type: "int unsigned", nullable: false),
                    MapColor = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMaps", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameMaps");
        }
    }
}
