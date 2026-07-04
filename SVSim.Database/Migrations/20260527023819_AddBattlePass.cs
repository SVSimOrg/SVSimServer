using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBattlePass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BattlePassSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaxLevel = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CanPurchase = table.Column<bool>(type: "boolean", nullable: false),
                    PriceCrystal = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattlePassSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerBattlePassClaims",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    Track = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerBattlePassClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerBattlePassProgress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    CurrentPoint = table.Column<int>(type: "integer", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false),
                    WeeklyPoints = table.Column<int>(type: "integer", nullable: false),
                    WeeklyPeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerBattlePassProgress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BattlePassRewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    Track = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false),
                    IsAppealExclusion = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattlePassRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattlePassRewards_BattlePassSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "BattlePassSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattlePassRewards_SeasonId_Track_Level",
                table: "BattlePassRewards",
                columns: new[] { "SeasonId", "Track", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BattlePassSeasons_StartDate_EndDate",
                table: "BattlePassSeasons",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerBattlePassClaims_ViewerId_SeasonId",
                table: "ViewerBattlePassClaims",
                columns: new[] { "ViewerId", "SeasonId" });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerBattlePassClaims_ViewerId_SeasonId_Track_Level",
                table: "ViewerBattlePassClaims",
                columns: new[] { "ViewerId", "SeasonId", "Track", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewerBattlePassProgress_ViewerId_SeasonId",
                table: "ViewerBattlePassProgress",
                columns: new[] { "ViewerId", "SeasonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattlePassRewards");

            migrationBuilder.DropTable(
                name: "ViewerBattlePassClaims");

            migrationBuilder.DropTable(
                name: "ViewerBattlePassProgress");

            migrationBuilder.DropTable(
                name: "BattlePassSeasons");
        }
    }
}
