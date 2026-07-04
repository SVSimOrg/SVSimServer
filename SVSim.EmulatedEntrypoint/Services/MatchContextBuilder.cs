using System.Text.Json;
using SVSim.BattleNode.Bridge;
using SVSim.Database.Enums;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

public class MatchContextBuilder : IMatchContextBuilder
{
    private readonly IArenaTwoPickRunRepository _runs;
    private readonly IArenaColosseumRunRepository _colosseumRuns;
    private readonly IViewerRepository _viewers;
    private readonly IDeckRepository _decks;
    private readonly IGameConfigService _config;

    public MatchContextBuilder(
        IArenaTwoPickRunRepository runs,
        IArenaColosseumRunRepository colosseumRuns,
        IViewerRepository viewers,
        IDeckRepository decks,
        IGameConfigService config)
    {
        _runs = runs;
        _colosseumRuns = colosseumRuns;
        _viewers = viewers;
        _decks = decks;
        _config = config;
    }

    public async Task<MatchContext> BuildForTwoPickAsync(long viewerId)
    {
        var run = await _runs.GetByViewerIdAsync(viewerId)
            ?? throw new ArenaTwoPickException("arena_two_pick_no_active_run");

        var deck = JsonSerializer.Deserialize<List<long>>(run.SelectedCardIdsJson) ?? new();
        if (deck.Count < 30)
            throw new ArenaTwoPickException("arena_two_pick_draft_incomplete");

        var viewer = await _viewers.LoadForMatchContextAsync(viewerId)
            ?? throw new ArenaTwoPickException("arena_two_pick_no_active_run");

        var defaults = _config.Get<DefaultLoadoutConfig>();

        var emblemId = viewer.Info.SelectedEmblem.Id != 0
            ? viewer.Info.SelectedEmblem.Id.ToString()
            : defaults.EmblemId.ToString();
        var degreeId = viewer.Info.SelectedDegree.Id != 0
            ? viewer.Info.SelectedDegree.Id.ToString()
            : defaults.DegreeId.ToString();
        var charaId = run.LeaderSkinId != 0
            ? run.LeaderSkinId.ToString()
            : run.ClassId.ToString();
        // TK2-specific cosmetic source; falls back to the global default sleeve if the
        // viewer hasn't set a challenge-specific one via /config/update_challenge_config.
        var twoPickSleeveId = viewer.Info.ChallengeTwoPickSleeveId != 0
            ? viewer.Info.ChallengeTwoPickSleeveId
            : defaults.SleeveId;

        return new MatchContext(
            SelfDeckCardIds: deck,
            ClassId: (CardClass)run.ClassId,
            CharaId: charaId,
            // Hardcoded v1; see spec §Deferred plumbing.
            CardMasterName: "card_master_node_10015",
            CountryCode: viewer.Info.CountryCode ?? string.Empty,
            UserName: viewer.DisplayName,
            // TK2-specific cosmetic source; other modes will use the deck row's SleeveId.
            SleeveId: twoPickSleeveId.ToString(),
            EmblemId: emblemId,
            DegreeId: degreeId,
            // Hardcoded v1; needs equipped-MyPageBackground lookup (see spec §Deferred).
            FieldId: 43,
            IsOfficial: viewer.Info.IsOfficial ? 1 : 0,
            BattleModeId: BattleModes.TakeTwo);
    }

    public async Task<MatchContext> BuildForRankBattleAsync(long viewerId, Format format, int deckNo)
    {
        var viewer = await _viewers.LoadForMatchContextAsync(viewerId)
            ?? throw new InvalidOperationException($"viewer {viewerId} not found");

        // IDeckRepository is the right path here — viewer-graph nav refs (DeckCard.Card)
        // don't auto-load (see project_ef_nav_include_pitfall memory), which would
        // silently ship card_id=0.
        var deck = await _decks.GetDeck(viewerId, format, deckNo)
            ?? throw new InvalidOperationException(
                $"viewer {viewerId} has no deck #{deckNo} for format {format}");

        var defaults = _config.Get<DefaultLoadoutConfig>();
        var emblemId = viewer.Info.SelectedEmblem.Id != 0
            ? viewer.Info.SelectedEmblem.Id.ToString()
            : defaults.EmblemId.ToString();
        var degreeId = viewer.Info.SelectedDegree.Id != 0
            ? viewer.Info.SelectedDegree.Id.ToString()
            : defaults.DegreeId.ToString();
        var charaId = deck.LeaderSkin.Id != 0
            ? deck.LeaderSkin.Id.ToString()
            : deck.Class.Id.ToString();
        var sleeveId = deck.Sleeve.Id != 0
            ? deck.Sleeve.Id.ToString()
            : defaults.SleeveId.ToString();
        // DeckCard is count-based (one row per unique card + a Count). The node's deck
        // is one entry PER PHYSICAL CARD (idx 1..N), so expand each row by its Count —
        // otherwise a 3-copy card ships as a single in-battle card.
        var deckCardIds = deck.Cards
            .SelectMany(c => Enumerable.Repeat(c.Card.Id, c.Count))
            .ToList();

        return new MatchContext(
            SelfDeckCardIds: deckCardIds,
            ClassId: (CardClass)deck.Class.Id,
            CharaId: charaId,
            CardMasterName: "card_master_node_10015",
            CountryCode: viewer.Info.CountryCode ?? string.Empty,
            UserName: viewer.DisplayName,
            SleeveId: sleeveId,
            EmblemId: emblemId,
            DegreeId: degreeId,
            FieldId: 43,
            IsOfficial: viewer.Info.IsOfficial ? 1 : 0,
            BattleModeId: BattleModes.TakeTwo);
    }

    public async Task<MatchContext> BuildForColosseumAsync(long viewerId)
    {
        var run = await _colosseumRuns.GetByViewerIdAsync(viewerId)
            ?? throw new InvalidOperationException("arena_colosseum_no_active_run");

        // v1 single-deck slot — Round-3 multi-deck format is deferred per plan.
        var deckNos = JsonSerializer.Deserialize<List<int>>(run.RegisteredDeckNoListJson) ?? new();
        if (deckNos.Count == 0)
        {
            throw new InvalidOperationException("arena_colosseum_no_deck_registered");
        }
        var deckNo = deckNos[0];

        var viewer = await _viewers.LoadForMatchContextAsync(viewerId)
            ?? throw new InvalidOperationException($"viewer {viewerId} not found");

        var deck = await _decks.GetDeck(viewerId, run.DeckFormat, deckNo)
            ?? throw new InvalidOperationException(
                $"viewer {viewerId} has no deck #{deckNo} for format {run.DeckFormat}");

        var defaults = _config.Get<DefaultLoadoutConfig>();
        var emblemId = viewer.Info.SelectedEmblem.Id != 0
            ? viewer.Info.SelectedEmblem.Id.ToString()
            : defaults.EmblemId.ToString();
        var degreeId = viewer.Info.SelectedDegree.Id != 0
            ? viewer.Info.SelectedDegree.Id.ToString()
            : defaults.DegreeId.ToString();
        var charaId = run.LeaderSkinId != 0
            ? run.LeaderSkinId.ToString()
            : deck.LeaderSkin.Id != 0
                ? deck.LeaderSkin.Id.ToString()
                : deck.Class.Id.ToString();
        var sleeveId = deck.Sleeve.Id != 0
            ? deck.Sleeve.Id.ToString()
            : defaults.SleeveId.ToString();
        var deckCardIds = deck.Cards
            .SelectMany(c => Enumerable.Repeat(c.Card.Id, c.Count))
            .ToList();

        return new MatchContext(
            SelfDeckCardIds: deckCardIds,
            ClassId: (CardClass)deck.Class.Id,
            CharaId: charaId,
            CardMasterName: "card_master_node_10015",
            CountryCode: viewer.Info.CountryCode ?? string.Empty,
            UserName: viewer.DisplayName,
            SleeveId: sleeveId,
            EmblemId: emblemId,
            DegreeId: degreeId,
            FieldId: 43,
            IsOfficial: viewer.Info.IsOfficial ? 1 : 0,
            BattleModeId: BattleModes.TakeTwo);
    }
}
