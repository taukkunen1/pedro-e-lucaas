using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Portals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Portals",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapID = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    X = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Y = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DestinationMapID = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DestinationX = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DestinationY = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portals", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Portals");
        }
    }
}
