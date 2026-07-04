using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Globals;

public class GlobalsRepository : IGlobalsRepository
{
    private readonly SVSimDbContext _dbContext;

    public GlobalsRepository(SVSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ClassExpEntry>> GetClassExpCurve()
    {
        return await _dbContext.Set<ClassExpEntry>().ToListAsync();
    }

    public async Task<List<BattlefieldEntry>> GetBattlefields(bool onlyOpen)
    {
        return await _dbContext.Set<BattlefieldEntry>().Where(bf => !onlyOpen || bf.IsOpen).ToListAsync();
    }

    public async Task<List<RankInfoEntry>> GetRankInfo()
    {
        return await _dbContext.Set<RankInfoEntry>().ToListAsync();
    }

    // ---------- Prod-captured globals ----------

    public Task<List<MyRotationSettingEntry>> GetMyRotationSettings() =>
        _dbContext.MyRotationSettings.AsNoTracking().ToListAsync();

    public Task<List<MyRotationAbilityEntry>> GetMyRotationAbilities() =>
        _dbContext.MyRotationAbilities.AsNoTracking().ToListAsync();

    public Task<List<AvatarAbilityEntry>> GetAvatarAbilities() =>
        _dbContext.AvatarAbilities.AsNoTracking().ToListAsync();

    public Task<List<DefaultDeckEntry>> GetDefaultDecks() =>
        _dbContext.DefaultDecks.AsNoTracking().ToListAsync();

    public Task<ArenaSeasonConfig?> GetCurrentArenaSeason() =>
        _dbContext.ArenaSeasons.AsNoTracking().FirstOrDefaultAsync(e => e.Id == 1);

    public Task<List<SpotCardEntry>> GetSpotCards() =>
        _dbContext.SpotCards.AsNoTracking().ToListAsync();

    public Task<List<ReprintedCardEntry>> GetReprintedCards() =>
        _dbContext.ReprintedCards.AsNoTracking().ToListAsync();

    public Task<List<UnlimitedRestrictionEntry>> GetUnlimitedRestrictions() =>
        _dbContext.UnlimitedRestrictions.AsNoTracking().ToListAsync();

    public Task<List<LoadingExclusionCardEntry>> GetLoadingExclusionCards() =>
        _dbContext.LoadingExclusionCards.AsNoTracking().ToListAsync();

    public Task<List<BattlePassLevelEntry>> GetBattlePassLevels() =>
        _dbContext.BattlePassLevels.AsNoTracking().ToListAsync();

    public Task<List<BannerEntry>> GetBanners() =>
        _dbContext.Banners.AsNoTracking().OrderBy(b => b.Id).ToListAsync();

    public async Task<IReadOnlyList<HomeDialogEntry>> GetActiveHomeDialogsAsync(DateTime nowUtc) =>
        await _dbContext.HomeDialogEntries.AsNoTracking()
            .Where(e => e.BeginTime <= nowUtc && e.EndTime > nowUtc)
            .OrderByDescending(e => e.Priority)
            .ThenBy(e => e.Id)
            .ToListAsync();

    public Task<SealedConfig?> GetCurrentSealedSeason() =>
        _dbContext.SealedSeasons.AsNoTracking().FirstOrDefaultAsync(e => e.Id == 1);

    /// <summary>Returns the master-point ranking period whose EndTime is in the future, or the latest by EndTime as fallback.</summary>
    public async Task<MasterPointRankingPeriodEntry?> GetCurrentMasterPointPeriod()
    {
        var now = DateTime.UtcNow;
        return await _dbContext.MasterPointRankingPeriods.AsNoTracking()
                   .Where(p => p.EndTime >= now)
                   .OrderBy(p => p.EndTime)
                   .FirstOrDefaultAsync()
               ?? await _dbContext.MasterPointRankingPeriods.AsNoTracking()
                   .OrderByDescending(p => p.EndTime)
                   .FirstOrDefaultAsync();
    }

    public Task<List<SpecialDeckFormatEntry>> GetActiveSpecialDeckFormats() =>
        _dbContext.SpecialDeckFormats.AsNoTracking().OrderBy(e => e.Id).ToListAsync();

    public Task<List<PaymentItemEntry>> GetPaymentItems() =>
        _dbContext.PaymentItems.AsNoTracking().OrderBy(e => e.Id).ToListAsync();

    public Task<List<MaintenanceCardEntry>> GetMaintenanceCards() =>
        _dbContext.MaintenanceCards.AsNoTracking().ToListAsync();

    public Task<List<FeatureMaintenanceEntry>> GetFeatureMaintenances() =>
        _dbContext.FeatureMaintenances.AsNoTracking().ToListAsync();

    public Task<PreReleaseInfo?> GetPreReleaseInfo() =>
        _dbContext.PreReleaseInfos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == 1);

    public Task<List<ShadowverseCardSetEntry>> GetRotationCardSets() =>
        _dbContext.CardSets.AsNoTracking().Where(s => s.IsInRotation).ToListAsync();

    public Task<List<PracticeOpponentEntry>> GetPracticeOpponents() =>
        _dbContext.PracticeOpponents.AsNoTracking().OrderBy(e => e.ClassId).ThenBy(e => e.Id).ToListAsync();

    public Task<List<BotRosterEntry>> GetBotRoster() =>
        _dbContext.BotRoster.AsNoTracking().OrderBy(e => e.ClassId).ThenBy(e => e.Id).ToListAsync();
}
