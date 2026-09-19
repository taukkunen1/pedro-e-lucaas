using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class GuildWar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildWars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    WinnerGuildID = table.Column<uint>(type: "int unsigned", nullable: false),
                    WinnerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeaderReward = table.Column<int>(type: "int", nullable: false),
                    DeputiLeaderReward = table.Column<int>(type: "int", nullable: false),
                    PoleHitPoints = table.Column<int>(type: "int", nullable: false),
                    GuildConductor1 = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildConductor2 = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildConductor3 = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildConductor4 = table.Column<uint>(type: "int unsigned", nullable: false),
                    RewardLeadersJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RewardDeputiesJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildWars", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildWars");
        }
    }
}
