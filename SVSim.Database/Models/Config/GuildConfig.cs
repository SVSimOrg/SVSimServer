using System.Collections.Generic;
using System.Linq;

namespace SVSim.Database.Models.Config;

/// <summary>
/// Guild-feature configuration: membership caps, search caps, stamp palette,
/// and chat-polling intervals. All collection defaults live in
/// <see cref="ShippedDefaults"/> (not property initializers) to survive the
/// GameConfigService tier-merge correctly.
/// </summary>
[ConfigSection("Guild")]
public sealed class GuildConfig
{
    public int MaxMemberNum { get; init; } = 30;
    public int MaxSubLeaderNum { get; init; } = 2;
    public int SearchResultCap { get; init; } = 50;
    public int ChatPollIdleSeconds { get; init; } = 10;
    public int ChatPollActiveSeconds { get; init; } = 3;
    public List<int> UsableStampList { get; init; } = new();

    public static GuildConfig ShippedDefaults() => new()
    {
        UsableStampList = Enumerable.Range(100001, 20).ToList(),
    };
}
