using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class PlayerItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_items",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<uint>(type: "int unsigned", nullable: false),
                    EntityID = table.Column<uint>(type: "int unsigned", nullable: false),
                    ItemId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Durability = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MaxDurability = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Position = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    SocketProgress = table.Column<uint>(type: "int unsigned", nullable: false),
                    SocketOne = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    SocketTwo = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Effect = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Plus = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Bless = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Bound = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Enchant = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Suspicious = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Locked = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PlusProgress = table.Column<uint>(type: "int unsigned", nullable: false),
                    Inscribed = table.Column<uint>(type: "int unsigned", nullable: false),
                    Activate = table.Column<uint>(type: "int unsigned", nullable: false),
                    TimeLeftInMinutes = table.Column<uint>(type: "int unsigned", nullable: false),
                    StackSize = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    WarehouseId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Color = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    IDEvent = table.Column<int>(type: "int", nullable: false),
                    Fake = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UnlockTimer = table.Column<int>(type: "int", nullable: false),
                    ItemPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    RemainingTime = table.Column<uint>(type: "int unsigned", nullable: false),
                    PurificationItemID = table.Column<uint>(type: "int unsigned", nullable: false),
                    PurificationLevel = table.Column<uint>(type: "int unsigned", nullable: false),
                    PurificationDuration = table.Column<uint>(type: "int unsigned", nullable: false),
                    PurificationAddedOn = table.Column<long>(type: "bigint", nullable: false),
                    EffectID = table.Column<uint>(type: "int unsigned", nullable: false),
                    EffectLevel = table.Column<uint>(type: "int unsigned", nullable: false),
                    EffectPercent = table.Column<uint>(type: "int unsigned", nullable: false),
                    EffectPercent2 = table.Column<uint>(type: "int unsigned", nullable: false),
                    EffectDuration = table.Column<uint>(type: "int unsigned", nullable: false),
                    EffectAddedOn = table.Column<long>(type: "bigint", nullable: false),
                    Expiration = table.Column<long>(type: "bigint", nullable: false),
                    DepositeCount = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_items", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_items");
        }
    }
}
