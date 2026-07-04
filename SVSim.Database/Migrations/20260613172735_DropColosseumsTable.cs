using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropColosseumsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colosseums");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colosseums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    CardPoolName = table.Column<string>(type: "text", nullable: false),
                    ColosseumId = table.Column<string>(type: "text", nullable: false),
                    ColosseumName = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeckFormat = table.Column<string>(type: "text", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAllCardEnabled = table.Column<int>(type: "integer", nullable: false),
                    IsColosseumPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisplayTips = table.Column<string>(type: "text", nullable: false),
                    IsNormalTwoPick = table.Column<string>(type: "text", nullable: false),
                    IsRoundPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    IsSpecialMode = table.Column<string>(type: "text", nullable: false),
                    NowRound = table.Column<string>(type: "text", nullable: false),
                    SalesPeriodInfo = table.Column<string>(type: "jsonb", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipsId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colosseums", x => x.Id);
                });
        }
    }
}
