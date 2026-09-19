using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FurnituresAndNPCs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Furnitures",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemID = table.Column<uint>(type: "int unsigned", nullable: false),
                    MoneyCost = table.Column<uint>(type: "int unsigned", nullable: false),
                    Mesh = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Map = table.Column<uint>(type: "int unsigned", nullable: false),
                    X = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Y = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Type = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Furnitures", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NPCs",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HitPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    MaxHitPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    Map = table.Column<uint>(type: "int unsigned", nullable: false),
                    X = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Y = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ObjType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Mesh = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Sort = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ShowName = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NPCs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Furnitures");

            migrationBuilder.DropTable(
                name: "NPCs");
        }
    }
}
