using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddViewerBattleHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewerBattleHistories",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    BattleId = table.Column<long>(type: "bigint", nullable: false),
                    BattleType = table.Column<int>(type: "integer", nullable: false),
                    DeckFormat = table.Column<int>(type: "integer", nullable: false),
                    TwoPickType = table.Column<int>(type: "integer", nullable: false),
                    IsLimitTurn = table.Column<int>(type: "integer", nullable: false),
                    SelfClassId = table.Column<int>(type: "integer", nullable: false),
                    SelfSubClassId = table.Column<int>(type: "integer", nullable: false),
                    SelfCharaId = table.Column<int>(type: "integer", nullable: false),
                    SelfRotationId = table.Column<string>(type: "text", nullable: false),
                    OpponentClassId = table.Column<int>(type: "integer", nullable: false),
                    OpponentSubClassId = table.Column<int>(type: "integer", nullable: false),
                    OpponentCharaId = table.Column<int>(type: "integer", nullable: false),
                    OpponentName = table.Column<string>(type: "text", nullable: false),
                    OpponentCountryCode = table.Column<string>(type: "text", nullable: false),
                    OpponentEmblemId = table.Column<long>(type: "bigint", nullable: false),
                    OpponentDegreeId = table.Column<long>(type: "bigint", nullable: false),
                    OpponentRotationId = table.Column<string>(type: "text", nullable: false),
                    IsWin = table.Column<bool>(type: "boolean", nullable: false),
                    BattleStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerBattleHistories", x => new { x.ViewerId, x.BattleId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerBattleHistories_ViewerId_CreateTime",
                table: "ViewerBattleHistories",
                columns: new[] { "ViewerId", "CreateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerBattleHistories");
        }
    }
}
