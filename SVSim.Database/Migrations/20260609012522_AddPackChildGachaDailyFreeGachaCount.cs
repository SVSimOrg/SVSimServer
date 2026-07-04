using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPackChildGachaDailyFreeGachaCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyFreeGachaCount",
                table: "PackChildGachaEntry",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyFreeGachaCount",
                table: "PackChildGachaEntry");
        }
    }
}
