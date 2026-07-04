using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Card;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Card;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Card;
using System.Text.Json;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /card/* — viewer card-inventory mutations. Ships /card/destruct, /card/create, /card/protect,
/// /card/create_foil_card.
/// </summary>
[Route("card")]
public class CardController : SVSimController
{
    /// <summary>Item catalog id for Orb (Seer's Globe currency). Source: PremiumCardConversionTask</summary>
    private const int OrbItemId = 1000;

    /// <summary>Per-card ownership cap mirrored from the client gate (CardDetailUI.cs:2261).</summary>
    private const int MaxCardCopies = 3;

    private readonly ICardInventoryRepository _inventory;
    private readonly IInventoryService _inv;
    private readonly SVSimDbContext _db;
    private readonly ILogger<CardController> _log;

    public CardController(ICardInventoryRepository inventory, IInventoryService inv, SVSimDbContext db, ILogger<CardController> log)
    {
        _inventory = inventory;
        _inv = inv;
        _db = db;
        _log = log;
    }

    [HttpPost("destruct")]
    public async Task<ActionResult<CardDestructResponse>> Destruct(CardDestructRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (!TryParseCardCountDict(request.CardIdNumberArray, out var destructCounts, out var snapshots, out var parseError))
            return BadRequest(new { error = parseError });

        if (destructCounts.Count == 0)
            return BadRequest(new { error = "malformed_request" });

        var outcome = await _inventory.DestructCards(viewerId, destructCounts);
        if (!outcome.IsSuccess)
            return BadRequest(new { error = ErrorKey(outcome.Error!.Value) });

        // Client snapshot mismatch is warn-log only; never blocks the request.
        foreach (var (cardId, snapshot) in snapshots)
        {
            // We don't carry pre-state counts back, but post + destructed = pre.
            int destructedNum = destructCounts[cardId];
            int reconstructedPre = outcome.Result!.NewOwnedCounts[cardId] + destructedNum;
            if (reconstructedPre != snapshot)
            {
                _log.LogWarning(
                    "Destruct possession-snapshot mismatch: card={CardId} client_snapshot={Snapshot} server_pre={ServerPre}",
                    cardId, snapshot, reconstructedPre);
            }
        }

        // Wire spec is int; clamp the ulong total so a hypothetical 2B+ balance can't underflow
        // to a negative wire value. Realistic balances are well under int.MaxValue.
        int redEtherWire = outcome.Result!.NewRedEtherTotal > int.MaxValue
            ? int.MaxValue
            : (int)outcome.Result!.NewRedEtherTotal;
        var rewardList = new List<RewardListEntry>
        {
            new() { RewardType = 1, RewardId = 0, RewardNum = redEtherWire },
        };
        foreach (var (cardId, postCount) in outcome.Result!.NewOwnedCounts)
        {
            rewardList.Add(new RewardListEntry { RewardType = 5, RewardId = cardId, RewardNum = postCount });
        }

        return new CardDestructResponse { RewardList = rewardList };
    }

    [HttpPost("create")]
    public async Task<ActionResult<CardCreateResponse>> Create(CardCreateRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (!TryParseCardCountDict(request.CardIdNumberArray, out var createCounts, out var snapshots, out var parseError))
            return BadRequest(new { error = parseError });

        if (createCounts.Count == 0)
            return BadRequest(new { error = "malformed_request" });

        var outcome = await _inventory.CreateCards(viewerId, createCounts);
        if (!outcome.IsSuccess)
            return BadRequest(new { error = CreateErrorKey(outcome.Error!.Value) });

        // Snapshot mismatch is warn-log only. pre-state = post-state - num.
        var grants = outcome.Result!.Grants;
        foreach (var (cardId, snapshot) in snapshots)
        {
            int requestedNum = createCounts[cardId];
            int postCount = grants.FirstOrDefault(g => g.RewardType == UserGoodsType.Card && g.RewardId == cardId)?.RewardNum ?? 0;
            int reconstructedPre = postCount - requestedNum;
            if (reconstructedPre != snapshot)
            {
                _log.LogWarning(
                    "Create possession-snapshot mismatch: card={CardId} client_snapshot={Snapshot} server_pre={ServerPre}",
                    cardId, snapshot, reconstructedPre);
            }
        }

        // Wire spec is int; clamp the ulong total so a hypothetical 2B+ balance can't underflow
        // to a negative wire value. Mirrors destruct's clamp.
        int redEtherWire = outcome.Result!.NewRedEtherTotal > int.MaxValue
            ? int.MaxValue
            : (int)outcome.Result!.NewRedEtherTotal;
        var rewardList = new List<RewardListEntry>
        {
            new() { RewardType = (int)UserGoodsType.RedEther, RewardId = 0, RewardNum = redEtherWire },
        };
        foreach (var grant in grants)
        {
            rewardList.Add(new RewardListEntry
            {
                RewardType = (int)grant.RewardType,
                RewardId   = grant.RewardId,
                RewardNum  = grant.RewardNum,
            });
        }

        return new CardCreateResponse { RewardList = rewardList };
    }

    private static string CreateErrorKey(CreateError error) => error switch
    {
        CreateError.UnknownCard          => "unknown_card",
        CreateError.NotCraftable         => "not_craftable",
        CreateError.WouldExceedMaxCopies => "would_exceed_max_copies",
        CreateError.InsufficientVials    => "insufficient_vials",
        _ => "malformed_request",
    };

    /// <summary>
    /// Seer's Globe conversion (decomp: Wizard/PremiumCardConversionTask.cs). Spends N Orbs
    /// (Item 1000) + N copies of the base card, grants N copies of the foil twin (base.Id + 1).
    /// Foil convention is the documented one on <see cref="ShadowverseCardEntry.IsFoil"/> —
    /// the foil twin shares the base's CardSet at card_id = base + 1. The 3-copy cap is mirrored
    /// from the client gate (CardDetailUI.cs:2261); reject server-side too so direct API hits
    /// can't bypass.
    /// </summary>
    [HttpPost("create_foil_card")]
    public async Task<ActionResult<CardCreateFoilCardResponse>> CreateFoilCard(
        CardCreateFoilCardRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        if (request.CreateNumber < 1) return BadRequest(new { error = "invalid_create_number" });

        var baseCard = await _db.Set<ShadowverseCardEntry>()
            .FirstOrDefaultAsync(c => c.Id == request.BaseCardId, ct);
        if (baseCard is null || baseCard.IsFoil)
            return BadRequest(new { error = "invalid_base_card" });

        var foilCard = await _db.Set<ShadowverseCardEntry>()
            .FirstOrDefaultAsync(c => c.Id == baseCard.Id + 1 && c.IsFoil, ct);
        if (foilCard is null)
            return BadRequest(new { error = "no_foil_variant" });

        await using var tx = await _inv.BeginAsync(viewerId, ct);
        var viewer = tx.Viewer;

        var baseOwned = viewer.Cards.FirstOrDefault(c => c.Card.Id == baseCard.Id);
        if (baseOwned is null || baseOwned.Count < request.CreateNumber)
            return BadRequest(new { error = "insufficient_base_card" });

        var foilOwned = viewer.Cards.FirstOrDefault(c => c.Card.Id == foilCard.Id);
        int existingFoilCount = foilOwned?.Count ?? 0;
        if (existingFoilCount + request.CreateNumber > MaxCardCopies)
            return BadRequest(new { error = "would_exceed_max_copies" });

        // Snapshot mismatch is warn-log only — matches the /card/create + /card/destruct pattern.
        if (request.BaseCardNumber != baseOwned.Count)
        {
            _log.LogWarning(
                "CreateFoilCard possession-snapshot mismatch: card={BaseCardId} client_snapshot={Snapshot} server={Server}",
                request.BaseCardId, request.BaseCardNumber, baseOwned.Count);
        }

        // Spend Orbs first so an Orb-poor viewer keeps their base copy on failure. The inventory
        // facade throws InventoryCatalogException when the item has never been owned at all (no
        // OwnedItemEntry row), so pre-check rather than relying on TryDebitAsync alone.
        var orbOwned = viewer.Items.FirstOrDefault(i => i.Item.Id == OrbItemId);
        if (orbOwned is null || orbOwned.Count < request.CreateNumber)
            return BadRequest(new { error = "insufficient_orb" });
        var orbSpend = await tx.TryDebitAsync(UserGoodsType.Item, OrbItemId, request.CreateNumber, ct);
        if (!orbSpend.Success)
            return BadRequest(new { error = "insufficient_orb" });

        // Debit base copies directly (InventoryTransaction.TryDebitAsync doesn't route Card).
        baseOwned.Count -= request.CreateNumber;

        // Grant foil copies via the canonical primitive so CardCosmeticReward cascades fire.
        await tx.GrantAsync(UserGoodsType.Card, foilCard.Id, request.CreateNumber, ct);

        var result = await tx.CommitAsync(ct);

        // Append the base card's post-state total so the client can update its on-screen count
        // (UpdateHaveUserGoodsNumByJsonData does direct assignment, not delta).
        var rewardList = result.RewardList.ToRewardList();
        rewardList.Add(new RewardListEntry
        {
            RewardType = (int)UserGoodsType.Card,
            RewardId = baseCard.Id,
            RewardNum = baseOwned.Count,
        });

        return new CardCreateFoilCardResponse { RewardList = rewardList };
    }

    [HttpPost("protect")]
    public async Task<ActionResult<CardProtectResponse>> Protect(CardProtectRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var outcome = await _inventory.SetProtected(viewerId, request.CardId, request.IsProtected);
        if (!outcome.IsSuccess)
        {
            return outcome.Error switch
            {
                ProtectError.UnknownCard => BadRequest(new { error = "unknown_card" }),
                _                        => BadRequest(new { error = "malformed_request" }),
            };
        }

        return new CardProtectResponse();
    }

    private static string ErrorKey(DestructError error) => error switch
    {
        DestructError.UnknownCard       => "unknown_card",
        DestructError.NotDestructible   => "not_destructible",
        DestructError.CardProtected     => "card_protected",
        DestructError.InsufficientCards => "insufficient_cards",
        _ => "malformed_request",
    };

    /// <summary>
    /// Decodes the inner JSON of <c>card_id_number_array</c>. Values are
    /// <c>"&lt;num_to_destruct&gt;,&lt;client_possession_snapshot&gt;"</c> — both strings.
    /// Returns false (and sets <paramref name="errorKey"/>) on any structural problem.
    /// </summary>
    private static bool TryParseCardCountDict(
        string raw,
        out Dictionary<long, int> counts,
        out Dictionary<long, int> clientSnapshots,
        out string errorKey)
    {
        counts = new();
        clientSnapshots = new();
        errorKey = "malformed_request";

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        JsonDocument? doc;
        try { doc = JsonDocument.Parse(raw); }
        catch (JsonException) { return false; }

        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return false;

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (!long.TryParse(prop.Name, out long cardId))
                    return false;
                if (prop.Value.ValueKind != JsonValueKind.String)
                    return false;

                var pair = prop.Value.GetString()!.Split(',');
                if (pair.Length != 2)
                    return false;
                if (!int.TryParse(pair[0], out int num) || num <= 0)
                    return false;
                if (!int.TryParse(pair[1], out int snapshot) || snapshot < 0)
                    return false;

                counts[cardId] = num;
                clientSnapshots[cardId] = snapshot;
            }
        }

        return true;
    }
}
