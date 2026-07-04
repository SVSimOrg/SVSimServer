using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>Server-synthesized control/broadcast frames + inbound-body helpers, relocated verbatim
/// from BattleSession so the per-URI handlers can build them. Pure: no session state.</summary>
internal static class BattleFrames
{
    internal static MsgEnvelope BuildAck(NetworkBattleUri uri) => new(
        uri,
        ViewerId: ServerBattleFrames.FakeOpponentViewerId,
        Uuid: WireConstants.ServerUuid,
        Bid: null,
        RetryAttempt: 0,
        Cat: EmitCategory.General,
        PubSeq: null,
        PlaySeq: null,
        Body: new ResultCodeOnlyBody());

    internal static MsgEnvelope BuildTurnEndBroadcast() => new(
        NetworkBattleUri.TurnEnd,
        ViewerId: ServerBattleFrames.FakeOpponentViewerId,
        Uuid: WireConstants.ServerUuid,
        Bid: null,
        RetryAttempt: 0,
        Cat: EmitCategory.Battle,
        PubSeq: null,
        PlaySeq: null,
        Body: new TurnEndBody(TurnState: TurnState.First));

    internal static MsgEnvelope BuildJudgeBroadcast() => new(
        NetworkBattleUri.Judge,
        ViewerId: ServerBattleFrames.FakeOpponentViewerId,
        Uuid: WireConstants.ServerUuid,
        Bid: null,
        RetryAttempt: 0,
        Cat: EmitCategory.Battle,
        PubSeq: null,
        PlaySeq: null,
        Body: new JudgeBody(Spin: BattleFrameDefaults.OpponentJudgeSpin));

    internal static MsgEnvelope BuildBattleFinish(BattleResult result) => new(
        NetworkBattleUri.BattleFinish,
        ViewerId: ServerBattleFrames.FakeOpponentViewerId,
        Uuid: WireConstants.ServerUuid,
        Bid: null,
        RetryAttempt: 0,
        Cat: EmitCategory.Battle,
        PubSeq: null,
        PlaySeq: null,
        Body: new BattleFinishBody(Result: result));

    internal static IReadOnlyList<long> ExtractIdxList(MsgEnvelope env)
    {
        if (env.Body is not RawBody rawBody) return Array.Empty<long>();
        if (rawBody.Entries.TryGetValue(WireKeys.IdxList, out var raw) && raw is System.Collections.IEnumerable seq && raw is not string)
        {
            var result = new List<long>();
            foreach (var item in seq)
            {
                switch (item)
                {
                    case long l: result.Add(l); break;
                    case int i: result.Add(i); break;
                    case double d: result.Add((long)d); break;
                    case decimal m: result.Add((long)m); break;
                    case string s when long.TryParse(s, out var p): result.Add(p); break;
                }
            }
            return result;
        }
        return Array.Empty<long>();
    }
}
