using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddColosseumCuratedDecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColosseumAvatarDecks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckNo = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    CardListJson = table.Column<string>(type: "jsonb", nullable: false),
                    SleeveId = table.Column<long>(type: "bigint", nullable: false),
                    LeaderSkinId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColosseumAvatarDecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColosseumHofDecks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckNo = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    CardListJson = table.Column<string>(type: "jsonb", nullable: false),
                    SleeveId = table.Column<long>(type: "bigint", nullable: false),
                    LeaderSkinId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColosseumHofDecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColosseumWindFallDecks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckNo = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    CardListJson = table.Column<string>(type: "jsonb", nullable: false),
                    SleeveId = table.Column<long>(type: "bigint", nullable: false),
                    LeaderSkinId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColosseumWindFallDecks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColosseumAvatarDecks_DeckNo",
                table: "ColosseumAvatarDecks",
                column: "DeckNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColosseumHofDecks_DeckNo",
                table: "ColosseumHofDecks",
                column: "DeckNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColosseumWindFallDecks_DeckNo",
                table: "ColosseumWindFallDecks",
                column: "DeckNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColosseumAvatarDecks");

            migrationBuilder.DropTable(
                name: "ColosseumHofDecks");

            migrationBuilder.DropTable(
                name: "ColosseumWindFallDecks");
        }
    }
}
