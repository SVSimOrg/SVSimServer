using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionsAndAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementCatalog",
                columns: table => new
                {
                    AchievementType = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RequireNumber = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false),
                    OrderNum = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: true),
                    EventArg = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementCatalog", x => new { x.AchievementType, x.Level });
                });

            migrationBuilder.CreateTable(
                name: "BattlePassMonthlyMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    OrderNum = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RequireNumber = table.Column<int>(type: "integer", nullable: false),
                    BattlePassPoint = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: true),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: true),
                    RewardNumber = table.Column<int>(type: "integer", nullable: true),
                    EventType = table.Column<string>(type: "text", nullable: true),
                    EventArg = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattlePassMonthlyMissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LotType = table.Column<int>(type: "integer", nullable: false),
                    RequireNumber = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false),
                    BattlePassPoint = table.Column<int>(type: "integer", nullable: false),
                    DefaultFlag = table.Column<bool>(type: "boolean", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: true),
                    EventArg = table.Column<int>(type: "integer", nullable: true),
                    StartTime = table.Column<long>(type: "bigint", nullable: false),
                    EndTime = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerAchievements",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    AchievementType = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    AchievementStatus = table.Column<int>(type: "integer", nullable: false),
                    NowAchievedLevel = table.Column<int>(type: "integer", nullable: false),
                    ResultAnnounceSawLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerAchievements", x => new { x.ViewerId, x.AchievementType });
                    table.ForeignKey(
                        name: "FK_ViewerAchievements_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewerEventCounters",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    EventKey = table.Column<string>(type: "text", nullable: false),
                    Period = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerEventCounters", x => new { x.ViewerId, x.EventKey, x.Period });
                    table.ForeignKey(
                        name: "FK_ViewerEventCounters_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewerMissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    MissionCatalogId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<long>(type: "bigint", nullable: false),
                    ClaimedAt = table.Column<long>(type: "bigint", nullable: true),
                    MissionStatus = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewerMissions_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementCatalog_AchievementType",
                table: "AchievementCatalog",
                column: "AchievementType");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementCatalog_EventType_EventArg",
                table: "AchievementCatalog",
                columns: new[] { "EventType", "EventArg" });

            migrationBuilder.CreateIndex(
                name: "IX_BattlePassMonthlyMissions_Year_Month",
                table: "BattlePassMonthlyMissions",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_BattlePassMonthlyMissions_Year_Month_OrderNum",
                table: "BattlePassMonthlyMissions",
                columns: new[] { "Year", "Month", "OrderNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionCatalog_EventType_EventArg",
                table: "MissionCatalog",
                columns: new[] { "EventType", "EventArg" });

            migrationBuilder.CreateIndex(
                name: "IX_MissionCatalog_LotType",
                table: "MissionCatalog",
                column: "LotType");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerEventCounters_ViewerId_Period",
                table: "ViewerEventCounters",
                columns: new[] { "ViewerId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerMissions_ViewerId",
                table: "ViewerMissions",
                column: "ViewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerMissions_ViewerId_Slot",
                table: "ViewerMissions",
                columns: new[] { "ViewerId", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementCatalog");

            migrationBuilder.DropTable(
                name: "BattlePassMonthlyMissions");

            migrationBuilder.DropTable(
                name: "MissionCatalog");

            migrationBuilder.DropTable(
                name: "ViewerAchievements");

            migrationBuilder.DropTable(
                name: "ViewerEventCounters");

            migrationBuilder.DropTable(
                name: "ViewerMissions");
        }
    }
}
