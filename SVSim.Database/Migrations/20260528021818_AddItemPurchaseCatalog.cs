using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddItemPurchaseCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemPurchaseCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RequireItemType = table.Column<int>(type: "integer", nullable: false),
                    RequireItemId = table.Column<long>(type: "bigint", nullable: false),
                    RequireItemNum = table.Column<int>(type: "integer", nullable: false),
                    PurchaseItemType = table.Column<int>(type: "integer", nullable: false),
                    PurchaseItemId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseItemNum = table.Column<int>(type: "integer", nullable: false),
                    PurchaseName = table.Column<string>(type: "text", nullable: false),
                    IsMonthlyReset = table.Column<bool>(type: "boolean", nullable: false),
                    PurchaseLimit = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPurchaseCatalog", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemPurchaseCatalog");
        }
    }
}
