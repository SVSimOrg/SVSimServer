using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

/// <summary>PvP PlayActions translator. Synthesizes the opponent-facing knownList from the engine-resolved
/// played card + the orderList move op, renames targetList -> oppoTargetList, drops orderList, and forwards a
/// stripped keyAction for choice/Discover plays ({type,cardId}; selectCard dropped for a hidden open:0 pick).
/// The played card's IDENTITY (cardId), cost, spellboost, clan and tribe are all ENGINE-sourced (M-HC-3/4);
/// the identity is engine-first with a wire-mined idx->cardId fallback (deck card or recorded token). The
/// fallback still covers the token cases the engine doesn't headlessly resolve at a wire idx — choice/Discover
/// picks (keyAction.selectCard), baseIdx copies, and cross-side (isSelf:0) gifts — all WIRE-MINED on earlier
/// (or the same) frames via the Record*From calls below (TODO(M-HC-4f): retire once engine-proven at a wire
/// idx). An un-resolvable token idx (no engine id, no mined entry) degrades to {playIdx,type} (no knownList).
/// Bot drop (no rule).</summary>
internal sealed class PlayActionsHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        if (!ctx.BothSidesAfterReady())
            return Array.Empty<DispatchRoute>();

        var entries = (ctx.Env.Body as RawBody)?.Entries ?? new Dictionary<string, object?>();
        var playIdx = (int)KnownListBuilder.AsLong(entries.GetValueOrDefault(WireKeys.PlayIdx));
        var type = (int)KnownListBuilder.AsLong(entries.GetValueOrDefault(WireKeys.Type));

        var orderList = entries.GetValueOrDefault(WireKeys.OrderList);
        var keyAction = entries.GetValueOrDefault(WireKeys.KeyAction);

        // Mine generated-token identities from this frame's add ops into the right side's idx->cardId
        // map (isSelf:1 → sender; isSelf:0 → opponent, a cross-side gift), so a token played in a LATER
        // frame resolves its cardId — by whichever side ends up playing it (bullet-3 audit F1).
        ctx.State.RecordTokensFrom(ctx.From, ctx.Other, orderList);

        // Choice/Discover-into-hand: the chosen cardId rides keyAction.selectCard (the orderList's
        // choiceAdd carries candidates only). Record idx->chosenCardId now so the later play reveals it.
        ctx.State.RecordChoicePicksFrom(ctx.From, ctx.Other, orderList, keyAction);

        // Copy/clone tokens: card:{baseIdx} points at a card in the actor's own index space; resolve it
        // against that side's map and record copyIdx->cardId so the later play reveals it. Ordered after
        // the plain/choice mining so a same-frame copy of a just-added token resolves against the live map.
        ctx.State.RecordCopyTokensFrom(ctx.From, ctx.Other, orderList);

        var deckMap = ctx.State.GetOrSeedDeckMap(ctx.From);
        // The wire-mined idx->cardId fallback identity (deck card, or a token recorded into the map above by
        // the Record*From calls). Used ONLY as the engine-read's fallback below — 0 when the idx isn't in the map.
        long mappedCardId = deckMap.TryGetValue(playIdx, out var mid) ? mid : 0L;

        // The ENGINE-RESOLVED play-time cost (M-HC-3a). The conductor's ShadowIngest already ran
        // engine.Receive for THIS frame before this handler runs, so the engine has resolved the play and
        // PlayedCardCost reads the discounted cost it actually charged (spellboost + board modifiers folded
        // in BY CONSTRUCTION — no bookkeeping). Sender's seat == ctx.A (BattleSession.ShadowIngest uses the
        // same ReferenceEquals(from, A) mapping). Degrades to 0 when the engine isn't ready for this session
        // (Setup failed and the ComputeFrames try/catch swallowed it, ND6) so a non-engine session never crashes.
        bool senderSeat = ReferenceEquals(ctx.From, ctx.A);
        int playedCost = ctx.Engine.PlayedCardCost(senderSeat, playIdx, fallback: 0);

        // The spellboost (spell-charge) COUNT is now ALSO engine-sourced (M-HC-3b) — the wire-derived
        // bookkeeping is retired. The engine accumulated the true count for the played card during the
        // ShadowIngest's engine.Receive (each spell play runs the card's own AddSpellChargeCount), so
        // PlayedCardSpellboost reads it straight off the resolved card (persist-post-play, same zone search
        // as the cost). Cost already folds the discount in by construction; the count rides the entry only
        // to stay prod-faithful (prod sends the real count). Same senderSeat mapping as the cost read.
        int playedSpellboost = ctx.Engine.PlayedCardSpellboost(senderSeat, playIdx, fallback: 0);

        // clan/tribe are ALSO engine-sourced (M-HC-4e) — read off the resolved card's Clan/Tribe getters, so
        // any skill-applied clan/tribe change (e.g. change_affiliation) rides the wire (the static card-master
        // value would miss it). Prod always emits both on every knownList entry: clan as the int ClanType
        // ordinal, tribe as the comma-joined int TribeType string ("0" for none). Same senderSeat mapping.
        int playedClan = ctx.Engine.PlayedCardClan(senderSeat, playIdx, fallback: 0);
        string playedTribe = ctx.Engine.PlayedCardTribe(senderSeat, playIdx, fallback: "0");

        // The card IDENTITY is now ALSO engine-sourced (M-HC-4f): read the engine-resolved CardId off the played
        // card, falling back to the wire-mined idx->cardId map when the engine doesn't resolve it. PROVEN
        // engine-resolved (each backed by a HeadlessConductorTests PlayedCardId_* test): a DECK card and a
        // receive-path SUBSTITUTED/revealed token (the engine seats the wire id at the wire idx). The fallback
        // is LOAD-BEARING, not vestigial: it still supplies the identity for (a) a non-engine session (gate not
        // acquired) and (b) the token cases the engine doesn't headlessly resolve AT A WIRE IDX — choice/Discover
        // tokens (autonomous token_draw seats them at engine Index 0 headless), copy/clone tokens, and cross-side
        // (isSelf:0) gifts. Those remain WIRE-MINED via the Record*From calls above; retiring that mining is gated
        // on proving the engine seats them at a wire idx, which the current headless harness can't fixture cheaply.
        // TODO(M-HC-4f): once a node-native (or full wire add-op/replace) fixture proves the engine resolves a
        // choice token, a copy token and a cross-side gift at their WIRE idx, retire MineChoicePicks/MineCopyTokens/
        // the cross-side MineAddOps branch + the matching Record* bookkeeping and drop this fallback to deck-only.
        long playedCardId = ctx.Engine.PlayedCardId(senderSeat, playIdx, fallback: mappedCardId);

        var played = KnownListBuilder.BuildPlayedCard(
            playIdx, playedCardId, orderList, cost: playedCost, spellboost: playedSpellboost,
            clan: playedClan, tribe: playedTribe);
        var oppoTargets = KnownListBuilder.RenameTargets(entries.GetValueOrDefault(WireKeys.TargetList));

        // Deck-sourced movements (fetch / search / summon-from-deck) ride the uList — a verbatim,
        // separate receive slot the node forwards unchanged (bullet-3 audit F1). The node makes no
        // reveal decision; cardId presence is the sender's call. Coexists with the synthesized
        // knownList in the same frame (capture line 75).
        var uList = KnownListBuilder.RelayUList(entries.GetValueOrDefault(WireKeys.UList));

        var body = new PlayActionsBroadcastBody(
            PlayIdx: playIdx,
            Type: type,
            KnownList: played is null ? null : new[] { played },
            OppoTargetList: oppoTargets,
            UList: uList,
            // {type,cardId} forwarded so the opponent renders the choice token; selectCard dropped
            // when open==0 (hidden draw-to-hand pick). Null for a vanilla play (no keyAction).
            KeyAction: KnownListBuilder.StripKeyActionForOpponent(keyAction));

        var frame = ctx.Env with { Body = body };
        return new[] { new DispatchRoute(ctx.Other, frame, Stock.Normal) };
    }
}
