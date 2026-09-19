using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class StaticStatues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorBattleLimitTypes",
                table: "TutorBattleLimitTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Traps",
                table: "Traps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transformations",
                table: "Transformations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SobNPCs",
                table: "SobNPCs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Portals",
                table: "Portals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NPCs",
                table: "NPCs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameMaps",
                table: "GameMaps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Furnitures",
                table: "Furnitures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Crimes",
                table: "Crimes");

            migrationBuilder.RenameTable(
                name: "TutorBattleLimitTypes",
                newName: "tutorbattlelimittypes");

            migrationBuilder.RenameTable(
                name: "Traps",
                newName: "traps");

            migrationBuilder.RenameTable(
                name: "Transformations",
                newName: "transformations");

            migrationBuilder.RenameTable(
                name: "SobNPCs",
                newName: "sobnpcs");

            migrationBuilder.RenameTable(
                name: "Portals",
                newName: "portals");

            migrationBuilder.RenameTable(
                name: "NPCs",
                newName: "npcs");

            migrationBuilder.RenameTable(
                name: "GameMaps",
                newName: "gamemaps");

            migrationBuilder.RenameTable(
                name: "Furnitures",
                newName: "furnitures");

            migrationBuilder.RenameTable(
                name: "Crimes",
                newName: "crimes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tutorbattlelimittypes",
                table: "tutorbattlelimittypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_traps",
                table: "traps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transformations",
                table: "transformations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sobnpcs",
                table: "sobnpcs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portals",
                table: "portals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_npcs",
                table: "npcs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gamemaps",
                table: "gamemaps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_furnitures",
                table: "furnitures",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_crimes",
                table: "crimes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "static_statues",
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
                    ShowName = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatueSize = table.Column<uint>(type: "int unsigned", nullable: false),
                    StatuePackets = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_static_statues", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "static_statues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tutorbattlelimittypes",
                table: "tutorbattlelimittypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_traps",
                table: "traps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transformations",
                table: "transformations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sobnpcs",
                table: "sobnpcs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portals",
                table: "portals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_npcs",
                table: "npcs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gamemaps",
                table: "gamemaps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_furnitures",
                table: "furnitures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_crimes",
                table: "crimes");

            migrationBuilder.RenameTable(
                name: "tutorbattlelimittypes",
                newName: "TutorBattleLimitTypes");

            migrationBuilder.RenameTable(
                name: "traps",
                newName: "Traps");

            migrationBuilder.RenameTable(
                name: "transformations",
                newName: "Transformations");

            migrationBuilder.RenameTable(
                name: "sobnpcs",
                newName: "SobNPCs");

            migrationBuilder.RenameTable(
                name: "portals",
                newName: "Portals");

            migrationBuilder.RenameTable(
                name: "npcs",
                newName: "NPCs");

            migrationBuilder.RenameTable(
                name: "gamemaps",
                newName: "GameMaps");

            migrationBuilder.RenameTable(
                name: "furnitures",
                newName: "Furnitures");

            migrationBuilder.RenameTable(
                name: "crimes",
                newName: "Crimes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorBattleLimitTypes",
                table: "TutorBattleLimitTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Traps",
                table: "Traps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transformations",
                table: "Transformations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SobNPCs",
                table: "SobNPCs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Portals",
                table: "Portals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NPCs",
                table: "NPCs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameMaps",
                table: "GameMaps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Furnitures",
                table: "Furnitures",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Crimes",
                table: "Crimes",
                column: "Id");
        }
    }
}
