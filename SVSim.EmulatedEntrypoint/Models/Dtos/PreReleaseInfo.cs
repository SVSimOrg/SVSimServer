using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Pre-release window for an upcoming card set. Wire shape derived from 2026-05-23 prod capture:
/// most numeric fields arrive as quoted strings (prod's PHP backend convention) — only the truly
/// integer fields (card_master_id, statuses) are JSON numbers. Client parses with .ToInt()/.ToString()
/// so either works on read, but matching prod is the right baseline.
///
/// Optional in /load/index — see Prerelease.Create for parser-side handling.
/// </summary>
[MessagePackObject]
public class PreReleaseInfo
{
    [JsonPropertyName("id")]
    [Key("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    [Key("start_time")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("display_end_time")]
    [Key("display_end_time")]
    public DateTime DisplayEndTime { get; set; }

    [JsonPropertyName("next_card_set_id")]
    [Key("next_card_set_id")]
    public string NextCardSetId { get; set; } = string.Empty;

    [JsonPropertyName("default_card_master_id")]
    [Key("default_card_master_id")]
    public string DefaultCardMasterId { get; set; } = string.Empty;

    [JsonPropertyName("pre_release_card_master_id")]
    [Key("pre_release_card_master_id")]
    public string PreReleaseCardMasterId { get; set; } = string.Empty;

    [JsonPropertyName("free_match_start_time")]
    [Key("free_match_start_time")]
    public DateTime FreeMatchStartTime { get; set; }

    [JsonPropertyName("card_master_id")]
    [Key("card_master_id")]
    public int CardMasterId { get; set; }

    [JsonPropertyName("rotation_card_set_id_list")]
    [Key("rotation_card_set_id_list")]
    public List<int> RotationCardSets { get; set; } = new();

    /// <summary>
    /// Prod sends a dict of card_id (string) → card_id (string) — values mirror keys. The
    /// purpose is just to enumerate which base card ids count as reprinted in this window.
    /// </summary>
    [JsonPropertyName("reprinted_base_card_ids")]
    [Key("reprinted_base_card_ids")]
    public Dictionary<string, string> ReprintedCardIds { get; set; } = new();

    [JsonPropertyName("latest_reprinted_base_card_ids")]
    [Key("latest_reprinted_base_card_ids")]
    public List<int> LatestReprintedCardIds { get; set; } = new();

    [JsonPropertyName("pre_release_status")]
    [Key("pre_release_status")]
    public int PreReleaseStatus { get; set; }

    [JsonPropertyName("is_pre_rotation_free_match_term")]
    [Key("is_pre_rotation_free_match_term")]
    public int IsPreRotationFreeMatchTerm { get; set; }
}
