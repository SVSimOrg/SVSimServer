using System.Globalization;
using SVSim.Database.Models.Config;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Single source of truth for projecting <see cref="ColosseumSeasonConfig"/> +
/// <see cref="ColosseumRoundsConfig"/> onto the wire <see cref="ColosseumLobbyInfo"/> block.
/// Used by both <c>/arena_colosseum/{top,get_fee_info}</c> and
/// <c>/mypage/index data.colosseum_info</c> — the home-screen tab and the lobby reads must
/// agree on the season state, so they go through one builder.
/// <para>
/// When <c>IsColosseumPeriod = false</c> a minimal "no event" payload is emitted; the client
/// gates every other field on that flag (<c>Wizard/ColosseumEntryInfoTask.cs:100</c>).
/// </para>
/// <para>
/// Round-period derivation: <see cref="ColosseumRoundsConfig.Rounds"/> are walked against
/// <paramref name="nowUtc"/>. If a round's window covers <paramref name="nowUtc"/>, we emit
/// <c>is_round_period = true</c> + that round's window as <c>start_time/end_time</c> +
/// <c>now_round</c>. Otherwise we emit <c>is_round_period = false</c> + the NEXT round's
/// start (or the event start if no rounds are configured) so the client's
/// "Round X opens in HH:MM:SS" countdown points somewhere sensible. Client handles both
/// branches at <c>ColosseumEntryInfoTask.cs:111-126</c>.
/// </para>
/// </summary>
public static class ColosseumLobbyInfoBuilder
{
    private const string WireDateFormat = "yyyy-MM-dd HH:mm:ss";

    public static ColosseumLobbyInfo Build(
        ColosseumSeasonConfig season,
        ColosseumRoundsConfig rounds,
        DateTime nowUtc)
    {
        if (!season.IsColosseumPeriod)
        {
            return new ColosseumLobbyInfo { IsColosseumPeriod = false };
        }

        var (isRoundPeriod, nowRoundId, nextRoundId, windowStart, windowEnd) =
            ResolveRoundWindow(season, rounds, nowUtc);

        return new ColosseumLobbyInfo
        {
            IsColosseumPeriod = true,
            // Wire colosseum_id — surface SeasonId so client can disambiguate cups.
            // Falls back to absent when no season identity is configured.
            ColosseumId = season.SeasonId == 0 ? null : season.SeasonId,
            DeckFormat = (int)season.DeckFormat,
            IsNormalTwoPick = season.IsNormalTwoPick ? "1" : "0",
            ColosseumName = season.ColosseumName,
            IsRoundPeriod = isRoundPeriod,
            IsSpecialMode = season.IsSpecialMode,
            CardPoolName = string.IsNullOrEmpty(season.CardPoolName) ? null : season.CardPoolName,
            NowRound = isRoundPeriod ? nowRoundId : null,
            NextRound = isRoundPeriod ? null : nextRoundId,
            StartTime = FormatTime(windowStart),
            EndTime = FormatTime(windowEnd),
            // Tutorial tip toggle — wire ints 0/1 per ColosseumEntryInfoTask.cs:129.
            IsDisplayTips = season.IsDisplayTips ? 1 : 0,
            TipsId = season.TipsId == 0 ? null : season.TipsId,
            IsAllCardEnabled = season.IsAllCardEnabled ? 1 : 0,
            SalesPeriodInfo = new ColosseumSalesPeriodInfo
            {
                SalesPeriodTime = FormatTime(season.SalesPeriodEnd),
            },
            StrategyPickNum = season.StrategyPickNum > 0 ? season.StrategyPickNum : null,
        };
    }

    /// <summary>
    /// Walks <see cref="ColosseumRoundsConfig.Rounds"/> (ordered by RoundId) against
    /// <paramref name="nowUtc"/> and returns the round window to emit on the wire.
    /// <list type="bullet">
    ///   <item>Inside a round → <c>(true, round.RoundId, null, round.StartTime, round.EndTime)</c></item>
    ///   <item>Before any round → <c>(false, null, firstRound.RoundId, firstRound.StartTime, firstRound.EndTime)</c></item>
    ///   <item>Between two rounds → <c>(false, null, nextRound.RoundId, nextRound.StartTime, nextRound.EndTime)</c></item>
    ///   <item>After last round → <c>(false, null, lastRound.RoundId, lastRound.StartTime, lastRound.EndTime)</c>
    ///         — client renders "ended" UI; keeps the field non-null so the parser doesn't NRE.</item>
    ///   <item>No rounds configured → fall back to the event-level start/end window. Keeps the
    ///         wire well-formed even when rounds haven't been seeded yet.</item>
    /// </list>
    /// </summary>
    private static (bool IsRoundPeriod, int? NowRoundId, int? NextRoundId, DateTime Start, DateTime End)
        ResolveRoundWindow(ColosseumSeasonConfig season, ColosseumRoundsConfig rounds, DateTime nowUtc)
    {
        if (rounds.Rounds.Count == 0)
        {
            return (IsRoundPeriod: false, NowRoundId: null, NextRoundId: null,
                    Start: season.EventStartTime, End: season.EventEndTime);
        }

        var ordered = rounds.Rounds.OrderBy(r => r.RoundId).ToList();

        var current = ordered.FirstOrDefault(r => nowUtc >= r.StartTime && nowUtc <= r.EndTime);
        if (current is not null)
        {
            return (IsRoundPeriod: true, NowRoundId: current.RoundId, NextRoundId: null,
                    Start: current.StartTime, End: current.EndTime);
        }

        var upcoming = ordered.FirstOrDefault(r => r.StartTime > nowUtc);
        if (upcoming is not null)
        {
            return (IsRoundPeriod: false, NowRoundId: null, NextRoundId: upcoming.RoundId,
                    Start: upcoming.StartTime, End: upcoming.EndTime);
        }

        // Past last round — anchor to the last round's window so the wire stays well-formed.
        var last = ordered[^1];
        return (IsRoundPeriod: false, NowRoundId: null, NextRoundId: last.RoundId,
                Start: last.StartTime, End: last.EndTime);
    }

    private static string FormatTime(DateTime t) =>
        t == default ? "" : t.ToString(WireDateFormat, CultureInfo.InvariantCulture);
}
