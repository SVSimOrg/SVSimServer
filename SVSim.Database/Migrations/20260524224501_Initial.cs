using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "ShortUdidSequence",
                startValue: 400000000L);

            migrationBuilder.CreateTable(
                name: "ArenaSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Enable = table.Column<int>(type: "integer", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RupyCost = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketCost = table.Column<int>(type: "integer", nullable: false),
                    IsJoin = table.Column<bool>(type: "boolean", nullable: false),
                    FormatInfo = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvatarAbilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false),
                    BattleStartFirstPlayerTurnBp = table.Column<int>(type: "integer", nullable: false),
                    BattleStartSecondPlayerTurnBp = table.Column<int>(type: "integer", nullable: false),
                    BattleStartMaxLife = table.Column<int>(type: "integer", nullable: false),
                    AbilityCost = table.Column<string>(type: "text", nullable: false),
                    Ability = table.Column<string>(type: "text", nullable: false),
                    PassiveAbility = table.Column<string>(type: "text", nullable: false),
                    AbilityDesc = table.Column<string>(type: "text", nullable: false),
                    PassiveAbilityDesc = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvatarAbilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    Click = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ChangeTime = table.Column<int>(type: "integer", nullable: false),
                    RemainingTime = table.Column<int>(type: "integer", nullable: false),
                    ImagePaths = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Battlefields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battlefields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BattlePassLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    RewardData = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattlePassLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsInRotation = table.Column<bool>(type: "boolean", nullable: false),
                    IsBasic = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassExpCurve",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    NecessaryExp = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassExpCurve", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colosseums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ColosseumId = table.Column<string>(type: "text", nullable: false),
                    ColosseumName = table.Column<string>(type: "text", nullable: false),
                    CardPoolName = table.Column<string>(type: "text", nullable: false),
                    DeckFormat = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NowRound = table.Column<string>(type: "text", nullable: false),
                    IsDisplayTips = table.Column<string>(type: "text", nullable: false),
                    TipsId = table.Column<string>(type: "text", nullable: false),
                    IsColosseumPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    IsRoundPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    IsNormalTwoPick = table.Column<string>(type: "text", nullable: false),
                    IsSpecialMode = table.Column<string>(type: "text", nullable: false),
                    IsAllCardEnabled = table.Column<int>(type: "integer", nullable: false),
                    SalesPeriodInfo = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colosseums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyLoginBonuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    BonusId = table.Column<int>(type: "integer", nullable: false),
                    BonusData = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyLoginBonuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultDecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DeckNo = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    SleeveId = table.Column<long>(type: "bigint", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false),
                    DeckName = table.Column<string>(type: "text", nullable: false),
                    CardIdArray = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultDecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultLeaderSkinSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    IsRandomLeaderSkin = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultLeaderSkinSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Degrees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degrees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emblems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emblems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureMaintenances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    FeatureKey = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureMaintenances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameConfigs",
                columns: table => new
                {
                    SectionName = table.Column<string>(type: "text", nullable: false),
                    ValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigs", x => x.SectionName);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadingExclusionCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingExclusionCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterPointRankingPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PeriodNum = table.Column<int>(type: "integer", nullable: false),
                    NecessaryScore = table.Column<long>(type: "bigint", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPointRankingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MyPageBackgrounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyPageBackgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MyRotationAbilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    AbilityId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyRotationAbilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MyRotationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RotationId = table.Column<int>(type: "integer", nullable: false),
                    CardSetIdsCsv = table.Column<string>(type: "text", nullable: false),
                    AbilitiesCsv = table.Column<string>(type: "text", nullable: false),
                    ReprintedCardIds = table.Column<string>(type: "jsonb", nullable: false),
                    RestrictedCardIds = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyRotationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Packs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    BasePackId = table.Column<int>(type: "integer", nullable: false),
                    GachaType = table.Column<int>(type: "integer", nullable: false),
                    PackCategory = table.Column<int>(type: "integer", nullable: false),
                    PosterType = table.Column<int>(type: "integer", nullable: false),
                    CommenceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalesPeriodTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SleeveId = table.Column<int>(type: "integer", nullable: false),
                    SpecialSleeveId = table.Column<int>(type: "integer", nullable: false),
                    OverrideDrawEffectPackId = table.Column<int>(type: "integer", nullable: false),
                    OverrideUiEffectPackId = table.Column<int>(type: "integer", nullable: false),
                    GachaDetail = table.Column<string>(type: "text", nullable: false),
                    IsHide = table.Column<bool>(type: "boolean", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    IsPreRelease = table.Column<bool>(type: "boolean", nullable: false),
                    OpenCountLimit = table.Column<int>(type: "integer", nullable: false),
                    GachaPointConfig_ExchangeablePoint = table.Column<int>(type: "integer", nullable: true),
                    GachaPointConfig_IncreaseGachaPoint = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    StoreProductId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ChargeCrystalNum = table.Column<int>(type: "integer", nullable: false),
                    FreeCrystalNum = table.Column<int>(type: "integer", nullable: false),
                    PurchaseLimit = table.Column<int>(type: "integer", nullable: false),
                    SpecialShopFlag = table.Column<int>(type: "integer", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RemainingTime = table.Column<int>(type: "integer", nullable: false),
                    IsResaleProduct = table.Column<int>(type: "integer", nullable: false),
                    ResaleStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PracticeOpponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PracticeId = table.Column<int>(type: "integer", nullable: false),
                    TextId = table.Column<string>(type: "text", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    CharaId = table.Column<int>(type: "integer", nullable: false),
                    DegreeId = table.Column<int>(type: "integer", nullable: false),
                    AiDeckLevel = table.Column<int>(type: "integer", nullable: false),
                    AiLogicLevel = table.Column<int>(type: "integer", nullable: false),
                    AiMaxLife = table.Column<int>(type: "integer", nullable: false),
                    Battle3dFieldId = table.Column<string>(type: "text", nullable: false),
                    IsMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    IsCampaignPractice = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeOpponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreReleaseInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PreReleaseId = table.Column<string>(type: "text", nullable: false),
                    NextCardSetId = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisplayEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FreeMatchStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardMasterId = table.Column<int>(type: "integer", nullable: false),
                    DefaultCardMasterId = table.Column<string>(type: "text", nullable: false),
                    PreReleaseCardMasterId = table.Column<string>(type: "text", nullable: false),
                    IsPreRotationFreeMatchTerm = table.Column<bool>(type: "boolean", nullable: false),
                    RotationCardSetIdList = table.Column<string>(type: "jsonb", nullable: false),
                    ReprintedBaseCardIds = table.Column<string>(type: "jsonb", nullable: false),
                    LatestReprintedBaseCardIds = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreReleaseInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NecessaryPoint = table.Column<int>(type: "integer", nullable: false),
                    AccumulatePoint = table.Column<int>(type: "integer", nullable: false),
                    LowerLimitPoint = table.Column<int>(type: "integer", nullable: false),
                    BaseAddBp = table.Column<int>(type: "integer", nullable: false),
                    BaseDropBp = table.Column<int>(type: "integer", nullable: false),
                    StreakBonusPt = table.Column<int>(type: "integer", nullable: false),
                    WinBonus = table.Column<double>(type: "double precision", nullable: false),
                    LoseBonus = table.Column<double>(type: "double precision", nullable: false),
                    MaxWinBonus = table.Column<int>(type: "integer", nullable: false),
                    MaxLoseBonus = table.Column<int>(type: "integer", nullable: false),
                    IsPromotionWar = table.Column<int>(type: "integer", nullable: false),
                    MatchCount = table.Column<int>(type: "integer", nullable: false),
                    NecessaryWin = table.Column<int>(type: "integer", nullable: false),
                    ResetLose = table.Column<int>(type: "integer", nullable: false),
                    AccumulateMasterPoint = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReprintedCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReprintedCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SealedSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Enable = table.Column<int>(type: "integer", nullable: false),
                    CrystalCost = table.Column<int>(type: "integer", nullable: false),
                    RupyCost = table.Column<int>(type: "integer", nullable: false),
                    TicketCost = table.Column<int>(type: "integer", nullable: false),
                    DeckUsingNumMin = table.Column<int>(type: "integer", nullable: false),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    IsJoin = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeckCodeMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    PackInfo = table.Column<string>(type: "jsonb", nullable: false),
                    SalesPeriodInfo = table.Column<string>(type: "jsonb", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SealedSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sleeves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sleeves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialDeckFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DeckFormat = table.Column<string>(type: "text", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialDeckFormats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpotCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    Cost = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnlimitedRestrictions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    RestrictionValue = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnlimitedRestrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Attack = table.Column<int>(type: "integer", nullable: true),
                    Defense = table.Column<int>(type: "integer", nullable: true),
                    PrimaryResourceCost = table.Column<int>(type: "integer", nullable: true),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    IsFoil = table.Column<bool>(type: "boolean", nullable: false),
                    CollectionInfo_CraftCost = table.Column<int>(type: "integer", nullable: true),
                    CollectionInfo_DustReward = table.Column<int>(type: "integer", nullable: true),
                    ClassId = table.Column<int>(type: "integer", nullable: true),
                    ShadowverseCardSetEntryId = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_CardSets_ShadowverseCardSetEntryId",
                        column: x => x.ShadowverseCardSetEntryId,
                        principalTable: "CardSets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cards_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaderSkins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EmoteId = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderSkins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderSkins_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Viewers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    ShortUdid = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"ShortUdidSequence\"')"),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Info_BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Info_CountryCode = table.Column<string>(type: "text", nullable: false),
                    Info_MaxFriends = table.Column<int>(type: "integer", nullable: false),
                    Info_IsOfficial = table.Column<bool>(type: "boolean", nullable: false),
                    Info_IsOfficialMarkDisplayed = table.Column<bool>(type: "boolean", nullable: false),
                    Info_SelectedEmblemId = table.Column<int>(type: "integer", nullable: false),
                    Info_SelectedDegreeId = table.Column<int>(type: "integer", nullable: false),
                    MissionData_HasReceivedPickTwoMission = table.Column<bool>(type: "boolean", nullable: false),
                    MissionData_MissionReceiveType = table.Column<int>(type: "integer", nullable: false),
                    MissionData_MissionChangeTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MissionData_TutorialState = table.Column<int>(type: "integer", nullable: false),
                    Currency_Crystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_AndroidCrystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_IosCrystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_SteamCrystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_DmmCrystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_FreeCrystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_LifeTotalCrystals = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_RedEther = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Currency_Rupees = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Viewers_Degrees_Info_SelectedDegreeId",
                        column: x => x.Info_SelectedDegreeId,
                        principalTable: "Degrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Viewers_Emblems_Info_SelectedEmblemId",
                        column: x => x.Info_SelectedEmblemId,
                        principalTable: "Emblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackBannerEntry",
                columns: table => new
                {
                    PackConfigEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BannerName = table.Column<string>(type: "text", nullable: false),
                    DialogTitle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackBannerEntry", x => new { x.PackConfigEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_PackBannerEntry_Packs_PackConfigEntryId",
                        column: x => x.PackConfigEntryId,
                        principalTable: "Packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackChildGachaEntry",
                columns: table => new
                {
                    PackConfigEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GachaId = table.Column<int>(type: "integer", nullable: false),
                    TypeDetail = table.Column<int>(type: "integer", nullable: false),
                    Cost = table.Column<int>(type: "integer", nullable: false),
                    CardCount = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: true),
                    IsDailySingle = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideIncreaseGachaPoint = table.Column<int>(type: "integer", nullable: false),
                    PurchaseLimitCount = table.Column<int>(type: "integer", nullable: false),
                    FreeGachaCampaignId = table.Column<int>(type: "integer", nullable: true),
                    CampaignName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackChildGachaEntry", x => new { x.PackConfigEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_PackChildGachaEntry_Packs_PackConfigEntryId",
                        column: x => x.PackConfigEntryId,
                        principalTable: "Packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardCosmeticRewards",
                columns: table => new
                {
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CosmeticId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardCosmeticRewards", x => new { x.CardId, x.Type, x.CosmeticId });
                    table.ForeignKey(
                        name: "FK_CardCosmeticRewards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    RandomLeaderSkin = table.Column<bool>(type: "boolean", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    SleeveId = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false),
                    ViewerId = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decks_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Decks_LeaderSkins_LeaderSkinId",
                        column: x => x.LeaderSkinId,
                        principalTable: "LeaderSkins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Decks_Sleeves_SleeveId",
                        column: x => x.SleeveId,
                        principalTable: "Sleeves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Decks_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DegreeEntryViewer",
                columns: table => new
                {
                    DegreesId = table.Column<int>(type: "integer", nullable: false),
                    ViewersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DegreeEntryViewer", x => new { x.DegreesId, x.ViewersId });
                    table.ForeignKey(
                        name: "FK_DegreeEntryViewer_Degrees_DegreesId",
                        column: x => x.DegreesId,
                        principalTable: "Degrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DegreeEntryViewer_Viewers_ViewersId",
                        column: x => x.ViewersId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmblemEntryViewer",
                columns: table => new
                {
                    EmblemsId = table.Column<int>(type: "integer", nullable: false),
                    ViewersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmblemEntryViewer", x => new { x.EmblemsId, x.ViewersId });
                    table.ForeignKey(
                        name: "FK_EmblemEntryViewer_Emblems_EmblemsId",
                        column: x => x.EmblemsId,
                        principalTable: "Emblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmblemEntryViewer_Viewers_ViewersId",
                        column: x => x.ViewersId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderSkinEntryViewer",
                columns: table => new
                {
                    LeaderSkinsId = table.Column<int>(type: "integer", nullable: false),
                    ViewersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderSkinEntryViewer", x => new { x.LeaderSkinsId, x.ViewersId });
                    table.ForeignKey(
                        name: "FK_LeaderSkinEntryViewer_LeaderSkins_LeaderSkinsId",
                        column: x => x.LeaderSkinsId,
                        principalTable: "LeaderSkins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderSkinEntryViewer_Viewers_ViewersId",
                        column: x => x.ViewersId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyPageBackgroundEntryViewer",
                columns: table => new
                {
                    MyPageBackgroundsId = table.Column<int>(type: "integer", nullable: false),
                    ViewersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyPageBackgroundEntryViewer", x => new { x.MyPageBackgroundsId, x.ViewersId });
                    table.ForeignKey(
                        name: "FK_MyPageBackgroundEntryViewer_MyPageBackgrounds_MyPageBackgro~",
                        column: x => x.MyPageBackgroundsId,
                        principalTable: "MyPageBackgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MyPageBackgroundEntryViewer_Viewers_ViewersId",
                        column: x => x.ViewersId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnedCardEntry",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    IsProtected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnedCardEntry", x => new { x.ViewerId, x.Id });
                    table.ForeignKey(
                        name: "FK_OwnedCardEntry_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnedCardEntry_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnedItemEntry",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnedItemEntry", x => new { x.ViewerId, x.Id });
                    table.ForeignKey(
                        name: "FK_OwnedItemEntry_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnedItemEntry_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleeveEntryViewer",
                columns: table => new
                {
                    SleevesId = table.Column<int>(type: "integer", nullable: false),
                    ViewersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleeveEntryViewer", x => new { x.SleevesId, x.ViewersId });
                    table.ForeignKey(
                        name: "FK_SleeveEntryViewer_Sleeves_SleevesId",
                        column: x => x.SleevesId,
                        principalTable: "Sleeves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SleeveEntryViewer_Viewers_ViewersId",
                        column: x => x.ViewersId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialAccountConnection",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialAccountConnection", x => new { x.ViewerId, x.Id });
                    table.ForeignKey(
                        name: "FK_SocialAccountConnection_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewerClassData",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Exp = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerClassData", x => new { x.ViewerId, x.Id });
                    table.ForeignKey(
                        name: "FK_ViewerClassData_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewerClassData_LeaderSkins_LeaderSkinId",
                        column: x => x.LeaderSkinId,
                        principalTable: "LeaderSkins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewerClassData_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewerPackOpenCount",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    OpenCount = table.Column<int>(type: "integer", nullable: false),
                    LastDailyFreeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerPackOpenCount", x => new { x.ViewerId, x.Id });
                    table.ForeignKey(
                        name: "FK_ViewerPackOpenCount_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckCard",
                columns: table => new
                {
                    ShadowverseDeckEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckCard", x => new { x.ShadowverseDeckEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_DeckCard_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckCard_Decks_ShadowverseDeckEntryId",
                        column: x => x.ShadowverseDeckEntryId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardCosmeticRewards_CardId",
                table: "CardCosmeticRewards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ClassId",
                table: "Cards",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ShadowverseCardSetEntryId",
                table: "Cards",
                column: "ShadowverseCardSetEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCard_CardId",
                table: "DeckCard",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_ClassId",
                table: "Decks",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_LeaderSkinId",
                table: "Decks",
                column: "LeaderSkinId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_SleeveId",
                table: "Decks",
                column: "SleeveId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_ViewerId",
                table: "Decks",
                column: "ViewerId");

            migrationBuilder.CreateIndex(
                name: "IX_DegreeEntryViewer_ViewersId",
                table: "DegreeEntryViewer",
                column: "ViewersId");

            migrationBuilder.CreateIndex(
                name: "IX_EmblemEntryViewer_ViewersId",
                table: "EmblemEntryViewer",
                column: "ViewersId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderSkinEntryViewer_ViewersId",
                table: "LeaderSkinEntryViewer",
                column: "ViewersId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderSkins_ClassId",
                table: "LeaderSkins",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MyPageBackgroundEntryViewer_ViewersId",
                table: "MyPageBackgroundEntryViewer",
                column: "ViewersId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedCardEntry_CardId",
                table: "OwnedCardEntry",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnedItemEntry_ItemId",
                table: "OwnedItemEntry",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SleeveEntryViewer_ViewersId",
                table: "SleeveEntryViewer",
                column: "ViewersId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerClassData_ClassId",
                table: "ViewerClassData",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerClassData_LeaderSkinId",
                table: "ViewerClassData",
                column: "LeaderSkinId");

            migrationBuilder.CreateIndex(
                name: "IX_Viewers_Info_SelectedDegreeId",
                table: "Viewers",
                column: "Info_SelectedDegreeId");

            migrationBuilder.CreateIndex(
                name: "IX_Viewers_Info_SelectedEmblemId",
                table: "Viewers",
                column: "Info_SelectedEmblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Viewers_ShortUdid",
                table: "Viewers",
                column: "ShortUdid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArenaSeasons");

            migrationBuilder.DropTable(
                name: "AvatarAbilities");

            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "Battlefields");

            migrationBuilder.DropTable(
                name: "BattlePassLevels");

            migrationBuilder.DropTable(
                name: "CardCosmeticRewards");

            migrationBuilder.DropTable(
                name: "ClassExpCurve");

            migrationBuilder.DropTable(
                name: "Colosseums");

            migrationBuilder.DropTable(
                name: "DailyLoginBonuses");

            migrationBuilder.DropTable(
                name: "DeckCard");

            migrationBuilder.DropTable(
                name: "DefaultDecks");

            migrationBuilder.DropTable(
                name: "DefaultLeaderSkinSettings");

            migrationBuilder.DropTable(
                name: "DegreeEntryViewer");

            migrationBuilder.DropTable(
                name: "EmblemEntryViewer");

            migrationBuilder.DropTable(
                name: "FeatureMaintenances");

            migrationBuilder.DropTable(
                name: "GameConfigs");

            migrationBuilder.DropTable(
                name: "LeaderSkinEntryViewer");

            migrationBuilder.DropTable(
                name: "LoadingExclusionCards");

            migrationBuilder.DropTable(
                name: "MaintenanceCards");

            migrationBuilder.DropTable(
                name: "MasterPointRankingPeriods");

            migrationBuilder.DropTable(
                name: "MyPageBackgroundEntryViewer");

            migrationBuilder.DropTable(
                name: "MyRotationAbilities");

            migrationBuilder.DropTable(
                name: "MyRotationSettings");

            migrationBuilder.DropTable(
                name: "OwnedCardEntry");

            migrationBuilder.DropTable(
                name: "OwnedItemEntry");

            migrationBuilder.DropTable(
                name: "PackBannerEntry");

            migrationBuilder.DropTable(
                name: "PackChildGachaEntry");

            migrationBuilder.DropTable(
                name: "PaymentItems");

            migrationBuilder.DropTable(
                name: "PracticeOpponents");

            migrationBuilder.DropTable(
                name: "PreReleaseInfos");

            migrationBuilder.DropTable(
                name: "RankInfo");

            migrationBuilder.DropTable(
                name: "ReprintedCards");

            migrationBuilder.DropTable(
                name: "SealedSeasons");

            migrationBuilder.DropTable(
                name: "SleeveEntryViewer");

            migrationBuilder.DropTable(
                name: "SocialAccountConnection");

            migrationBuilder.DropTable(
                name: "SpecialDeckFormats");

            migrationBuilder.DropTable(
                name: "SpotCards");

            migrationBuilder.DropTable(
                name: "UnlimitedRestrictions");

            migrationBuilder.DropTable(
                name: "ViewerClassData");

            migrationBuilder.DropTable(
                name: "ViewerPackOpenCount");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropTable(
                name: "MyPageBackgrounds");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Packs");

            migrationBuilder.DropTable(
                name: "LeaderSkins");

            migrationBuilder.DropTable(
                name: "Sleeves");

            migrationBuilder.DropTable(
                name: "Viewers");

            migrationBuilder.DropTable(
                name: "CardSets");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Degrees");

            migrationBuilder.DropTable(
                name: "Emblems");

            migrationBuilder.DropSequence(
                name: "ShortUdidSequence");
        }
    }
}
