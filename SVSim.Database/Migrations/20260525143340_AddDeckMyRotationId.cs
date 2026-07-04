using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckMyRotationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyRotationId",
                table: "Decks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyRotationId",
                table: "Decks");
        }
    }
}
