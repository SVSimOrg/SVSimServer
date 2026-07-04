using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Globals;
using SVSim.EmulatedEntrypoint.Configuration;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Builds the shared <see cref="DeckListResponse"/> consumed by the client's
/// <c>DeckGroupListData(jsonData, format)</c>. Used by <c>/deck/info</c>, <c>/deck/my_list</c>,
/// and <c>/practice/deck_list</c> — all three return the same wire shape (default decks +
/// per-class leader-skin settings + the viewer's decks).
///
/// <para><paramref name="padEmptySlots"/> distinguishes the deck *builder* screens
/// (<c>/deck/*</c>, which need empty "New Deck" tiles up to the slot cap) from the deck *select*
/// screens (<c>/practice/deck_list</c>, where prod returns the real decks unpadded — confirmed by
/// the 2026-05-29 practice capture returning empty user-deck arrays for a fresh account).</para>
/// </summary>
public interface IDeckListBuilder
{
    Task<DeckListResponse> BuildAsync(long viewerId, Format requestFormat, bool padEmptySlots);

    /// <summary>
    /// Pads a viewer's real deck list with empty-slot placeholders up to the slot cap. Exposed for
    /// deck-builder endpoints (e.g. <c>/deck/update</c>) that return a deck list directly rather
    /// than through <see cref="BuildAsync"/>.
    /// </summary>
    List<UserDeck> PadEmptySlots(List<UserDeck> realDecks);
}

public class DeckListBuilder : IDeckListBuilder
{
    private readonly IDeckRepository _deckRepository;
    private readonly IGlobalsRepository _globalsRepository;
    private readonly SVSimDbContext _dbContext;
    private readonly DeckOptions _deckOptions;

    private static readonly System.Text.Json.JsonSerializerOptions JsonbReadOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
    };

    public DeckListBuilder(
        IDeckRepository deckRepository,
        IGlobalsRepository globalsRepository,
        SVSimDbContext dbContext,
        IOptions<DeckOptions> deckOptions)
    {
        _deckRepository = deckRepository;
        _globalsRepository = globalsRepository;
        _dbContext = dbContext;
        _deckOptions = deckOptions.Value;
    }

    public async Task<DeckListResponse> BuildAsync(long viewerId, Format requestFormat, bool padEmptySlots)
    {
        var defaultDecks = await _globalsRepository.GetDefaultDecks();

        // user_leader_skin_setting_list is PER-VIEWER (the wire `user_` prefix is honest, despite
        // the misleading docstring on DefaultLeaderSkinSetting). Source it from the viewer's
        // ViewerClassData rows, matching how /load/index's user_class_list reads them. The global
        // DefaultLeaderSkinSettings table is now used only as initial seed values for fresh
        // viewers (ViewerRepository.RegisterViewer); the per-class current skin is on
        // viewer.Classes[i].LeaderSkin and gets mutated by /leader_skin/update.
        var viewerClasses = await _dbContext.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Classes)
            .Select(c => new { c.Class.Id, LeaderSkinId = c.LeaderSkin.Id })
            .ToListAsync();

        var response = new DeckListResponse
        {
            DefaultDeckList = defaultDecks.ToDictionary(
                d => d.Id.ToString(),
                d => new DefaultDeck
                {
                    DeckNo = d.DeckNo,
                    ClassId = d.ClassId,
                    SleeveId = d.SleeveId,
                    LeaderSkinId = d.LeaderSkinId,
                    DeckName = d.DeckName,
                    CardIdArray = System.Text.Json.JsonSerializer.Deserialize<List<long>>(d.CardIdArray, JsonbReadOptions) ?? new(),
                    // TODO(deck-stub): wire from real per-deck state once user maintenance / availability tracking lands.
                    // Prod emits is_complete_deck=1, is_available_deck=1, maintenance_card_ids=[] for the 8 starter decks.
                    IsCompleteDeck = 1,
                    IsAvailableDeck = 1,
                    MaintenanceCardIds = new(),
                }),
            UserLeaderSkinSettingList = viewerClasses.ToDictionary(
                vc => vc.Id.ToString(),
                vc => new UserLeaderSkinSetting
                {
                    ClassId = vc.Id,
                    IsRandomLeaderSkin = 0,   // random-skin mode (per-class shuffle pool) not yet persisted
                    LeaderSkinId = vc.LeaderSkinId,
                }),
            MaintenanceCardList = new(), // sourced from same place as /load/index when wired
        };

        if (requestFormat == Format.All)
        {
            // Prod's All-format response emits these three per-format lists (each [] for fresh viewers).
            // The PreRotation / Crossover / Avatar siblings exist in client code but prod omits them
            // for our profile; we mirror that omission and leave the nullable DTO fields unset.
            var formats = new[] { Format.Rotation, Format.Unlimited, Format.MyRotation };
            var byFormat = await _deckRepository.GetDecksByFormats(viewerId, formats);
            response.UserDeckRotation = MaybePad(byFormat[Format.Rotation].Select(d => new UserDeck(d)).ToList(), padEmptySlots);
            response.UserDeckUnlimited = MaybePad(byFormat[Format.Unlimited].Select(d => new UserDeck(d)).ToList(), padEmptySlots);
            response.UserDeckMyRotation = MaybePad(byFormat[Format.MyRotation].Select(d => new UserDeck(d)).ToList(), padEmptySlots);
            // trial_deck_list is prod-emitted on /deck/info (All format) but omitted on /deck/my_list
            // (specific format). Empty array in the 2026-05-23 prod capture.
            response.TrialDeckList = new();
        }
        else
        {
            var decks = await _deckRepository.GetDecks(viewerId, requestFormat);
            response.UserDeckList = MaybePad(decks.Select(d => new UserDeck(d)).ToList(), padEmptySlots);
        }

        return response;
    }

    private List<UserDeck> MaybePad(List<UserDeck> realDecks, bool pad) => pad ? PadEmptySlots(realDecks) : realDecks;

    /// <summary>
    /// Pads a viewer's real deck list with empty-slot placeholders up to <see cref="DeckOptions.MaxDeckSlots"/>.
    /// Required on the deck *builder* screens because the client's
    /// <c>DeckUI.DeckViewData.CreateDeckViewList</c> only renders a "New Deck" tile when the response
    /// contains an entry whose <c>card_id_array</c> is empty — without padding, the player cannot
    /// create additional decks once any exist. Deck *select* screens (practice) skip padding: prod
    /// returns the real decks unpadded there.
    /// </summary>
    public List<UserDeck> PadEmptySlots(List<UserDeck> realDecks)
    {
        var taken = realDecks.Select(d => d.DeckNumber).ToHashSet();
        var result = new List<UserDeck>(realDecks);
        for (int slot = 1; slot <= _deckOptions.MaxDeckSlots; slot++)
        {
            if (!taken.Contains(slot))
            {
                result.Add(UserDeck.CreateEmptySlot(slot));
            }
        }
        return result;
    }
}
