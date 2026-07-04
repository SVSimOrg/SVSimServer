using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoryDecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DeckNo = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    DeckName = table.Column<string>(type: "text", nullable: false),
                    SleeveId = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false),
                    IsRecommend = table.Column<int>(type: "integer", nullable: false),
                    OrderNum = table.Column<int>(type: "integer", nullable: false),
                    EntryNo = table.Column<int>(type: "integer", nullable: false),
                    DeckFormat = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryDecks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryDecks");
        }
    }
}
