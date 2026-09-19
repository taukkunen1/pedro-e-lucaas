using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class PlayerHouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_houses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Level = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_houses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "player_house_furnitures",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ApiPlayerHouseId = table.Column<uint>(type: "int unsigned", nullable: true),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Data = table.Column<uint>(type: "int unsigned", nullable: false),
                    X = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Y = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Mesh = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    NpcType = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ObjType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Sort = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DynamicID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Map = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_house_furnitures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_house_furnitures_player_houses_ApiPlayerHouseId",
                        column: x => x.ApiPlayerHouseId,
                        principalTable: "player_houses",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_player_house_furnitures_ApiPlayerHouseId",
                table: "player_house_furnitures",
                column: "ApiPlayerHouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_house_furnitures");

            migrationBuilder.DropTable(
                name: "player_houses");
        }
    }
}
