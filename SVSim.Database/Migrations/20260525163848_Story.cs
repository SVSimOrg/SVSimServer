using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class Story : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialBattleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PlayerFirstTurn = table.Column<int>(type: "integer", nullable: false),
                    PlayerStartPp = table.Column<int>(type: "integer", nullable: false),
                    EnemyStartPp = table.Column<int>(type: "integer", nullable: false),
                    PlayerStartLife = table.Column<int>(type: "integer", nullable: false),
                    EnemyStartLife = table.Column<int>(type: "integer", nullable: false),
                    PlayerAttachSkill = table.Column<string>(type: "text", nullable: false),
                    EnemyAttachSkill = table.Column<string>(type: "text", nullable: false),
                    IdOverrideInBattleLog = table.Column<string>(type: "text", nullable: false),
                    BanishEffectOverride = table.Column<string>(type: "text", nullable: false),
                    TokenDrawEffectOverride = table.Column<string>(type: "text", nullable: false),
                    SpecialTokenDrawEffectOverride = table.Column<string>(type: "text", nullable: false),
                    ResultSkip = table.Column<int>(type: "integer", nullable: false),
                    VsEffectOverride = table.Column<int>(type: "integer", nullable: false),
                    ClassDestroyEffectOverride = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialBattleSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoryWorlds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    TitleTextKey = table.Column<string>(type: "text", nullable: false),
                    PanelImageName = table.Column<string>(type: "text", nullable: false),
                    RibbonText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryWorlds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerStoryBranchUnlocks",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerStoryBranchUnlocks", x => new { x.ViewerId, x.StoryId });
                });

            migrationBuilder.CreateTable(
                name: "ViewerStoryProgress",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    IsFinish = table.Column<bool>(type: "boolean", nullable: false),
                    IsSkipped = table.Column<bool>(type: "boolean", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SkippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerStoryProgress", x => new { x.ViewerId, x.StoryId });
                });

            migrationBuilder.CreateTable(
                name: "StorySections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    WorldId = table.Column<int>(type: "integer", nullable: true),
                    StoryApiType = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    AllStoryOrderId = table.Column<int>(type: "integer", nullable: false),
                    NameTextKey = table.Column<string>(type: "text", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    IsLeaderSelect = table.Column<bool>(type: "boolean", nullable: false),
                    BackGroundId = table.Column<int>(type: "integer", nullable: false),
                    ChapterSelectType = table.Column<int>(type: "integer", nullable: false),
                    StoryTypeOverwrite = table.Column<int>(type: "integer", nullable: false),
                    IsUnderMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    IsPlayAnotherEndAppearanceAnimation = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorySections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorySections_StoryWorlds_WorldId",
                        column: x => x.WorldId,
                        principalTable: "StoryWorlds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StoryChapters",
                columns: table => new
                {
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    CharaId = table.Column<int>(type: "integer", nullable: false),
                    ChapterId = table.Column<string>(type: "text", nullable: false),
                    NextChapterId = table.Column<string>(type: "text", nullable: false),
                    RequiredChapterId = table.Column<string>(type: "text", nullable: true),
                    SelectionDisplayPosition = table.Column<string>(type: "text", nullable: true),
                    SelectionTextId = table.Column<string>(type: "text", nullable: true),
                    XCoordinate = table.Column<decimal>(type: "numeric", nullable: false),
                    YCoordinate = table.Column<decimal>(type: "numeric", nullable: false),
                    ShowCoordinate = table.Column<int>(type: "integer", nullable: false),
                    IsCameraMovable = table.Column<int>(type: "integer", nullable: false),
                    ShowSubtitles = table.Column<int>(type: "integer", nullable: false),
                    BattleExists = table.Column<bool>(type: "boolean", nullable: false),
                    EnemyCharaId = table.Column<int>(type: "integer", nullable: false),
                    EnemyClass = table.Column<int>(type: "integer", nullable: false),
                    EnemyAiId = table.Column<int>(type: "integer", nullable: false),
                    BgFileName = table.Column<string>(type: "text", nullable: false),
                    ChapterEffectPath = table.Column<string>(type: "text", nullable: true),
                    ChapterClearTextId = table.Column<string>(type: "text", nullable: true),
                    Battle3dFieldId = table.Column<int>(type: "integer", nullable: false),
                    BgmId = table.Column<string>(type: "text", nullable: false),
                    SpecialBattleSettingId = table.Column<int>(type: "integer", nullable: true),
                    ReleasePoint = table.Column<int>(type: "integer", nullable: false),
                    IsMaintenanceChapter = table.Column<bool>(type: "boolean", nullable: false),
                    IsPlayAnotherEndAppearanceAnimation = table.Column<bool>(type: "boolean", nullable: false),
                    IsReleasedAnotherEnd = table.Column<bool>(type: "boolean", nullable: false),
                    IsSkipEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryChapters", x => x.StoryId);
                    table.ForeignKey(
                        name: "FK_StoryChapters_SpecialBattleSettings_SpecialBattleSettingId",
                        column: x => x.SpecialBattleSettingId,
                        principalTable: "SpecialBattleSettings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryChapters_StorySections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "StorySections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryChapterBattleSetting",
                columns: table => new
                {
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckClassId = table.Column<int>(type: "integer", nullable: false),
                    PlayerEmotionOverride = table.Column<int>(type: "integer", nullable: false),
                    EnemyEmotionOverride = table.Column<int>(type: "integer", nullable: false),
                    SkinIdOverride = table.Column<int>(type: "integer", nullable: false),
                    Battle3dFieldIdOverride = table.Column<int>(type: "integer", nullable: false),
                    BgmIdOverride = table.Column<int>(type: "integer", nullable: false),
                    DeckSkinIdOverride = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryChapterBattleSetting", x => new { x.StoryId, x.Id });
                    table.ForeignKey(
                        name: "FK_StoryChapterBattleSetting_StoryChapters_StoryId",
                        column: x => x.StoryId,
                        principalTable: "StoryChapters",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryChapterReward",
                columns: table => new
                {
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryChapterReward", x => new { x.StoryId, x.Id });
                    table.ForeignKey(
                        name: "FK_StoryChapterReward_StoryChapters_StoryId",
                        column: x => x.StoryId,
                        principalTable: "StoryChapters",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorySubChapter",
                columns: table => new
                {
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubChapterId = table.Column<int>(type: "integer", nullable: false),
                    SubChapterStoryId = table.Column<int>(type: "integer", nullable: false),
                    IsMaintenanceChapter = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorySubChapter", x => new { x.StoryId, x.Id });
                    table.ForeignKey(
                        name: "FK_StorySubChapter_StoryChapters_StoryId",
                        column: x => x.StoryId,
                        principalTable: "StoryChapters",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryChapters_NextChapterId",
                table: "StoryChapters",
                column: "NextChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryChapters_SectionId_CharaId_ChapterId",
                table: "StoryChapters",
                columns: new[] { "SectionId", "CharaId", "ChapterId" });

            migrationBuilder.CreateIndex(
                name: "IX_StoryChapters_SpecialBattleSettingId",
                table: "StoryChapters",
                column: "SpecialBattleSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_StorySections_WorldId",
                table: "StorySections",
                column: "WorldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryChapterBattleSetting");

            migrationBuilder.DropTable(
                name: "StoryChapterReward");

            migrationBuilder.DropTable(
                name: "StorySubChapter");

            migrationBuilder.DropTable(
                name: "ViewerStoryBranchUnlocks");

            migrationBuilder.DropTable(
                name: "ViewerStoryProgress");

            migrationBuilder.DropTable(
                name: "StoryChapters");

            migrationBuilder.DropTable(
                name: "SpecialBattleSettings");

            migrationBuilder.DropTable(
                name: "StorySections");

            migrationBuilder.DropTable(
                name: "StoryWorlds");
        }
    }
}
