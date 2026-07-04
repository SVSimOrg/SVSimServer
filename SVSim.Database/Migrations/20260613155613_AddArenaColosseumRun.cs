using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddArenaColosseumRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewerArenaColosseumRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    EntryId = table.Column<long>(type: "bigint", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    DeckFormat = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<long>(type: "bigint", nullable: false),
                    ConsumeItemType = table.Column<int>(type: "integer", nullable: false),
                    CandidateClassIdsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SelectTurn = table.Column<int>(type: "integer", nullable: false),
                    IsSelectCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    SelectedCardIdsJson = table.Column<string>(type: "jsonb", nullable: false),
                    PendingPickSetsJson = table.Column<string>(type: "jsonb", nullable: false),
                    NextCandidateId = table.Column<long>(type: "bigint", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    ChaosId = table.Column<int>(type: "integer", nullable: false),
                    ResultListJson = table.Column<string>(type: "jsonb", nullable: false),
                    WinCount = table.Column<int>(type: "integer", nullable: false),
                    LossCount = table.Column<int>(type: "integer", nullable: false),
                    BattleCountThisRound = table.Column<int>(type: "integer", nullable: false),
                    MaxBattleCountThisRound = table.Column<int>(type: "integer", nullable: false),
                    BreakthroughNumberThisRound = table.Column<int>(type: "integer", nullable: false),
                    RestEntryNum = table.Column<int>(type: "integer", nullable: false),
                    IsRankMatching = table.Column<bool>(type: "boolean", nullable: false),
                    IsChampion = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredDeckNoListJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerArenaColosseumRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerArenaColosseumRuns_ViewerId",
                table: "ViewerArenaColosseumRuns",
                column: "ViewerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerArenaColosseumRuns");
        }
    }
}
