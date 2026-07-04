using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Hosting;
using SVSim.BattleNode.Sessions.Participants;
using SVSim.UnitTests.BattleNode.Infrastructure;

namespace SVSim.UnitTests.BattleNode.Hosting;

[TestFixture]
public class WaitingRoomTests
{
    [Test]
    public void Pair_on_empty_slot_returns_null()
    {
        var room = new WaitingRoom();
        var participant = NewParticipant(viewerId: 1);

        var paired = room.Pair("bid-1", participant);

        Assert.That(paired, Is.Null);
    }

    [Test]
    public async Task Park_then_Pair_resolves_with_each_arriver_seeing_the_other()
    {
        var room = new WaitingRoom();
        var first = NewParticipant(viewerId: 1);
        var second = NewParticipant(viewerId: 2);

        var parkTask = room.ParkAsync("bid-1", first, TimeSpan.FromSeconds(5), CancellationToken.None);
        // Yield so Park's TryAdd lands first.
        await Task.Yield();

        var firstReturnedToSecond = room.Pair("bid-1", second);
        var secondReturnedToFirst = await parkTask;

        Assert.That(firstReturnedToSecond, Is.SameAs(first),
            "Pair must return the first arriver to the second.");
        Assert.That(secondReturnedToFirst, Is.SameAs(second),
            "Park must return the second arriver to the first.");
    }

    [Test]
    public async Task Park_times_out_returns_null_and_evicts_slot()
    {
        var room = new WaitingRoom();
        var first = NewParticipant(viewerId: 1);

        var second = await room.ParkAsync("bid-1", first, TimeSpan.FromMilliseconds(50), CancellationToken.None);

        Assert.That(second, Is.Null);
        // Slot should be evicted; a subsequent Pair returns null (no first arriver).
        var paired = room.Pair("bid-1", NewParticipant(viewerId: 2));
        Assert.That(paired, Is.Null);
    }

    [Test]
    public void Evict_is_idempotent()
    {
        var room = new WaitingRoom();

        Assert.DoesNotThrow(() => room.Evict("bid-1"));
        Assert.DoesNotThrow(() => room.Evict("bid-1"));
    }

    private static RealParticipant NewParticipant(long viewerId)
    {
        var ws = new TestWebSocket();
        var ctx = new MatchContext(
            SelfDeckCardIds: Array.Empty<long>(),
            ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
            CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "0",
            EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0,
            BattleModeId: BattleModes.TakeTwo);
        return new RealParticipant(ws, viewerId, ctx, NullLogger<RealParticipant>.Instance);
    }
}
