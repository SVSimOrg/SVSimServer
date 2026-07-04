using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

/// <summary>Echo is the receiver's per-frame ack; the client has no inbound Echo handler, so the
/// node never relays it (bullet-2 audit — relaying would risk an echo->echo storm). It IS mined,
/// though: an Echo's orderList carries the same add-op shape as PlayActions
/// (SendCardDataMaker.MakeEchoData -> MakeCommonSendAndEchoCardData), so it can hold a token's real
/// identity — notably the receiver's own (isSelf:1) view of a cross-side gift. We mine it (concrete
/// tokens and baseIdx copies) into the right side's idx->cardId map and still return no routes
/// (mining != relaying).</summary>
internal sealed class EchoHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        if (ctx.BothSidesAfterReady())
        {
            var orderList = (ctx.Env.Body as RawBody)?.Entries.GetValueOrDefault(WireKeys.OrderList);
            ctx.State.RecordTokensFrom(ctx.From, ctx.Other, orderList);
            // Copy tokens ride Echo too (same add-op shape); resolve baseIdx against the side's map.
            ctx.State.RecordCopyTokensFrom(ctx.From, ctx.Other, orderList);
            // No RecordChoicePicksFrom here: choice picks ride keyAction.selectCard on the generating
            // SEND, not the receiver's Echo (Echo carries orderList only) — the pick is already
            // recorded by PlayActionsHandler. MineChoicePicks(orderList, null) would yield nothing.
        }
        return Array.Empty<DispatchRoute>();
    }
}
