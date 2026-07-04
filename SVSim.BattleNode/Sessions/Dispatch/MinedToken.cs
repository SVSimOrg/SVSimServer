using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>One generated-token identity mined from a sender's <c>orderList</c> <c>add</c> op:
/// the token's <paramref name="Idx"/> in a side's index space, its resolved
/// <paramref name="CardId"/>, and <paramref name="IsSelf"/> — whose map it belongs to (the
/// sender's own token vs a cross-side gift living in the opponent's index space; routed by
/// <see cref="BattleSessionState.RecordTokensFrom"/>). Replaces the transpose-prone
/// <c>(int Idx, long CardId, CardOwner IsSelf)</c> tuple the <c>Mine*</c> methods returned:
/// <c>Idx</c> and <c>CardId</c> are both numeric, so <c>(cardId, idx, …)</c> silently compiled
/// and corrupted the reveal map. As a positional record struct it keeps the named members and
/// positional deconstruct (call sites stay <c>foreach (var (idx, cardId, isSelf) in …)</c>)
/// while the compiler rejects a transposed construction.</summary>
internal readonly record struct MinedToken(int Idx, long CardId, CardOwner IsSelf);
