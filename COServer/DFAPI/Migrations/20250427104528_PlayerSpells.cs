using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class PlayerSpells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerSpells",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TypeID = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Level = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    PreviousLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SoulLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UseJiangSpell = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PlayerUid = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSpells", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerSpells");
        }
    }
}
