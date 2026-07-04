using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;

namespace SVSim.Database.Repositories.Card;

public class CardInventoryRepository : ICardInventoryRepository
{
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;

    public CardInventoryRepository(SVSimDbContext db, IInventoryService inv)
    {
        _db = db;
        _inv = inv;
    }

    public async Task<DestructOutcome> DestructCards(long viewerId, IReadOnlyDictionary<long, int> destructCounts)
    {
        // Load covers cards + currency + decks. DeckCard.Card and OwnedCardEntry.Card both
        // need explicit Includes — owned-collection auto-loading does not cover nested nav refs
        // (see project_ef_nav_include_pitfall memory).
        //
        // AsSplitQuery is essential here. Without it, EF emits one SQL with a cartesian JOIN
        // across OwnedCardEntry × DeckCard, materializing ~|owned_cards| × |deck_cards| rows
        // for a single destruct. For a real account that's ~1500 × ~1600 = 2.4M rows and ~5s
        // round-trip. Split queries issue separate SELECTs per Include chain — total rows
        // stay linear in the data instead of multiplicative.
        var viewer = await _db.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card).ThenInclude(c => c.CollectionInfo)
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .AsSplitQuery()
            .FirstAsync(v => v.Id == viewerId);

        var ownedByCardId = viewer.Cards.ToDictionary(c => c.Card.Id);

        foreach (var (cardId, num) in destructCounts)
        {
            // TryGetValue can succeed with Card.Id == 0 due to an EF owned-collection nav-ref
            // default-init quirk (see project_ef_nav_include_pitfall memory).
            if (!ownedByCardId.TryGetValue(cardId, out var owned) || owned.Card.Id == 0)
                return DestructOutcome.Fail(DestructError.UnknownCard);
            if (owned.IsProtected)
                return DestructOutcome.Fail(DestructError.CardProtected);
            if (owned.Card.CollectionInfo is null || owned.Card.CollectionInfo.DustReward <= 0)
                return DestructOutcome.Fail(DestructError.NotDestructible);
            if (owned.Count < num)
                return DestructOutcome.Fail(DestructError.InsufficientCards);
        }

        using var tx = await _db.Database.BeginTransactionAsync();

        ulong totalVials = 0;
        var postCounts = new Dictionary<long, int>(destructCounts.Count);
        foreach (var (cardId, num) in destructCounts)
        {
            var owned = ownedByCardId[cardId];
            owned.Count -= num;
            totalVials += (ulong)owned.Card.CollectionInfo!.DustReward * (ulong)num;
            postCounts[cardId] = owned.Count;
        }
        // Direct credit (not via RewardGrantService.ApplyAsync) because destruct is a debit-pair
        // operation (destroy cards + credit vials) handled atomically here. ApplyAsync is the
        // standard path for one-shot reward grants — see RewardGrantService for that pattern.
        viewer.Currency.RedEther += totalVials;

        // Deck auto-strip: any deck holding more copies of a destructed card than the viewer now owns
        // has the excess removed. DeckCard.Count is the multiplicity; a row that hits 0 is deleted so
        // wire serialization (card_id_array expansion) doesn't emit a phantom.
        foreach (var deck in viewer.Decks)
        {
            foreach (var deckCard in deck.Cards.ToList())
            {
                if (!postCounts.TryGetValue(deckCard.Card.Id, out int newOwned))
                    continue;
                int excess = deckCard.Count - newOwned;
                if (excess <= 0)
                    continue;
                deckCard.Count -= excess;
                if (deckCard.Count == 0)
                    deck.Cards.Remove(deckCard);
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return DestructOutcome.Ok(new DestructResult(viewer.Currency.RedEther, postCounts));
    }

    public async Task<CreateOutcome> CreateCards(long viewerId, IReadOnlyDictionary<long, int> createCounts)
    {
        // Load viewer with owned cards + their catalog rows (for CraftCost). Decks aren't needed —
        // create never modifies them. AsSplitQuery for symmetry with destruct and to avoid any
        // future cartesian explosion if more Includes are added.
        var viewer = await _db.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card).ThenInclude(c => c.CollectionInfo)
            .AsSplitQuery()
            .FirstAsync(v => v.Id == viewerId);

        var ownedByCardId = viewer.Cards.ToDictionary(c => c.Card.Id);

        // For unknown_card validation we need the catalog rows for ids the viewer DOESN'T own yet.
        var requestedIds = createCounts.Keys.ToList();
        var catalogRows = await _db.Cards
            .Include(c => c.CollectionInfo)
            .Where(c => requestedIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        ulong totalCost = 0;
        foreach (var (cardId, num) in createCounts)
        {
            // unknown_card: must be in the global catalog
            if (!catalogRows.TryGetValue(cardId, out var catalogCard))
                return CreateOutcome.Fail(CreateError.UnknownCard);

            // not_craftable: client's IsNotCraftDestruct check — CraftCost ≤ 0 means uncraftable
            if (catalogCard.CollectionInfo is null || catalogCard.CollectionInfo.CraftCost <= 0)
                return CreateOutcome.Fail(CreateError.NotCraftable);

            // would_exceed_max_copies: viewer already owns N → can craft at most MaxCopies - N
            int existingCount = ownedByCardId.TryGetValue(cardId, out var owned) && owned.Card.Id != 0
                ? owned.Count
                : 0;
            if (existingCount + num > OwnedCardEntry.MaxCopies)
                return CreateOutcome.Fail(CreateError.WouldExceedMaxCopies);

            totalCost += (ulong)catalogCard.CollectionInfo.CraftCost * (ulong)num;
        }

        // insufficient_vials pre-check (validation-before-mutation atomicity, keeps same error ordering)
        if (viewer.Currency.RedEther < totalCost)
            return CreateOutcome.Fail(CreateError.InsufficientVials);

        // Mutation phase via InventoryService transaction — freeplay-aware RedEther debit,
        // card grants with cosmetic cascade.
        await using var tx = await _inv.BeginAsync(viewerId, configure: cfg => cfg.Source = GrantSource.CardCraft);

        var spendResult = await tx.TrySpendAsync(SpendCurrency.RedEther, (long)totalCost);
        if (!spendResult.Success)
            return CreateOutcome.Fail(CreateError.InsufficientVials);

        var allGrants = new List<GrantedReward>();
        foreach (var (cardId, num) in createCounts)
        {
            var granted = await tx.GrantAsync(UserGoodsType.Card, cardId, num);
            allGrants.AddRange(granted);
        }

        await tx.CommitAsync();
        return CreateOutcome.Ok(new CreateResult(tx.Viewer.Currency.RedEther, allGrants));
    }

    public async Task<ProtectOutcome> SetProtected(long viewerId, long cardId, bool isProtected)
    {
        // Lighter load than create/destruct: only need viewer's owned-cards collection. No decks,
        // no currency, no CollectionInfo.
        var viewer = await _db.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);

        var owned = viewer.Cards.FirstOrDefault(c => c.Card.Id == cardId);
        if (owned is null || owned.Card.Id == 0)
            return ProtectOutcome.Fail(ProtectError.UnknownCard);

        owned.IsProtected = isProtected;
        await _db.SaveChangesAsync();
        return ProtectOutcome.Ok();
    }
}
