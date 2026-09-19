using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ClanWars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clan_wars",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ClanId = table.Column<uint>(type: "int unsigned", nullable: false),
                    PoleName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WinnerClaimReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    WinnerNextReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    WinnerOccupationDays = table.Column<uint>(type: "int unsigned", nullable: false),
                    WinnerReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    BestWinnerClanId = table.Column<uint>(type: "int unsigned", nullable: false),
                    BestWinnerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BestWinnerClaimReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    BestWinnerNextReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    BestWinnerOccupationDays = table.Column<uint>(type: "int unsigned", nullable: false),
                    BestWinnerReward = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clan_wars", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clan_wars");
        }
    }
}
