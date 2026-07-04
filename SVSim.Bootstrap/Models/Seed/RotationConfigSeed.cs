using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors <c>seeds/rotation-config.json</c>. Drives the Rotation <c>GameConfigSection</c>.
/// Note: <c>rotation_card_set_ids</c> is the rotation CardSet flag list — consumed by
/// RotationFlagUpdater in Stage 9C, not by RotationConfigImporter.
/// </summary>
public sealed class RotationConfigSeed
{
    [JsonPropertyName("ts_rotation_id")] public string TsRotationId { get; set; } = "";
    [JsonPropertyName("is_battle_pass_period")] public bool IsBattlePassPeriod { get; set; }
    [JsonPropertyName("is_beginner_mission")] public bool IsBeginnerMission { get; set; }
    [JsonPropertyName("card_set_id_for_resource_dl_view")] public int CardSetIdForResourceDlView { get; set; }
    [JsonPropertyName("rotation_card_set_ids")] public List<int> RotationCardSetIds { get; set; } = new();
}

/// <summary>Mirrors <c>seeds/challenge-config.json</c>. Drives the Challenge <c>GameConfigSection</c>.</summary>
public sealed class ChallengeConfigSeed
{
    [JsonPropertyName("last_card_pack_set_id")] public int LastCardPackSetId { get; set; }
    [JsonPropertyName("card_pool_name")] public string CardPoolName { get; set; } = "";
    [JsonPropertyName("card_pool_url")] public string CardPoolUrl  { get; set; } = "";
    [JsonPropertyName("announce_id")] public string AnnounceId   { get; set; } = "";
    [JsonPropertyName("start_time")] public string StartTime    { get; set; } = "";
    [JsonPropertyName("end_time")] public string EndTime      { get; set; } = "";
    [JsonPropertyName("two_pick_type")] public int TwoPickType     { get; set; } = 0;
    [JsonPropertyName("strategy_pick_num")] public int StrategyPickNum { get; set; } = 0;
    [JsonPropertyName("pool_card_set_ids")] public List<int> PoolCardSetIds { get; set; } = new();
}

/// <summary>
/// Mirrors <c>seeds/my-rotation-schedule.json</c>. Drives the MyRotationSchedule
/// <c>GameConfigSection</c>. The extractor pre-joins <c>gathering</c> and <c>free_battle</c>
/// from <c>my_rotation_info.schedules</c> into two top-level fields.
/// </summary>
public sealed class MyRotationScheduleSeed
{
    [JsonPropertyName("gathering")] public ScheduleWindowSeed? Gathering { get; set; }
    [JsonPropertyName("free_battle")] public ScheduleWindowSeed? FreeBattle { get; set; }
}

public sealed class ScheduleWindowSeed
{
    [JsonPropertyName("begin")] public string Begin { get; set; } = "";
    [JsonPropertyName("end")] public string End { get; set; } = "";
}
