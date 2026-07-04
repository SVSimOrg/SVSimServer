using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Globals;

public interface IGlobalsRepository
{
    Task<List<ClassExpEntry>> GetClassExpCurve();
    Task<List<BattlefieldEntry>> GetBattlefields(bool onlyOpen);
    Task<List<RankInfoEntry>> GetRankInfo();

    // Seed-driven globals — populated by per-domain importers in SVSim.Bootstrap.
    Task<List<MyRotationSettingEntry>> GetMyRotationSettings();
    Task<List<MyRotationAbilityEntry>> GetMyRotationAbilities();
    Task<List<AvatarAbilityEntry>> GetAvatarAbilities();
    Task<List<DefaultDeckEntry>> GetDefaultDecks();
    Task<ArenaSeasonConfig?> GetCurrentArenaSeason();
    Task<List<SpotCardEntry>> GetSpotCards();
    Task<List<ReprintedCardEntry>> GetReprintedCards();
    Task<List<UnlimitedRestrictionEntry>> GetUnlimitedRestrictions();
    Task<List<LoadingExclusionCardEntry>> GetLoadingExclusionCards();
    Task<List<BattlePassLevelEntry>> GetBattlePassLevels();
    Task<List<BannerEntry>> GetBanners();
    Task<IReadOnlyList<HomeDialogEntry>> GetActiveHomeDialogsAsync(DateTime nowUtc);
    Task<SealedConfig?> GetCurrentSealedSeason();
    Task<MasterPointRankingPeriodEntry?> GetCurrentMasterPointPeriod();
    Task<List<SpecialDeckFormatEntry>> GetActiveSpecialDeckFormats();
    Task<List<PaymentItemEntry>> GetPaymentItems();
    Task<List<MaintenanceCardEntry>> GetMaintenanceCards();
    Task<List<FeatureMaintenanceEntry>> GetFeatureMaintenances();
    Task<PreReleaseInfo?> GetPreReleaseInfo();
    Task<List<ShadowverseCardSetEntry>> GetRotationCardSets();
    Task<List<PracticeOpponentEntry>> GetPracticeOpponents();
    Task<List<BotRosterEntry>> GetBotRoster();
}
