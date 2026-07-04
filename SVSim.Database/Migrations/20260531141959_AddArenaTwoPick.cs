using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddArenaTwoPick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArenaTwoPickRewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WinCount = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaTwoPickRewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerArenaTwoPickRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    EntryId = table.Column<long>(type: "bigint", nullable: false),
                    RewardScheduleId = table.Column<int>(type: "integer", nullable: false),
                    ChallengeId = table.Column<int>(type: "integer", nullable: false),
                    MaxBattleCount = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateClassIdsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SelectTurn = table.Column<int>(type: "integer", nullable: false),
                    IsSelectCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    SelectedCardIdsJson = table.Column<string>(type: "jsonb", nullable: false),
                    PendingPickSetsJson = table.Column<string>(type: "jsonb", nullable: false),
                    NextCandidateId = table.Column<long>(type: "bigint", nullable: false),
                    ResultListJson = table.Column<string>(type: "jsonb", nullable: false),
                    WinCount = table.Column<int>(type: "integer", nullable: false),
                    LossCount = table.Column<int>(type: "integer", nullable: false),
                    IsRetire = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerArenaTwoPickRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArenaTwoPickRewards_WinCount",
                table: "ArenaTwoPickRewards",
                column: "WinCount");

            migrationBuilder.CreateIndex(
                name: "IX_ArenaTwoPickRewards_WinCount_RewardType_RewardId",
                table: "ArenaTwoPickRewards",
                columns: new[] { "WinCount", "RewardType", "RewardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewerArenaTwoPickRuns_ViewerId",
                table: "ViewerArenaTwoPickRuns",
                column: "ViewerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArenaTwoPickRewards");

            migrationBuilder.DropTable(
                name: "ViewerArenaTwoPickRuns");
        }
    }
}
