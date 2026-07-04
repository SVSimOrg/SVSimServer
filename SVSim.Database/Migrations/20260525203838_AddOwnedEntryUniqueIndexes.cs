using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnedEntryUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OwnedItemEntry_ViewerId_ItemId",
                table: "OwnedItemEntry",
                columns: new[] { "ViewerId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnedCardEntry_ViewerId_CardId",
                table: "OwnedCardEntry",
                columns: new[] { "ViewerId", "CardId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OwnedItemEntry_ViewerId_ItemId",
                table: "OwnedItemEntry");

            migrationBuilder.DropIndex(
                name: "IX_OwnedCardEntry_ViewerId_CardId",
                table: "OwnedCardEntry");
        }
    }
}
