using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ArenaAndTeamArena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "arenas",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Class = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Mesh = table.Column<uint>(type: "int unsigned", nullable: false),
                    ArenaPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    CurrentHonor = table.Column<uint>(type: "int unsigned", nullable: false),
                    HistoryHonor = table.Column<uint>(type: "int unsigned", nullable: false),
                    TodayBattles = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalWin = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalLose = table.Column<uint>(type: "int unsigned", nullable: false),
                    TodayWin = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonArenaPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonWin = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonLose = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonRank = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arenas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Crimes",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Money = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crimes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "teamarenas",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Class = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Mesh = table.Column<uint>(type: "int unsigned", nullable: false),
                    ArenaPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    CurrentHonor = table.Column<uint>(type: "int unsigned", nullable: false),
                    HistoryHonor = table.Column<uint>(type: "int unsigned", nullable: false),
                    TodayBattles = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalWin = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalLose = table.Column<uint>(type: "int unsigned", nullable: false),
                    TodayWin = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonArenaPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonWin = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonLose = table.Column<uint>(type: "int unsigned", nullable: false),
                    LastSeasonRank = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teamarenas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arenas");

            migrationBuilder.DropTable(
                name: "Crimes");

            migrationBuilder.DropTable(
                name: "teamarenas");
        }
    }
}
