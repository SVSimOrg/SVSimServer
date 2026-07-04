using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBattlePassLevels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RewardData", table: "BattlePassLevels");
            migrationBuilder.AddColumn<int>(
                name: "RequiredPoint", table: "BattlePassLevels",
                type: "integer", nullable: false, defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RequiredPoint", table: "BattlePassLevels");
            migrationBuilder.AddColumn<string>(
                name: "RewardData", table: "BattlePassLevels",
                type: "jsonb", nullable: false, defaultValue: "{}");
        }
    }
}
