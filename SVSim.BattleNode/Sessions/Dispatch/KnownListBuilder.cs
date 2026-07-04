using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>Pure transforms from the active player's RawBody sub-structures to the opponent-facing
/// shapes. No session state, no wire I/O — unit-testable in isolation. RawBody nested values arrive
/// as <c>Dictionary&lt;string,object?&gt;</c> / <c>List&lt;object?&gt;</c> with numeric leaves boxed
/// as long/int/double (see MsgEnvelope.FromJson). Inbound wire keys come from <see cref="WireKeys"/>.</summary>
internal static class KnownListBuilder
{
    /// <summary>The played card's knownList entry, or null when its identity can't be synthesized
    /// (the played idx resolves to no card identity, or there's no matching move op). <paramref name="cardId"/>,
    /// <paramref name="cost"/>, <paramref name="spellboost"/>, <paramref name="clan"/> and <paramref name="tribe"/>
    /// are ALL ENGINE-SOURCED at the call site (M-HC-3a/3b/4e/4f) — the handler reads them off the shadow engine
    /// (<c>SessionBattleEngine.PlayedCardId</c>/<c>PlayedCardCost</c>/<c>PlayedCardSpellboost</c>/<c>PlayedCardClan</c>/
    /// <c>PlayedCardTribe</c>) and passes them in; all land on the entry verbatim.
    /// <para><paramref name="cardId"/> is the engine-resolved TRUE identity of the played card (M-HC-4f). The
    /// handler computes it engine-first with a fallback: <c>engine.PlayedCardId(seat, playIdx, fallback: deckMapId)</c>,
    /// where <c>deckMapId</c> is the wire-mined idx→cardId entry (deck card or recorded token). So an engine-backed
    /// session emits the engine identity; a non-engine session (or an idx the engine doesn't headlessly resolve at a
    /// wire idx — choice/copy/cross-side tokens, still wire-mined; see TODO(M-HC-4f) in PlayActionsHandler) falls back
    /// to the mined map. A <paramref name="cardId"/> of 0 (no engine id AND no mined entry) means an un-synthesizable
    /// play → null (no knownList; the play degrades to {playIdx,type}).</para>
    /// The wire-derived spellboost bookkeeping is retired — the engine owns cost and count by construction (cost folds
    /// the spellboost discount in already; the count rides the entry only to stay prod-faithful, prod sends the real
    /// count here). <paramref name="clan"/>/<paramref name="tribe"/> are the LIVE (skill-applied) values the engine
    /// resolved — prod always sends both on every knownList entry (clan int, tribe the comma-joined int string, "0"
    /// for none). Prod's client reads cost straight into the card's cost model (<c>NetworkBattleReceiver</c>), so a
    /// vanilla play resolves to its base cost and count 0. attachTarget stays "".</summary>
    public static KnownCardEntry? BuildPlayedCard(
        int playIdx, long cardId, object? orderList,
        int cost = 0, int spellboost = 0, int clan = 0, string tribe = "0")
    {
        if (cardId == 0) return null; // no engine id AND no mined/deck-map fallback → can't synthesize an identity
        var to = ExtractMoveTo(orderList, playIdx);
        if (to is null) return null;
        return new KnownCardEntry(
            Idx: playIdx, CardId: cardId, To: to.Value, Spellboost: spellboost, AttachTarget: "", Cost: cost,
            Clan: clan, Tribe: tribe);
    }

    /// <summary>The <c>to</c> place-state of the FIRST <c>move</c> op whose <c>idx</c> list contains
    /// <paramref name="playIdx"/> (the played card's own move; later add/alter ops are the deferred
    /// token slice), or null if absent. NOTE: the sender-side <c>to</c> is passed through verbatim —
    /// for the vanilla slice we assume send-side and recv-side place-state codes match, pending
    /// recv-capture confirmation.</summary>
    public static int? ExtractMoveTo(object? orderList, int playIdx)
    {
        if (orderList is not IEnumerable<object?> ops) return null;
        foreach (var op in ops)
        {
            if (op is not IDictionary<string, object?> opDict) continue;
            if (!opDict.TryGetValue(WireKeys.Move, out var moveRaw) || moveRaw is not IDictionary<string, object?> move) continue;
            if (move.TryGetValue(WireKeys.Idx, out var idxRaw) && idxRaw is IEnumerable<object?> idxList)
            {
                foreach (var i in idxList)
                    if (AsLong(i) == playIdx && move.TryGetValue(WireKeys.To, out var toRaw))
                        return (int)AsLong(toRaw);
            }
        }
        return null;
    }

    /// <summary>Mine generated-token identities from a sender's <c>add</c> ops: yields
    /// <c>(idx, cardId, isSelf)</c> for every idx in each <c>{add:{idx:[...], isSelf, card:{cardId}}}</c>
    /// op. <c>isSelf</c> is surfaced verbatim (the sender's perspective tag on <c>CardObj.IsPlayer</c>,
    /// <c>RegisterToken.cs:22</c>) so the caller can route the identity into the correct side's map —
    /// <c>isSelf:1</c> = the sender's own token, <c>isSelf:0</c> = a cross-side gift living at this idx
    /// in the OPPONENT's index space (<see cref="BattleSessionState.RecordTokensFrom"/>). Skips any add
    /// whose <c>card</c> has no concrete <c>cardId</c> — choice tokens (<c>card:{candidates}</c>,
    /// <c>RegisterChoiceAdd</c>), copy tokens (<c>card:{baseIdx}</c>, <c>RegisterCopyToken</c>), and
    /// private-group adds (string <c>idx</c>) — all deferred and all caught by the <c>cardId</c>-key /
    /// <c>idx</c>-is-list guards. This is the only place a freshly-generated card's identity exists on
    /// the wire (bullet-3 audit F1; producing code <c>RegisterToken</c>/<c>RegisterActionBase</c>) —
    /// the played-card op itself never carries a <c>cardId</c>.</summary>
    public static IEnumerable<MinedToken> MineAddOps(object? orderList)
    {
        if (orderList is not IEnumerable<object?> ops) yield break;
        foreach (var op in ops)
        {
            if (op is not IDictionary<string, object?> opDict) continue;
            if (!opDict.TryGetValue(WireKeys.Add, out var addRaw) || addRaw is not IDictionary<string, object?> add) continue;

            add.TryGetValue(WireKeys.IsSelf, out var isSelfRaw);
            var isSelf = (CardOwner)(int)AsLong(isSelfRaw);

            if (!add.TryGetValue(WireKeys.Card, out var cardRaw) || cardRaw is not IDictionary<string, object?> card) continue;
            if (!card.TryGetValue(WireKeys.CardId, out var cardIdRaw)) continue; // candidates/isChoice → no identity yet
            var cardId = AsLong(cardIdRaw);

            if (!add.TryGetValue(WireKeys.Idx, out var idxRaw) || idxRaw is not IEnumerable<object?> idxList) continue;
            foreach (var i in idxList)
                yield return new MinedToken((int)AsLong(i), cardId, isSelf);
        }
    }

    /// <summary>Mine choice/Discover-token identities: for each <c>isChoice</c> add op (idx, isSelf,
    /// candidates), resolve its cardId from the keyAction <c>selectCard</c> pick whose cardId is in that
    /// op's candidate pool. Yields <c>(idx, cardId, isSelf)</c> — same shape as <see cref="MineAddOps"/>,
    /// routed by the same <see cref="BattleSessionState.RecordTokensFrom"/> rule. The pick is on
    /// keyAction.selectCard, NOT the add op (RegisterChoiceAdd strips the concrete cardId,
    /// <c>NetworkBattleSetupCardEvent.cs:531-543</c>); the candidate-membership join handles the single
    /// case unambiguously (multi-choice: each chosen cardId matches the one choiceAdd whose candidates
    /// contain it). <c>type</c>/<c>cardId</c>/<c>open</c> on the keyAction are ignored here — <c>open</c>
    /// only gates the strip (<see cref="StripKeyActionForOpponent"/>), not the recording. An add whose
    /// candidates contain none of the picks is skipped (defensive — no record, no desync); Echo (no
    /// keyAction) yields nothing, leaving it mining-only via <see cref="MineAddOps"/>.</summary>
    public static IEnumerable<MinedToken> MineChoicePicks(object? orderList, object? keyAction)
    {
        if (orderList is not IEnumerable<object?> ops) yield break;

        // Flatten every selectCard.cardId pick across all keyAction entries into a membership set.
        var picks = new HashSet<long>();
        if (keyAction is IEnumerable<object?> kaEntries)
        {
            foreach (var ka in kaEntries)
            {
                if (ka is not IDictionary<string, object?> kaDict) continue;
                if (!kaDict.TryGetValue(WireKeys.SelectCard, out var scRaw) || scRaw is not IDictionary<string, object?> sc) continue;
                if (!sc.TryGetValue(WireKeys.CardId, out var idsRaw) || idsRaw is not IEnumerable<object?> ids) continue;
                foreach (var id in ids) picks.Add(AsLong(id));
            }
        }
        if (picks.Count == 0) yield break;

        foreach (var op in ops)
        {
            if (op is not IDictionary<string, object?> opDict) continue;
            if (!opDict.TryGetValue(WireKeys.Add, out var addRaw) || addRaw is not IDictionary<string, object?> add) continue;
            if (!add.ContainsKey(WireKeys.IsChoice)) continue;
            if (!add.TryGetValue(WireKeys.Card, out var cardRaw) || cardRaw is not IDictionary<string, object?> card) continue;
            if (!card.TryGetValue(WireKeys.Candidates, out var candRaw) || candRaw is not IEnumerable<object?> candidates) continue;

            // The chosen cardId is the candidate that the active player picked (∈ picks). One per op.
            long? chosen = null;
            foreach (var c in candidates)
            {
                var cid = AsLong(c);
                if (picks.Contains(cid)) { chosen = cid; break; }
            }
            if (chosen is null) continue; // no pick in this op's pool — skip (no desync, just no record)

            add.TryGetValue(WireKeys.IsSelf, out var isSelfRaw);
            var isSelf = (CardOwner)(int)AsLong(isSelfRaw);

            if (!add.TryGetValue(WireKeys.Idx, out var idxRaw) || idxRaw is not IEnumerable<object?> idxList) continue;
            foreach (var i in idxList)
                yield return new MinedToken((int)AsLong(i), chosen.Value, isSelf);
        }
    }

    /// <summary>Mine copy/clone-token identities: for each copy <c>add</c> op
    /// (<c>{idx:[...], isSelf, card:{baseIdx, isPremium}}</c>), resolve its cardId from the appropriate
    /// side's idx->cardId map. The copied card lives at <c>baseIdx</c> in the actor's OWN index space —
    /// <c>RegisterCopyToken</c> is emitted only for <c>!IsReferenceOpponenCard</c>
    /// (<c>NetworkBattleManagerBase.cs:1106</c>); a cross-side copy sends a concrete <c>cardId</c> via a
    /// plain <c>RegisterToken</c> instead (handled by <see cref="MineAddOps"/>). Yields
    /// <c>(idx, cardId, isSelf)</c> — same shape as <see cref="MineAddOps"/>, routed by the same
    /// <see cref="BattleSessionState.RecordTokensFrom"/> rule: <c>isSelf:1</c> resolves+records into the
    /// sender's map (<paramref name="selfMap"/>), <c>isSelf:0</c> into the opponent's
    /// (<paramref name="otherMap"/>). Skips an add with a concrete <c>cardId</c> (→ MineAddOps), one with
    /// <c>candidates</c> (→ MineChoicePicks), a <c>string</c> <c>baseIdx</c> (private-group copy,
    /// <c>RegisterCopyToken.cs:19-22</c>), and a <c>baseIdx</c> absent from the chosen map (unknown source
    /// → degrade, no desync). <c>isPremium</c> (IsFoil) is cosmetic and ignored.</summary>
    public static IEnumerable<MinedToken> MineCopyTokens(
        object? orderList,
        IReadOnlyDictionary<int, long> selfMap,
        IReadOnlyDictionary<int, long> otherMap)
    {
        if (orderList is not IEnumerable<object?> ops) yield break;
        foreach (var op in ops)
        {
            if (op is not IDictionary<string, object?> opDict) continue;
            if (!opDict.TryGetValue(WireKeys.Add, out var addRaw) || addRaw is not IDictionary<string, object?> add) continue;

            if (!add.TryGetValue(WireKeys.Card, out var cardRaw) || cardRaw is not IDictionary<string, object?> card) continue;
            if (card.ContainsKey(WireKeys.CardId)) continue;                 // concrete token → MineAddOps
            if (!card.TryGetValue(WireKeys.BaseIdx, out var baseRaw)) continue;    // not a copy (candidates → MineChoicePicks)
            if (baseRaw is string) continue;                                // private-group copy → string baseIdx, skip
            var baseIdx = (int)AsLong(baseRaw);

            add.TryGetValue(WireKeys.IsSelf, out var isSelfRaw);
            var isSelf = (CardOwner)(int)AsLong(isSelfRaw);
            var map = isSelf == CardOwner.Self ? selfMap : otherMap;
            if (!map.TryGetValue(baseIdx, out var cardId)) continue;        // unknown source → degrade

            if (!add.TryGetValue(WireKeys.Idx, out var idxRaw) || idxRaw is not IEnumerable<object?> idxList) continue;
            foreach (var i in idxList)
                yield return new MinedToken((int)AsLong(i), cardId, isSelf);
        }
    }

    /// <summary>Map an inbound keyAction (the active player's send) to the opponent-facing list:
    /// for each Choice(1)/HaveBeforeSkillChoice(5) entry, keep <c>{type,cardId}</c> and drop
    /// <c>selectCard</c> when its <c>open==0</c> (hidden draw-to-hand pick stays secret), pass it
    /// through when <c>open==1</c> (visible board choice — provisional reveal-immediately, §6).
    /// Non-choice KeyActionTypes are dropped (current behavior) until their own specs. Returns null
    /// for absent/empty keyAction or when every entry was dropped (vanilla play unchanged).</summary>
    public static IReadOnlyList<KeyActionEntry>? StripKeyActionForOpponent(object? keyAction)
    {
        if (keyAction is not IEnumerable<object?> entries) return null;
        var result = new List<KeyActionEntry>();
        foreach (var e in entries)
        {
            if (e is not IDictionary<string, object?> d) continue;
            d.TryGetValue(WireKeys.Type, out var typeRaw);
            var type = (KeyActionType)(int)AsLong(typeRaw);
            if (type is not (KeyActionType.Choice or KeyActionType.HaveBeforeSkillChoice)) continue;

            d.TryGetValue(WireKeys.CardId, out var cardIdRaw);
            var cardId = AsLong(cardIdRaw);

            SelectCardEntry? selectCard = null;
            if (d.TryGetValue(WireKeys.SelectCard, out var scRaw) && scRaw is IDictionary<string, object?> sc)
            {
                sc.TryGetValue(WireKeys.Open, out var openRaw);
                var open = (ChoiceVisibility)(int)AsLong(openRaw);
                if (open != ChoiceVisibility.Hidden && sc.TryGetValue(WireKeys.CardId, out var idsRaw) && idsRaw is IEnumerable<object?> ids)
                    selectCard = new SelectCardEntry(ids.Select(AsLong).ToList(), open);
            }
            result.Add(new KeyActionEntry(type, cardId, selectCard));
        }
        return result.Count == 0 ? null : result;
    }

    /// <summary>Rename <c>targetList</c> -> <c>oppoTargetList</c>; <c>isSelf</c> is actor-relative
    /// and passes through unchanged (F2). Null for a missing/empty list.</summary>
    public static IReadOnlyList<OppoTargetEntry>? RenameTargets(object? targetList)
    {
        if (targetList is not IEnumerable<object?> entries) return null;
        var result = new List<OppoTargetEntry>();
        foreach (var e in entries)
        {
            if (e is not IDictionary<string, object?> d) continue;
            d.TryGetValue(WireKeys.TargetIdx, out var targetIdxRaw);
            d.TryGetValue(WireKeys.IsSelf, out var isSelfRaw);
            result.Add(new OppoTargetEntry(
                TargetIdx: (int)AsLong(targetIdxRaw),
                IsSelf: (CardOwner)(int)AsLong(isSelfRaw)));
        }
        return result.Count == 0 ? null : result;
    }

    /// <summary>Map the sender's <c>uList</c> (unapproved-movement list) to the opponent-facing
    /// <see cref="UnapprovedCardEntry"/> list, VERBATIM — the node makes no reveal decision; it forwards
    /// whatever the sender emitted (cardId present = the sender chose to reveal). The five always-present
    /// fields (idxList/from/to/isSelf/skill) map directly; the conditionals map only when their key is
    /// present (mirroring the emitter, <c>SendCardDataMaker.MakeUList:188-244</c>). Null for an
    /// absent/empty list (mirrors <see cref="RenameTargets"/>). isSelf/place-states pass through unchanged
    /// (F2; same verbatim assumption already shipped for the synthesized knownList).</summary>
    public static IReadOnlyList<UnapprovedCardEntry>? RelayUList(object? uList)
    {
        if (uList is not IEnumerable<object?> entries) return null;
        var result = new List<UnapprovedCardEntry>();
        foreach (var e in entries)
        {
            if (e is not IDictionary<string, object?> d) continue;

            d.TryGetValue(WireKeys.IdxList, out var idxRaw);
            d.TryGetValue(WireKeys.From, out var fromRaw);
            d.TryGetValue(WireKeys.To, out var toRaw);
            d.TryGetValue(WireKeys.IsSelf, out var isSelfRaw);
            d.TryGetValue(WireKeys.Skill, out var skillRaw);

            result.Add(new UnapprovedCardEntry(
                IdxList: AsIntList(idxRaw) ?? new List<int>(),
                From: (int)AsLong(fromRaw),
                To: (int)AsLong(toRaw),
                IsSelf: (CardOwner)(int)AsLong(isSelfRaw),
                Skill: skillRaw as string ?? "",
                CardId: d.TryGetValue(WireKeys.CardId, out var c) ? AsLong(c) : null,
                Clan: d.TryGetValue(WireKeys.Clan, out var cl) ? (int)AsLong(cl) : null,
                Cost: d.TryGetValue(WireKeys.Cost, out var co) ? (int)AsLong(co) : null,
                SkillKeyCardIdx: AsIntList(d.TryGetValue(WireKeys.SkillKeyCardIdx, out var sk) ? sk : null),
                RandomTargetIdx: AsIntList(d.TryGetValue(WireKeys.RandomTargetIdx, out var rt) ? rt : null),
                IsInvoke: d.TryGetValue(WireKeys.IsInvoke, out var iv) ? AsLong(iv) != 0 : null,
                AttachTarget: d.TryGetValue(WireKeys.AttachTarget, out var at) ? at as string : null));
        }
        return result.Count == 0 ? null : result;
    }

    /// <summary>Coerce a boxed RawBody list leaf to <c>List&lt;int&gt;</c> (each element via
    /// <see cref="AsLong"/>); null when the value isn't a list.</summary>
    private static IReadOnlyList<int>? AsIntList(object? value) =>
        value is IEnumerable<object?> items ? items.Select(i => (int)AsLong(i)).ToList() : null;

    /// <summary>Coerce a boxed RawBody numeric leaf (long/int/double/decimal/string) to long; 0 for
    /// null/unparseable.</summary>
    public static long AsLong(object? value) => value switch
    {
        long l => l,
        int i => i,
        double d => (long)d,
        decimal m => (long)m,
        string s when long.TryParse(s, out var p) => p,
        _ => 0,
    };
}
