using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Card;
using SVSim.Database.Repositories.Collectibles;

namespace SVSim.Database.Services.Inventory;

public sealed class InventoryService : IInventoryService
{
    private readonly SVSimDbContext _db;
    private readonly IGameConfigService _config;
    private readonly ICardRepository _cards;
    private readonly ICollectionRepository _collection;
    private readonly ILogger<InventoryService> _log;

    public InventoryService(
        SVSimDbContext db,
        IGameConfigService config,
        ICardRepository cards,
        ICollectionRepository collection,
        ILogger<InventoryService> log)
    {
        _db = db;
        _config = config;
        _cards = cards;
        _collection = collection;
        _log = log;
    }

    public async Task<IInventoryTransaction> BeginAsync(
        long viewerId,
        CancellationToken ct = default,
        Action<InventoryLoadConfig>? configure = null)
    {
        var loadCfg = new InventoryLoadConfig();
        configure?.Invoke(loadCfg);

        IQueryable<Viewer> query = _db.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .Include(v => v.Sleeves)
            .Include(v => v.Emblems)
            .Include(v => v.LeaderSkins)
            .Include(v => v.Degrees)
            .Include(v => v.MyPageBackgrounds)
            .Include(v => v.Items).ThenInclude(i => i.Item);

        foreach (var include in loadCfg.Includes)
            query = include(query);

        var viewer = await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct)
            ?? throw new InventoryViewerNotFoundException(viewerId);

        var freeplay = _config.Get<FreeplayConfig>();
        var dbTx = await _db.Database.BeginTransactionAsync(ct);

        return new InventoryTransaction(_db, dbTx, viewer, freeplay, loadCfg.Source, _log);
    }

    public long EffectiveBalance(Viewer viewer, SpendCurrency currency)
    {
        var cfg = _config.Get<FreeplayConfig>();
        if (cfg.Enabled && currency != SpendCurrency.SpotPoint)
            return checked((long)cfg.CurrencyAmount);

        return currency switch
        {
            SpendCurrency.Crystal => (long)viewer.Currency.Crystals,
            SpendCurrency.Rupee => (long)viewer.Currency.Rupees,
            SpendCurrency.RedEther => (long)viewer.Currency.RedEther,
            SpendCurrency.SpotPoint => (long)viewer.Currency.SpotPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(currency)),
        };
    }

    public async Task<IReadOnlyList<OwnedCardEntry>> EffectiveOwnedCardsAsync(
        Viewer viewer, CancellationToken ct = default)
    {
        var defaults = await _cards.GetDefaultCards();
        var defaultIds = defaults.Select(c => c.Id).ToHashSet();
        var cfg = _config.Get<FreeplayConfig>();

        if (cfg.Enabled)
        {
            var all = await _cards.GetAll(onlyCollectible: true);
            return all
                .Select(c => new OwnedCardEntry
                {
                    Card = c,
                    Count = cfg.CardCopies,
                    IsProtected = defaultIds.Contains(c.Id),
                })
                .ToList();
        }

        var owned = viewer.Cards.Where(c => c.Count > 0 && !defaultIds.Contains(c.Card.Id));
        return owned
            .Concat(defaults.Select(bc => new OwnedCardEntry { Card = bc, Count = 3, IsProtected = true }))
            .ToList();
    }

    public async Task<EffectiveCosmetics> EffectiveCosmeticsAsync(
        Viewer viewer, CancellationToken ct = default)
    {
        var allSkins = await _collection.GetLeaderSkins();
        var cfg = _config.Get<FreeplayConfig>();

        if (cfg.Enabled)
        {
            return new EffectiveCosmetics(
                await _collection.GetAllSleeveIds(),
                await _collection.GetAllEmblemIds(),
                await _collection.GetAllDegreeIds(),
                await _collection.GetAllMyPageBackgroundIds(),
                allSkins,
                allSkins.Select(s => s.Id).ToHashSet());
        }

        return new EffectiveCosmetics(
            viewer.Sleeves.Select(s => s.Id).ToList(),
            viewer.Emblems.Select(e => e.Id).ToList(),
            viewer.Degrees.Select(d => d.Id).ToList(),
            viewer.MyPageBackgrounds.Select(m => m.Id).ToList(),
            allSkins,
            viewer.LeaderSkins.Select(s => s.Id).ToHashSet());
    }
}
