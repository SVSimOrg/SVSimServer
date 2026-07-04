using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Sessions.Dispatch;

namespace SVSim.UnitTests.BattleNode.Sessions;

[TestFixture]
public class KnownListBuilderTests
{
    // orderList as it arrives in a RawBody: a list of single-key op dicts.
    private static List<object?> OrderListMove(int idx, int from, int to) => new()
    {
        new Dictionary<string, object?>
        {
            ["move"] = new Dictionary<string, object?>
            {
                ["idx"] = new List<object?> { (long)idx },
                ["isSelf"] = 1L, ["from"] = (long)from, ["to"] = (long)to,
            }
        }
    };

    [Test]
    public void ExtractMoveTo_returns_to_for_matching_idx()
    {
        var to = KnownListBuilder.ExtractMoveTo(OrderListMove(3, 10, 20), playIdx: 3);
        Assert.That(to, Is.EqualTo(20));
    }

    [Test]
    public void ExtractMoveTo_returns_null_when_no_move_op_matches()
    {
        Assert.That(KnownListBuilder.ExtractMoveTo(OrderListMove(3, 10, 20), playIdx: 99), Is.Null);
        Assert.That(KnownListBuilder.ExtractMoveTo(null, playIdx: 3), Is.Null);
    }

    [Test]
    public void ExtractMoveTo_returns_first_matching_move_op()
    {
        // A real PlayActions can carry several move ops; the played card's move comes first,
        // later ops (token add/alter) target other idxs. Confirm first-match-wins, not last.
        var orderList = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["move"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L,
                }
            },
            new Dictionary<string, object?>
            {
                ["move"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { 31L, 32L }, ["isSelf"] = 1L, ["from"] = 0L, ["to"] = 40L,
                }
            },
        };
        Assert.That(KnownListBuilder.ExtractMoveTo(orderList, playIdx: 3), Is.EqualTo(30));
        Assert.That(KnownListBuilder.ExtractMoveTo(orderList, playIdx: 31), Is.EqualTo(40));
    }

    [Test]
    public void BuildPlayedCard_returns_null_for_card_with_no_matching_move_op()
    {
        // A resolved cardId, but the orderList has no move op for the played idx → can't synthesize.
        var entry = KnownListBuilder.BuildPlayedCard(playIdx: 3, cardId: 128821011L, orderList: OrderListMove(7, 10, 20));
        Assert.That(entry, Is.Null);
    }

    [Test]
    public void BuildPlayedCard_synthesizes_entry_from_engine_sourced_cardId()
    {
        // M-HC-4f: the handler resolves the cardId engine-first (PlayedCardId, deck-map/mined fallback) and passes
        // it in; BuildPlayedCard lands it on the entry verbatim.
        var entry = KnownListBuilder.BuildPlayedCard(playIdx: 3, cardId: 128821011L, orderList: OrderListMove(3, 10, 20));

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Idx, Is.EqualTo(3));
        Assert.That(entry.CardId, Is.EqualTo(128821011L));
        Assert.That(entry.To, Is.EqualTo(20));
        Assert.That(entry.Spellboost, Is.EqualTo(0));
        Assert.That(entry.AttachTarget, Is.EqualTo(""));
        Assert.That(entry.Cost, Is.EqualTo(0), "cost defaults to 0 when the caller passes none");
    }

    [Test]
    public void BuildPlayedCard_emits_engine_resolved_cost_passed_by_caller()
    {
        // M-HC-3a: the handler reads the engine-resolved play-time cost and passes it in; BuildPlayedCard
        // lands it on the entry verbatim. (A wrong cost yields a different field — non-vacuity.)
        var entry = KnownListBuilder.BuildPlayedCard(playIdx: 3, cardId: 101314020L, orderList: OrderListMove(3, 10, 20), cost: 3);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Cost, Is.EqualTo(3));
    }

    [Test]
    public void BuildPlayedCard_emits_engine_sourced_spellboost_count()
    {
        // M-HC-3b: the handler reads the engine-resolved spell-charge count
        // (SessionBattleEngine.PlayedCardSpellboost) and passes it in; BuildPlayedCard lands it on the
        // entry verbatim. (Default 0 vs a non-zero value is the non-vacuity.)
        var entry = KnownListBuilder.BuildPlayedCard(playIdx: 3, cardId: 101314020L, orderList: OrderListMove(3, 10, 20), cost: 3, spellboost: 2);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Spellboost, Is.EqualTo(2));
    }

    [Test]
    public void BuildPlayedCard_returns_null_for_zero_cardId()
    {
        // M-HC-4f: cardId 0 means the engine resolved no id AND the deck-map/mined fallback had no entry for the
        // idx → un-synthesizable identity → null (the play degrades to {playIdx,type}, no knownList).
        var entry = KnownListBuilder.BuildPlayedCard(playIdx: 31, cardId: 0L, orderList: OrderListMove(31, 10, 20));
        Assert.That(entry, Is.Null);
    }

    [Test]
    public void BuildPlayedCard_defaults_spellboost_to_zero_when_caller_passes_none()
    {
        // A vanilla play emits spellboost 0 (the engine resolves no spell-charge for a non-boosted card,
        // so the handler's PlayedCardSpellboost read is 0 and the param defaults to 0).
        Assert.That(KnownListBuilder.BuildPlayedCard(playIdx: 3, cardId: 101311010L, orderList: OrderListMove(3, 10, 20))!.Spellboost, Is.EqualTo(0));
    }

    [Test]
    public void BuildPlayedCard_emits_clan_tribe_passed_by_caller()
    {
        // M-HC-4e: the handler reads the engine-resolved clan/tribe
        // (SessionBattleEngine.PlayedCardClan / PlayedCardTribe) and passes them in; BuildPlayedCard lands
        // them on the entry verbatim. (A wrong clan/tribe yields a different field — non-vacuity.)
        var entry = KnownListBuilder.BuildPlayedCard(
            playIdx: 3, cardId: 101314020L, orderList: OrderListMove(3, 10, 20), cost: 3, spellboost: 2, clan: 8, tribe: "7,16");
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Clan, Is.EqualTo(8));
        Assert.That(entry.Tribe, Is.EqualTo("7,16"));
    }

    [Test]
    public void BuildPlayedCard_defaults_clan_to_zero_and_tribe_to_string_zero_when_caller_passes_none()
    {
        // A play whose engine read degraded (Setup failed and the ComputeFrames try/catch swallowed it →
        // _mgr null → the accessor fallback) emits clan 0 (ClanType.ALL ordinal) and tribe "0" (the prod
        // no-tribe form, NEVER empty — empty is wire-illegal). The param defaults match the accessor fallbacks.
        var entry = KnownListBuilder.BuildPlayedCard(playIdx: 3, cardId: 101311010L, orderList: OrderListMove(3, 10, 20));
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Clan, Is.EqualTo(0));
        Assert.That(entry.Tribe, Is.EqualTo("0"));
    }

    [Test]
    public void RenameTargets_passes_isSelf_through_verbatim()
    {
        var targetList = new List<object?>
        {
            new Dictionary<string, object?> { ["targetIdx"] = 8L, ["isSelf"] = 0L },
        };
        var renamed = KnownListBuilder.RenameTargets(targetList);

        Assert.That(renamed, Is.Not.Null);
        Assert.That(renamed!.Count, Is.EqualTo(1));
        Assert.That(renamed[0].TargetIdx, Is.EqualTo(8));
        Assert.That(renamed[0].IsSelf, Is.EqualTo(CardOwner.Opponent));
    }

    [Test]
    public void RenameTargets_returns_null_for_missing_or_empty()
    {
        Assert.That(KnownListBuilder.RenameTargets(null), Is.Null);
        Assert.That(KnownListBuilder.RenameTargets(new List<object?>()), Is.Null);
    }

    // An add op as it arrives in a RawBody: { "add": { "idx": [..], "isSelf": n, "card": { "cardId": n } } }
    private static Dictionary<string, object?> AddOp(long[] idxs, long cardId, long isSelf = 1) => new()
    {
        ["add"] = new Dictionary<string, object?>
        {
            ["idx"] = idxs.Select(i => (object?)i).ToList(),
            ["isSelf"] = isSelf,
            ["card"] = new Dictionary<string, object?> { ["cardId"] = cardId },
        }
    };

    [Test]
    public void MineAddOps_yields_idx_to_cardId_for_every_idx_in_an_add_op()
    {
        var orderList = new List<object?> { AddOp(new[] { 31L, 32L }, 900111010L) };
        var mined = KnownListBuilder.MineAddOps(orderList).ToList();

        Assert.That(mined, Is.EquivalentTo(new[] { new MinedToken(31, 900111010L, CardOwner.Self), new MinedToken(32, 900111010L, CardOwner.Self) }));
    }

    [Test]
    public void MineAddOps_yields_cross_side_gifts_with_isSelf_0()
    {
        // A card gifted to the opponent (isSelf:0) is the opponent's card at this idx (isSelf is the
        // sender's perspective tag on CardObj.IsPlayer — RegisterToken.cs:22). The extractor surfaces
        // it; the caller routes it into the OTHER side's map.
        var orderList = new List<object?> { AddOp(new[] { 31L }, 900111010L, isSelf: 0) };
        Assert.That(KnownListBuilder.MineAddOps(orderList),
            Is.EquivalentTo(new[] { new MinedToken(31, 900111010L, CardOwner.Opponent) }));
    }

    [Test]
    public void MineAddOps_skips_choice_adds_with_no_concrete_cardId()
    {
        // { "add": { "idx":[46], "card": { "candidates":[...] }, "isChoice":"1" } } — identity undetermined.
        var orderList = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["add"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { 46L },
                    ["isSelf"] = 1L,
                    ["card"] = new Dictionary<string, object?>
                    {
                        ["candidates"] = new List<object?> { 810041260L, 101041020L },
                    },
                    ["isChoice"] = "1",
                }
            }
        };
        Assert.That(KnownListBuilder.MineAddOps(orderList), Is.Empty);
    }

    [Test]
    public void MineAddOps_skips_copy_token_adds_with_baseIdx_and_no_cardId()
    {
        // RegisterCopyToken.MakeCardData → { "baseIdx": N, "isPremium": 0 } — no cardId, deferred.
        var orderList = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["add"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { 33L },
                    ["isSelf"] = 1L,
                    ["card"] = new Dictionary<string, object?> { ["baseIdx"] = 12L, ["isPremium"] = 0L },
                }
            }
        };
        Assert.That(KnownListBuilder.MineAddOps(orderList), Is.Empty);
    }

    [Test]
    public void MineAddOps_ignores_non_add_ops_and_null()
    {
        Assert.That(KnownListBuilder.MineAddOps(OrderListMove(3, 10, 20)), Is.Empty);
        Assert.That(KnownListBuilder.MineAddOps(null), Is.Empty);
    }

    [Test]
    public void MineAddOps_yields_from_multiple_add_ops_in_one_orderList()
    {
        var orderList = new List<object?>
        {
            new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
            AddOp(new[] { 31L }, 900111010L),
            AddOp(new[] { 32L }, 900811090L),
        };
        var mined = KnownListBuilder.MineAddOps(orderList).ToList();
        Assert.That(mined, Is.EquivalentTo(new[] { new MinedToken(31, 900111010L, CardOwner.Self), new MinedToken(32, 900811090L, CardOwner.Self) }));
    }

    // A choice/Discover add op as it arrives in a RawBody: candidates-only (no concrete cardId —
    // RegisterChoiceAdd strips it), with isChoice present. Capture battle-traffic_tk2_regular line 152.
    private static Dictionary<string, object?> ChoiceAddOp(long idx, long[] candidates, long isSelf = 1) => new()
    {
        ["add"] = new Dictionary<string, object?>
        {
            ["idx"] = new List<object?> { idx },
            ["isSelf"] = isSelf,
            ["card"] = new Dictionary<string, object?>
            {
                ["candidates"] = candidates.Select(c => (object?)c).ToList(),
            },
            ["isChoice"] = "1",
        }
    };

    // A keyAction entry: { type, cardId (the GENERATING card), selectCard:{ cardId:[chosen...], open } }.
    private static List<object?> KeyActionChoice(long generatingCardId, long[] chosen, long open) => new()
    {
        new Dictionary<string, object?>
        {
            ["type"] = 1L,
            ["cardId"] = generatingCardId,
            ["selectCard"] = new Dictionary<string, object?>
            {
                ["cardId"] = chosen.Select(c => (object?)c).ToList(),
                ["open"] = open,
            },
        }
    };

    [Test]
    public void MineChoicePicks_resolves_idx_to_chosen_cardId_from_selectCard()
    {
        // The choiceAdd carries only candidates; the pick rides keyAction.selectCard.cardId. The node
        // joins them by candidate membership. Capture lines 151/152/193: chosen = candidates[0].
        var orderList = new List<object?> { ChoiceAddOp(46, new[] { 810041260L, 101041020L }) };
        var keyAction = KeyActionChoice(generatingCardId: 810014030L, chosen: new[] { 810041260L }, open: 0);

        Assert.That(KnownListBuilder.MineChoicePicks(orderList, keyAction),
            Is.EquivalentTo(new[] { new MinedToken(46, 810041260L, CardOwner.Self) }));
    }

    [Test]
    public void MineChoicePicks_routes_cross_side_choice_by_isSelf()
    {
        // A choiceAdd with isSelf:0 (a gifted choice in the opponent's index space) surfaces isSelf:0
        // so the caller routes it into the OTHER side's map (same rule as MineAddOps).
        var orderList = new List<object?> { ChoiceAddOp(46, new[] { 810041260L, 101041020L }, isSelf: 0) };
        var keyAction = KeyActionChoice(810014030L, new[] { 101041020L }, open: 0);

        Assert.That(KnownListBuilder.MineChoicePicks(orderList, keyAction),
            Is.EquivalentTo(new[] { new MinedToken(46, 101041020L, CardOwner.Opponent) }));
    }

    [Test]
    public void MineChoicePicks_yields_nothing_when_no_pick_matches_candidates()
    {
        var orderList = new List<object?> { ChoiceAddOp(46, new[] { 810041260L, 101041020L }) };
        var keyAction = KeyActionChoice(810014030L, new[] { 999999999L }, open: 0);

        Assert.That(KnownListBuilder.MineChoicePicks(orderList, keyAction), Is.Empty);
    }

    [Test]
    public void MineChoicePicks_ignores_non_choice_add_ops()
    {
        // A concrete-token add (cardId, no candidates) is MineAddOps' job — even if its cardId happens
        // to equal a selectCard pick, MineChoicePicks only mines isChoice/candidates adds.
        var orderList = new List<object?> { AddOp(new[] { 31L }, 900111010L) };
        var keyAction = KeyActionChoice(810014030L, new[] { 900111010L }, open: 0);

        Assert.That(KnownListBuilder.MineChoicePicks(orderList, keyAction), Is.Empty);
    }

    [Test]
    public void MineChoicePicks_yields_nothing_when_keyAction_absent()
    {
        // Echo carries orderList but no keyAction; choice mining keys on keyAction, so Echo yields
        // nothing here and stays mining-only via MineAddOps (§3.5).
        var orderList = new List<object?> { ChoiceAddOp(46, new[] { 810041260L, 101041020L }) };

        Assert.That(KnownListBuilder.MineChoicePicks(orderList, null), Is.Empty);
    }

    [Test]
    public void StripKeyActionForOpponent_drops_selectCard_when_open_0()
    {
        // Hidden draw-to-hand choice: opponent gets {type,cardId} only; the pick stays secret.
        // Capture line 151: keyAction:[{type:1, cardId:810014030}].
        var keyAction = KeyActionChoice(810014030L, new[] { 810041260L }, open: 0);
        var stripped = KnownListBuilder.StripKeyActionForOpponent(keyAction);

        Assert.That(stripped, Is.Not.Null);
        Assert.That(stripped!.Count, Is.EqualTo(1));
        Assert.That(stripped[0].Type, Is.EqualTo(KeyActionType.Choice));
        Assert.That(stripped[0].CardId, Is.EqualTo(810014030L));
        Assert.That(stripped[0].SelectCard, Is.Null);
    }

    [Test]
    public void StripKeyActionForOpponent_passes_selectCard_through_when_open_1()
    {
        // Visible board choice — provisional reveal-immediately behavior (§6, flagged for the live run).
        var keyAction = KeyActionChoice(810014030L, new[] { 810041260L }, open: 1);
        var stripped = KnownListBuilder.StripKeyActionForOpponent(keyAction);

        Assert.That(stripped![0].SelectCard, Is.Not.Null);
        Assert.That(stripped[0].SelectCard!.CardId, Is.EqualTo(new[] { 810041260L }));
        Assert.That(stripped[0].SelectCard.Open, Is.EqualTo(ChoiceVisibility.Open));
    }

    [Test]
    public void StripKeyActionForOpponent_drops_non_choice_types()
    {
        // Only Choice(1)/HaveBeforeSkillChoice(5) are handled; other KeyActionTypes are dropped
        // (current behavior) until their own specs (§6).
        var keyAction = new List<object?>
        {
            new Dictionary<string, object?> { ["type"] = 2L, ["cardId"] = 123L },
        };
        Assert.That(KnownListBuilder.StripKeyActionForOpponent(keyAction), Is.Null);
    }

    [Test]
    public void StripKeyActionForOpponent_returns_null_for_absent_keyAction()
    {
        Assert.That(KnownListBuilder.StripKeyActionForOpponent(null), Is.Null);
        Assert.That(KnownListBuilder.StripKeyActionForOpponent(new List<object?>()), Is.Null);
    }

    // A copy add op as it arrives in a RawBody: { "add": { "idx":[..], "isSelf":n, "card":{ "baseIdx":m, "isPremium":0 } } }
    private static Dictionary<string, object?> CopyOp(long[] idxs, long baseIdx, long isSelf = 1) => new()
    {
        ["add"] = new Dictionary<string, object?>
        {
            ["idx"] = idxs.Select(i => (object?)i).ToList(),
            ["isSelf"] = isSelf,
            ["card"] = new Dictionary<string, object?> { ["baseIdx"] = baseIdx, ["isPremium"] = 0L },
        }
    };

    [Test]
    public void MineCopyTokens_resolves_baseIdx_against_selfMap_for_isSelf_1()
    {
        var orderList = new List<object?> { CopyOp(new[] { 31L }, baseIdx: 5L, isSelf: 1) };
        var selfMap = new Dictionary<int, long> { [5] = 100_011_010L };
        var otherMap = new Dictionary<int, long>();
        var mined = KnownListBuilder.MineCopyTokens(orderList, selfMap, otherMap).ToList();
        Assert.That(mined, Is.EquivalentTo(new[] { new MinedToken(31, 100_011_010L, CardOwner.Self) }));
    }

    [Test]
    public void MineCopyTokens_resolves_baseIdx_against_otherMap_for_isSelf_0()
    {
        // Cross-side copy shape (battle-traffic_tk2_regular.ndjson:196 is an isSelf:0 Echo, baseIdx 21):
        // the source lives in the OPPONENT's index space, so resolve against otherMap and record there.
        var orderList = new List<object?> { CopyOp(new[] { 49L }, baseIdx: 21L, isSelf: 0) };
        var selfMap = new Dictionary<int, long>();
        var otherMap = new Dictionary<int, long> { [21] = 900_841_330L };
        var mined = KnownListBuilder.MineCopyTokens(orderList, selfMap, otherMap).ToList();
        Assert.That(mined, Is.EquivalentTo(new[] { new MinedToken(49, 900_841_330L, CardOwner.Opponent) }));
    }

    [Test]
    public void MineCopyTokens_skips_copy_when_baseIdx_absent_from_map()
    {
        // Unknown source (e.g. a card the node never recorded) → no record, no desync, the play degrades.
        var orderList = new List<object?> { CopyOp(new[] { 31L }, baseIdx: 99L, isSelf: 1) };
        Assert.That(
            KnownListBuilder.MineCopyTokens(orderList, new Dictionary<int, long>(), new Dictionary<int, long>()),
            Is.Empty);
    }

    [Test]
    public void MineCopyTokens_ignores_concrete_and_choice_adds()
    {
        // A concrete-cardId add is MineAddOps' job; a candidates add is MineChoicePicks' — both skipped here.
        var orderList = new List<object?>
        {
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                  ["card"] = new Dictionary<string, object?> { ["cardId"] = 900_111_010L } } },
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 32L }, ["isSelf"] = 1L,
                  ["card"] = new Dictionary<string, object?> { ["candidates"] = new List<object?> { 1L, 2L } },
                  ["isChoice"] = "1" } },
        };
        var map = new Dictionary<int, long> { [1] = 5L };
        Assert.That(KnownListBuilder.MineCopyTokens(orderList, map, map), Is.Empty);
    }

    [Test]
    public void MineCopyTokens_skips_string_baseIdx_private_group()
    {
        // PrivateGroupIndexMsg != "" makes baseIdx a STRING (RegisterCopyToken.cs:19-22) — the hidden
        // private-card path; skipped just like private-group idx in MineAddOps.
        var orderList = new List<object?>
        {
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                  ["card"] = new Dictionary<string, object?> { ["baseIdx"] = "g1", ["isPremium"] = 0L } } },
        };
        Assert.That(
            KnownListBuilder.MineCopyTokens(orderList, new Dictionary<int, long>(), new Dictionary<int, long>()),
            Is.Empty);
    }

    [Test]
    public void MineCopyTokens_yields_for_every_idx_in_a_multi_idx_copy_op()
    {
        var orderList = new List<object?> { CopyOp(new[] { 31L, 32L }, baseIdx: 5L, isSelf: 1) };
        var selfMap = new Dictionary<int, long> { [5] = 700L };
        var mined = KnownListBuilder.MineCopyTokens(orderList, selfMap, new Dictionary<int, long>()).ToList();
        Assert.That(mined, Is.EquivalentTo(new[] { new MinedToken(31, 700L, CardOwner.Self), new MinedToken(32, 700L, CardOwner.Self) }));
    }

    // A uList entry as it arrives in a RawBody. Minimal = the 5 always-present fields
    // (capture battle-traffic_tk2_regular.ndjson:75). Optional fields added per-test.
    private static Dictionary<string, object?> UListEntry(
        long[] idxList, int from, int to, int isSelf, string skill) => new()
    {
        ["idxList"] = idxList.Select(i => (object?)i).ToList(),
        ["from"] = (long)from, ["to"] = (long)to, ["isSelf"] = (long)isSelf, ["skill"] = skill,
    };

    [Test]
    public void RelayUList_maps_the_minimal_capture_entry_shape()
    {
        // battle-traffic_tk2_regular.ndjson:75 — a hidden deck-fetch (no cardId), the only uList shape
        // in any capture. The 5 always-present fields map; conditionals stay null.
        var uList = new List<object?> { UListEntry(new[] { 16L, 22L }, from: 0, to: 10, isSelf: 1, skill: "37|36|0") };
        var relayed = KnownListBuilder.RelayUList(uList);

        Assert.That(relayed, Is.Not.Null);
        Assert.That(relayed!.Count, Is.EqualTo(1));
        var e = relayed[0];
        Assert.That(e.IdxList, Is.EqualTo(new[] { 16, 22 }));
        Assert.That(e.From, Is.EqualTo(0));
        Assert.That(e.To, Is.EqualTo(10));
        Assert.That(e.IsSelf, Is.EqualTo(CardOwner.Self));
        Assert.That(e.Skill, Is.EqualTo("37|36|0"));
        Assert.That(e.CardId, Is.Null);
        Assert.That(e.Clan, Is.Null);
        Assert.That(e.Cost, Is.Null);
        Assert.That(e.SkillKeyCardIdx, Is.Null);
        Assert.That(e.RandomTargetIdx, Is.Null);
        Assert.That(e.IsInvoke, Is.Null);
        Assert.That(e.AttachTarget, Is.Null);
    }

    [Test]
    public void RelayUList_maps_a_revealed_summon_with_all_conditional_fields()
    {
        // Decomp-grounded (no capture): a revealed summon-to-field carries cardId + clan + cost etc.
        var entry = UListEntry(new[] { 40L }, from: 0, to: 20, isSelf: 1, skill: "5|3|0");
        entry["cardId"] = 900111010L;
        entry["clan"] = 8L;
        entry["cost"] = 2L;
        entry["skillKeyCardIdx"] = new List<object?> { 7L };
        entry["randomTargetIdx"] = new List<object?> { 2L, 3L };
        entry["isInvoke"] = 1L;
        entry["attachTarget"] = "12,13";
        var relayed = KnownListBuilder.RelayUList(new List<object?> { entry });

        var e = relayed![0];
        Assert.That(e.To, Is.EqualTo(20));
        Assert.That(e.CardId, Is.EqualTo(900111010L));
        Assert.That(e.Clan, Is.EqualTo(8));
        Assert.That(e.Cost, Is.EqualTo(2));
        Assert.That(e.SkillKeyCardIdx, Is.EqualTo(new[] { 7 }));
        Assert.That(e.RandomTargetIdx, Is.EqualTo(new[] { 2, 3 }));
        Assert.That(e.IsInvoke, Is.True);
        Assert.That(e.AttachTarget, Is.EqualTo("12,13"));
    }

    [Test]
    public void RelayUList_preserves_multiple_entries_in_order()
    {
        var uList = new List<object?>
        {
            UListEntry(new[] { 16L }, 0, 10, 1, "a"),
            UListEntry(new[] { 22L }, 0, 20, 0, "b"),
        };
        var relayed = KnownListBuilder.RelayUList(uList);

        Assert.That(relayed!.Count, Is.EqualTo(2));
        Assert.That(relayed[0].Skill, Is.EqualTo("a"));
        Assert.That(relayed[1].Skill, Is.EqualTo("b"));
        Assert.That(relayed[1].IsSelf, Is.EqualTo(CardOwner.Opponent));
    }

    [Test]
    public void RelayUList_returns_null_for_missing_or_empty()
    {
        Assert.That(KnownListBuilder.RelayUList(null), Is.Null);
        Assert.That(KnownListBuilder.RelayUList(new List<object?>()), Is.Null);
    }
}
