using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Guilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildID = table.Column<uint>(type: "int unsigned", nullable: false),
                    SilverFund = table.Column<long>(type: "bigint", nullable: false),
                    ConquerPointFund = table.Column<uint>(type: "int unsigned", nullable: false),
                    MembersCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    MyRank = table.Column<uint>(type: "int unsigned", nullable: false),
                    Level = table.Column<uint>(type: "int unsigned", nullable: false),
                    CreateTime = table.Column<uint>(type: "int unsigned", nullable: false),
                    LeaderName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Recruit = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdvertiseRecruit = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Allies = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enemies = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bulletin = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UseAdvertise = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BuletinEnrole = table.Column<uint>(type: "int unsigned", nullable: false),
                    CTFExploits = table.Column<uint>(type: "int unsigned", nullable: false),
                    CTFNextConquerPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    CTFNextMoney = table.Column<uint>(type: "int unsigned", nullable: false),
                    CTFRank = table.Column<uint>(type: "int unsigned", nullable: false),
                    ClaimCtfReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    Arsenal = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guilds");
        }
    }
}
