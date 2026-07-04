using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddArenaTwoPickRewardGroupAndWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArenaTwoPickRewards_WinCount_RewardType_RewardId",
                table: "ArenaTwoPickRewards");

            migrationBuilder.AddColumn<int>(
                name: "RewardGroup",
                table: "ArenaTwoPickRewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "ArenaTwoPickRewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ArenaTwoPickRewards_WinCount_RewardGroup_RewardType_RewardI~",
                table: "ArenaTwoPickRewards",
                columns: new[] { "WinCount", "RewardGroup", "RewardType", "RewardId", "RewardNum" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArenaTwoPickRewards_WinCount_RewardGroup_RewardType_RewardI~",
                table: "ArenaTwoPickRewards");

            migrationBuilder.DropColumn(
                name: "RewardGroup",
                table: "ArenaTwoPickRewards");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "ArenaTwoPickRewards");

            migrationBuilder.CreateIndex(
                name: "IX_ArenaTwoPickRewards_WinCount_RewardType_RewardId",
                table: "ArenaTwoPickRewards",
                columns: new[] { "WinCount", "RewardType", "RewardId" },
                unique: true);
        }
    }
}
