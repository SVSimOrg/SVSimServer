using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Globals;
using SVSim.EmulatedEntrypoint.Configuration;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

public class DeckController : SVSimController
{
    private readonly IDeckRepository _deckRepository;
    private readonly SVSimDbContext _dbContext;
    private readonly IDeckListBuilder _deckListBuilder;

    public DeckController(IDeckRepository deckRepository, SVSimDbContext dbContext, IDeckListBuilder deckListBuilder)
    {
        _deckRepository = deckRepository;
        _dbContext = dbContext;
        _deckListBuilder = deckListBuilder;
    }

    // Request deck_format fields arrive as wire ints (MessagePack-CSharp doesn't honor STJ
    // converters on request DTOs, so request DTO properties stay typed as int). Route through
    // FromApi here so controllers always work in internal Format space when comparing /
    // persisting.
    private static Format AsFormat(int apiValue) => FormatExtensions.FromApi(apiValue);

    [HttpPost("info")]
    public async Task<ActionResult<DeckListResponse>> Info(DeckInfoRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        // Deck builder screen: pad empty "New Deck" slots so the player can create more decks.
        return await _deckListBuilder.BuildAsync(viewerId, AsFormat(request.DeckFormat), padEmptySlots: true);
    }

    [HttpPost("my_list")]
    public async Task<ActionResult<DeckListResponse>> MyList(DeckFormatRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        return await _deckListBuilder.BuildAsync(viewerId, AsFormat(request.DeckFormat), padEmptySlots: true);
    }

    [HttpPost("get_empty_deck_number")]
    public async Task<ActionResult<EmptyDeckNumberResponse>> GetEmptyDeckNumber(DeckFormatRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        return new EmptyDeckNumberResponse
        {
            EmptyDeckNum = await _deckRepository.GetEmptyDeckNumber(viewerId, AsFormat(request.DeckFormat))
        };
    }

    [HttpPost("update")]
    public async Task<ActionResult<DeckUpdateResponse>> Update(DeckUpdateRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var format = AsFormat(request.DeckFormat);

        if (request.IsDelete == 1)
        {
            await _deckRepository.DeleteDecks(viewerId, format, new[] { request.DeckNo });
        }
        else
        {
            var cls = await _dbContext.Classes.FindAsync(request.ClassId);
            var sleeve = await _dbContext.Sleeves.FindAsync((int)request.SleeveId);
            var skin = await _dbContext.LeaderSkins.FindAsync(request.LeaderSkinId);
            var cards = await ResolveDeckCards(request.CardIdArray);

            await _deckRepository.UpsertDeck(viewerId, format, request.DeckNo, deck =>
            {
                deck.Name = request.DeckName ?? string.Empty;
                if (cls is not null) deck.Class = cls;
                if (sleeve is not null) deck.Sleeve = sleeve;
                if (skin is not null) deck.LeaderSkin = skin;
                deck.RandomLeaderSkin = request.IsRandomLeaderSkin;
                deck.Cards = cards;
                // Clear stale rotation_id if the deck moved to a non-MyRotation format;
                // otherwise persist the chosen period so it survives the next /load/index.
                deck.MyRotationId = format == Format.MyRotation ? request.RotationId : null;
            });
        }

        var decks = await _deckRepository.GetDecks(viewerId, format);
        return new DeckUpdateResponse
        {
            UserDeckList = _deckListBuilder.PadEmptySlots(decks.Select(d => new UserDeck(d)).ToList())
        };
    }

    [HttpPost("update_name")]
    public async Task<ActionResult<SingleDeckResponse>> UpdateName(DeckUpdateNameRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var deck = await _deckRepository.UpsertDeck(viewerId, AsFormat(request.DeckFormat), request.DeckNo,
            d => d.Name = request.DeckName ?? string.Empty);
        return new SingleDeckResponse { UserDeck = new UserDeck(deck) };
    }

    [HttpPost("update_sleeve")]
    public async Task<ActionResult<SingleDeckResponse>> UpdateSleeve(DeckUpdateSleeveRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var sleeve = await _dbContext.Sleeves.FindAsync((int)request.SleeveId);
        if (sleeve is null) return BadRequest($"Unknown sleeve {request.SleeveId}");

        var deck = await _deckRepository.UpsertDeck(viewerId, AsFormat(request.DeckFormat), request.DeckNo,
            d => d.Sleeve = sleeve);
        return new SingleDeckResponse { UserDeck = new UserDeck(deck) };
    }

    [HttpPost("update_leader_skin")]
    public async Task<ActionResult<SingleDeckResponse>> UpdateLeaderSkin(DeckUpdateLeaderSkinRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var skin = await _dbContext.LeaderSkins.FindAsync(request.LeaderSkinId);
        if (skin is null) return BadRequest($"Unknown leader skin {request.LeaderSkinId}");

        var deck = await _deckRepository.UpsertDeck(viewerId, AsFormat(request.DeckFormat), request.DeckNo,
            d =>
            {
                d.LeaderSkin = skin;
                d.RandomLeaderSkin = false;
            });
        return new SingleDeckResponse { UserDeck = new UserDeck(deck) };
    }

    // TODO: schema doesn't yet model the random-leader-skin pool — we just pick one and persist
    // that. Add a join table (DeckLeaderSkinPool) when ranked play / random skins become a real
    // feature. For now the UI flow still works (server returns a single chosen skin per spec).
    [HttpPost("update_random_leader_skin")]
    public async Task<ActionResult<SingleDeckResponse>> UpdateRandomLeaderSkin(DeckUpdateRandomLeaderSkinRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var pool = request.LeaderSkinIdList ?? new List<int>();
        if (pool.Count == 0) return BadRequest("leader_skin_id_list must contain at least one id");

        int chosenId = pool[Random.Shared.Next(pool.Count)];
        var skin = await _dbContext.LeaderSkins.FindAsync(chosenId);
        if (skin is null) return BadRequest($"Unknown leader skin {chosenId}");

        var deck = await _deckRepository.UpsertDeck(viewerId, AsFormat(request.DeckFormat), request.DeckNo,
            d =>
            {
                d.LeaderSkin = skin;
                d.RandomLeaderSkin = true;
            });
        return new SingleDeckResponse { UserDeck = new UserDeck(deck) };
    }

    [HttpPost("update_order")]
    public async Task<ActionResult<EmptyResponse>> UpdateOrder(DeckOrderRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        // Deck `Number` IS the slot order — the client sends the same slot numbers in a new
        // sequence. Today we don't model "display order" separately from "slot number", so
        // reordering is a no-op server-side. When a separate Order column lands, persist here.
        return new EmptyResponse();
    }

    [HttpPost("delete_deck_list")]
    public async Task<ActionResult<EmptyResponse>> DeleteDeckList(DeckDeleteListRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var nos = request.DeckNoList ?? new List<int>();
        if (nos.Count > 0) await _deckRepository.DeleteDecks(viewerId, AsFormat(request.DeckFormat), nos);
        return new EmptyResponse();
    }

    // /deck/set_deck_redis — server side is a Redis-cached "active deck per class" hint for
    // matchmaking. We don't model matchmaking yet; acknowledge the call and move on (real
    // server may not persist this either; the `_redis` suffix suggests cache-only).
    [HttpPost("set_deck_redis")]
    public Task<ActionResult<EmptyResponse>> SetDeckRedis(SetDeckRedisRequest request)
    {
        if (!TryGetViewerId(out long _)) return Task.FromResult<ActionResult<EmptyResponse>>(Unauthorized());
        return Task.FromResult<ActionResult<EmptyResponse>>(new EmptyResponse());
    }

    /// <summary>
    /// Convert a flat `card_id_array` (cards repeated for count) into a grouped DeckCard list.
    /// Cards not in the DB are silently dropped — until CardImport lands the result is always
    /// empty, which is acceptable for the deck-editing flow (UI saves what it can).
    /// </summary>
    private async Task<List<DeckCard>> ResolveDeckCards(List<long>? cardIdArray)
    {
        if (cardIdArray is null || cardIdArray.Count == 0) return new List<DeckCard>();

        var grouped = cardIdArray.GroupBy(id => id).Select(g => new { Id = g.Key, Count = g.Count() }).ToList();
        var ids = grouped.Select(g => g.Id).ToList();
        var cards = await _dbContext.Cards.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

        return grouped
            .Where(g => cards.ContainsKey(g.Id))
            .Select(g => new DeckCard { Card = cards[g.Id], Count = g.Count })
            .ToList();
    }
}
