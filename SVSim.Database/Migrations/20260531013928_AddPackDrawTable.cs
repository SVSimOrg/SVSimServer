using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPackDrawTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Packs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PackDrawCardWeights",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    RatePct = table.Column<double>(type: "double precision", nullable: true),
                    IsLeader = table.Column<bool>(type: "boolean", nullable: false),
                    IsAltArt = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackDrawCardWeights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackDrawConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    AnimationRatePct = table.Column<double>(type: "double precision", nullable: false),
                    HasBonusSlot = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialKind = table.Column<string>(type: "text", nullable: true),
                    ShortCode = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackDrawConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackDrawSlotRates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    RatePct = table.Column<double>(type: "double precision", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackDrawSlotRates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackDrawCardWeights_PackId_Slot_Tier",
                table: "PackDrawCardWeights",
                columns: new[] { "PackId", "Slot", "Tier" });

            migrationBuilder.CreateIndex(
                name: "IX_PackDrawSlotRates_PackId_Slot_Tier",
                table: "PackDrawSlotRates",
                columns: new[] { "PackId", "Slot", "Tier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackDrawCardWeights");

            migrationBuilder.DropTable(
                name: "PackDrawConfigs");

            migrationBuilder.DropTable(
                name: "PackDrawSlotRates");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Packs");
        }
    }
}
