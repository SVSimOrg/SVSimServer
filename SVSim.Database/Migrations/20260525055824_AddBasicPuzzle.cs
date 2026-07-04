using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicPuzzle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuzzleGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PuzzleMasterId = table.Column<int>(type: "integer", nullable: false),
                    BasicTitleTextId = table.Column<string>(type: "text", nullable: false),
                    PuzzleCharaId = table.Column<int>(type: "integer", nullable: false),
                    CharaId = table.Column<int>(type: "integer", nullable: false),
                    SortType = table.Column<int>(type: "integer", nullable: false),
                    DifficultyNameListJson = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuzzleMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    MissionName = table.Column<string>(type: "text", nullable: false),
                    AchievedMessage = table.Column<string>(type: "text", nullable: false),
                    RequireNumber = table.Column<int>(type: "integer", nullable: false),
                    CampaignCommenceTime = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false),
                    TargetPuzzleGroupId = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleMissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerPuzzleClears",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    PuzzleId = table.Column<int>(type: "integer", nullable: false),
                    ClearedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BestRetryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerPuzzleClears", x => new { x.ViewerId, x.PuzzleId });
                });

            migrationBuilder.CreateTable(
                name: "Puzzles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PuzzleId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    PuzzleDifficulty = table.Column<int>(type: "integer", nullable: false),
                    IsAdditional = table.Column<bool>(type: "boolean", nullable: false),
                    IsPlayable = table.Column<bool>(type: "boolean", nullable: false),
                    ReleaseConditionTextId = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puzzles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Puzzles_PuzzleGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "PuzzleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_GroupId",
                table: "Puzzles",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuzzleMissions");

            migrationBuilder.DropTable(
                name: "Puzzles");

            migrationBuilder.DropTable(
                name: "ViewerPuzzleClears");

            migrationBuilder.DropTable(
                name: "PuzzleGroups");
        }
    }
}
