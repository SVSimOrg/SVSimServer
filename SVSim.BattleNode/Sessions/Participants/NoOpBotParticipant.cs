using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Participants;

/// <summary>
/// Silent participant — produces no frames, swallows everything pushed to it.
/// Used as the "other" participant in <see cref="BattleType.Bot"/> sessions, where
/// the real opponent runs in the client and the server has no opponent-side state
/// to model. ViewerId is <see cref="ServerBattleFrames.FakeOpponentViewerId"/>;
/// Context is a fixed stub (irrelevant — never read because no frames are pushed
/// to the other side).
/// </summary>
public sealed class NoOpBotParticipant : IBattleParticipant
{
    /// <summary>Stub card-master id stamped on the bot's (never-read) MatchContext.</summary>
    private const string BotCardMasterName = "card_master_node_10015";

    public long ViewerId => ServerBattleFrames.FakeOpponentViewerId;
    public MatchContext Context { get; } = new(
        SelfDeckCardIds: Array.Empty<long>(),
        ClassId: CardClass.None, CharaId: "0", CardMasterName: BotCardMasterName,
        CountryCode: "", UserName: "Bot", SleeveId: "0",
        EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0,
        BattleModeId: 0);

    // Required by IBattleParticipant, but a silent bot never raises it — suppress the
    // "event is never used" warning rather than keeping a dead null-emitting method.
#pragma warning disable CS0067
    public event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;
#pragma warning restore CS0067

    public Task PushAsync(MsgEnvelope envelope, Stock stock, CancellationToken ct) => Task.CompletedTask;
    public Task RunAsync(CancellationToken ct) => Task.CompletedTask;
    public Task TerminateAsync(BattleFinishReason reason) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
