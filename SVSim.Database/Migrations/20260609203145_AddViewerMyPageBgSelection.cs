using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddViewerMyPageBgSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MyPageBgId",
                table: "Viewers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MyPageBgSelectType",
                table: "Viewers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ViewerMyPageBgRotation",
                columns: table => new
                {
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    BgId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerMyPageBgRotation", x => new { x.ViewerId, x.Slot });
                    table.ForeignKey(
                        name: "FK_ViewerMyPageBgRotation_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerMyPageBgRotation");

            migrationBuilder.DropColumn(
                name: "MyPageBgId",
                table: "Viewers");

            migrationBuilder.DropColumn(
                name: "MyPageBgSelectType",
                table: "Viewers");
        }
    }
}
