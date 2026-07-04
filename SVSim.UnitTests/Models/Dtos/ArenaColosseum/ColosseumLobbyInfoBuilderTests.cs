using System;
using NUnit.Framework;
using SVSim.Database.Enums;
using SVSim.Database.Models.Config;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

namespace SVSim.UnitTests.Models.Dtos.ArenaColosseum;

/// <summary>
/// Pure-projection tests for the builder shared by /mypage/index, /arena_colosseum/top,
/// and /arena_colosseum/get_fee_info. Locks the wire-level round-window derivation per
/// <c>Wizard/ColosseumEntryInfoTask.cs:111-126</c> plus the previously-missed
/// <c>colosseum_id</c>/<c>is_display_tips</c>/<c>tips_id</c> emissions.
/// </summary>
[TestFixture]
public class ColosseumLobbyInfoBuilderTests
{
    private static ColosseumSeasonConfig SeasonOn(int seasonId = 42) => new()
    {
        IsColosseumPeriod = true,
        SeasonId = seasonId,
        ColosseumName = "Test Cup",
        DeckFormat = Format.Rotation,
        EventStartTime = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
        EventEndTime   = new DateTime(2026, 7, 31, 23, 59, 59, DateTimeKind.Utc),
        SalesPeriodStart = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
        SalesPeriodEnd   = new DateTime(2026, 7, 31, 23, 59, 59, DateTimeKind.Utc),
    };

    private static ColosseumRoundsConfig ThreeRounds() => new()
    {
        Rounds = new()
        {
            new ColosseumRoundsConfig.RoundEntry
            {
                RoundId = 1,
                StartTime = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
                EndTime   = new DateTime(2026, 7, 10, 23, 59, 59, DateTimeKind.Utc),
            },
            new ColosseumRoundsConfig.RoundEntry
            {
                RoundId = 2,
                StartTime = new DateTime(2026, 7, 11, 10, 0, 0, DateTimeKind.Utc),
                EndTime   = new DateTime(2026, 7, 20, 23, 59, 59, DateTimeKind.Utc),
            },
            new ColosseumRoundsConfig.RoundEntry
            {
                RoundId = 3,
                StartTime = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc),
                EndTime   = new DateTime(2026, 7, 31, 23, 59, 59, DateTimeKind.Utc),
            },
        },
    };

    [Test]
    public void Off_season_emits_only_is_colosseum_period_false()
    {
        var info = ColosseumLobbyInfoBuilder.Build(
            new ColosseumSeasonConfig { IsColosseumPeriod = false },
            new ColosseumRoundsConfig(),
            DateTime.UtcNow);

        Assert.That(info.IsColosseumPeriod, Is.False);
        Assert.That(info.ColosseumName, Is.Null);
        Assert.That(info.NowRound, Is.Null);
        Assert.That(info.IsRoundPeriod, Is.Null);
        Assert.That(info.ColosseumId, Is.Null);
        Assert.That(info.IsDisplayTips, Is.Null);
    }

    [Test]
    public void Mid_round_2_emits_round_2_window_and_now_round_2()
    {
        var now = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc); // inside Round 2

        var info = ColosseumLobbyInfoBuilder.Build(SeasonOn(), ThreeRounds(), now);

        Assert.That(info.IsRoundPeriod, Is.True);
        Assert.That(info.NowRound, Is.EqualTo(2));
        Assert.That(info.NextRound, Is.Null);
        Assert.That(info.StartTime, Is.EqualTo("2026-07-11 10:00:00"));
        Assert.That(info.EndTime,   Is.EqualTo("2026-07-20 23:59:59"));
    }

    [Test]
    public void Between_rounds_emits_is_round_period_false_with_next_round()
    {
        var now = new DateTime(2026, 7, 11, 5, 0, 0, DateTimeKind.Utc); // after R1 end, before R2 start

        var info = ColosseumLobbyInfoBuilder.Build(SeasonOn(), ThreeRounds(), now);

        Assert.That(info.IsRoundPeriod, Is.False);
        Assert.That(info.NowRound, Is.Null);
        Assert.That(info.NextRound, Is.EqualTo(2),
            "client renders 'Round 2 opens in HH:MM:SS' — needs the next round number");
        Assert.That(info.StartTime, Is.EqualTo("2026-07-11 10:00:00"),
            "start_time is the next round's start so the client's countdown lands correctly");
    }

    [Test]
    public void Pre_event_emits_round_1_as_next()
    {
        var now = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc); // before R1 starts

        var info = ColosseumLobbyInfoBuilder.Build(SeasonOn(), ThreeRounds(), now);

        Assert.That(info.IsRoundPeriod, Is.False);
        Assert.That(info.NextRound, Is.EqualTo(1));
        Assert.That(info.StartTime, Is.EqualTo("2026-07-01 10:00:00"));
    }

    [Test]
    public void Post_event_anchors_to_last_round_window()
    {
        var now = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc); // after R3 end

        var info = ColosseumLobbyInfoBuilder.Build(SeasonOn(), ThreeRounds(), now);

        Assert.That(info.IsRoundPeriod, Is.False,
            "no active round once the bracket has ended");
        Assert.That(info.NextRound, Is.EqualTo(3),
            "anchored to last round so client doesn't NRE on the start_time/end_time read");
        Assert.That(info.EndTime, Is.EqualTo("2026-07-31 23:59:59"));
    }

    [Test]
    public void Empty_rounds_config_falls_back_to_event_window()
    {
        var now = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);

        var info = ColosseumLobbyInfoBuilder.Build(SeasonOn(), new ColosseumRoundsConfig(), now);

        Assert.That(info.IsRoundPeriod, Is.False);
        Assert.That(info.NowRound, Is.Null);
        Assert.That(info.StartTime, Is.EqualTo("2026-07-01 10:00:00"),
            "event window is the sensible fallback when no rounds are configured");
        Assert.That(info.EndTime, Is.EqualTo("2026-07-31 23:59:59"));
    }

    [Test]
    public void Surfaces_colosseum_id_from_season_id()
    {
        var info = ColosseumLobbyInfoBuilder.Build(
            SeasonOn(seasonId: 165), ThreeRounds(),
            new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc));

        Assert.That(info.ColosseumId, Is.EqualTo(165),
            "wire colosseum_id mirrors SeasonId so the client can disambiguate cups");
    }

    [Test]
    public void Surfaces_zero_season_id_as_absent()
    {
        var info = ColosseumLobbyInfoBuilder.Build(
            SeasonOn(seasonId: 0), ThreeRounds(),
            new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc));

        Assert.That(info.ColosseumId, Is.Null,
            "0 is the unconfigured default — strip so the field doesn't pollute the wire");
    }

    [Test]
    public void Surfaces_is_display_tips_and_tips_id()
    {
        var season = SeasonOn();
        season.IsDisplayTips = true;
        season.TipsId = 17;

        var info = ColosseumLobbyInfoBuilder.Build(season, ThreeRounds(),
            new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc));

        Assert.That(info.IsDisplayTips, Is.EqualTo(1),
            "wire int per ColosseumEntryInfoTask.cs:129 (NeedsFirstTips = == 1)");
        Assert.That(info.TipsId, Is.EqualTo(17));
    }

    [Test]
    public void Tips_default_off_with_zero_tips_id_stripped()
    {
        var info = ColosseumLobbyInfoBuilder.Build(SeasonOn(), ThreeRounds(),
            new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc));

        Assert.That(info.IsDisplayTips, Is.EqualTo(0),
            "tips off by default — wire int 0 (client maps to false)");
        Assert.That(info.TipsId, Is.Null);
    }
}
