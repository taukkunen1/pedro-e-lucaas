using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AssociateChangesStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "associates");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "associate_members",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "associate_members");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "associates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
