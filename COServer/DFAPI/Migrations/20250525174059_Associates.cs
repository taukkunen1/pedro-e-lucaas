using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Associates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "associates",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    MentorExpballs = table.Column<uint>(type: "int unsigned", nullable: false),
                    MentorBlessing = table.Column<uint>(type: "int unsigned", nullable: false),
                    MentorStones = table.Column<uint>(type: "int unsigned", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_associates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BanIPs",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IP = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hours = table.Column<uint>(type: "int unsigned", nullable: false),
                    StartBan = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanIPs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "associate_members",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Timer = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ExpBalls = table.Column<uint>(type: "int unsigned", nullable: false),
                    Stone = table.Column<uint>(type: "int unsigned", nullable: false),
                    Blessing = table.Column<uint>(type: "int unsigned", nullable: false),
                    MapName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KillsCount = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    BattlePower = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ApiAssociateId = table.Column<uint>(type: "int unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_associate_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_associate_members_associates_ApiAssociateId",
                        column: x => x.ApiAssociateId,
                        principalTable: "associates",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_associate_members_ApiAssociateId",
                table: "associate_members",
                column: "ApiAssociateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "associate_members");

            migrationBuilder.DropTable(
                name: "BanIPs");

            migrationBuilder.DropTable(
                name: "associates");
        }
    }
}
