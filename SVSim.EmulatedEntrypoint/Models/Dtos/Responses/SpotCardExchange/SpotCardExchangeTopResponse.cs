using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.SpotCardExchange;

/// <summary>
/// /spot_card_exchange/top response.
/// <para>
/// <c>exchangeable_card_list</c> is an array of exactly 9 entries indexed by clan id 0..8.
/// Each entry is a dict keyed by an arbitrary stringified int (prod always emits "1") whose
/// value is the array of cards for that clan. The client iterates by clan index then dict-keys
/// (LitJson positional iteration).
/// </para>
/// <para>
/// <c>pre_relase_info</c> — WIRE TYPO PRESERVED ("relase" not "release"). Renaming this field
/// breaks the client's <c>jsonData["pre_relase_info"]</c> access.
/// </para>
/// </summary>
[MessagePackObject]
public class SpotCardExchangeTopResponse
{
    [JsonPropertyName("spot_point")]
    [Key("spot_point")]
    public int SpotPoint { get; set; }

    [JsonPropertyName("exchangeable_card_list")]
    [Key("exchangeable_card_list")]
    public List<Dictionary<string, List<SpotCardExchangeCardDto>>> ExchangeableCardList { get; set; } = new();

    /// <summary>Card set id about to cycle out of spot-card eligibility — drives "last chance!" UI.
    /// Empty string in the captured response. Stays string-typed because the client uses
    /// <c>int.TryParse</c>.</summary>
    [JsonPropertyName("soon_cycle_out_card_set_id")]
    [Key("soon_cycle_out_card_set_id")]
    public string SoonCycleOutCardSetId { get; set; } = string.Empty;

    [JsonPropertyName("pre_relase_info")]
    [Key("pre_relase_info")]
    public PreReleaseInfoDto PreReleaseInfo { get; set; } = new();
}

[MessagePackObject]
public class SpotCardExchangeCardDto
{
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public long CardId { get; set; }

    /// <summary>SpotCardExchangeInfo.ExchangeStatus — 0=EnableExchange, 1=AlreadyExchange, 2=LimitOver.</summary>
    [JsonPropertyName("exchange_status")]
    [Key("exchange_status")]
    public int ExchangeStatus { get; set; }

    /// <summary>Stringified price — prod ships e.g. "3500", client reads via .ToInt().</summary>
    [JsonPropertyName("exchange_point")]
    [Key("exchange_point")]
    public string ExchangePoint { get; set; } = "0";

    /// <summary>Stringified clan id. Prod ships "0".."8".</summary>
    [JsonPropertyName("class")]
    [Key("class")]
    public string Class { get; set; } = "0";

    [JsonPropertyName("is_pre_release")]
    [Key("is_pre_release")]
    public bool IsPreRelease { get; set; }

    /// <summary>Stringified card_set_id this card belongs to.</summary>
    [JsonPropertyName("ts_rotation_id")]
    [Key("ts_rotation_id")]
    public string TsRotationId { get; set; } = "0";
}

[MessagePackObject]
public class PreReleaseInfoDto
{
    [JsonPropertyName("is_pre_release")]
    [Key("is_pre_release")]
    public bool IsPreRelease { get; set; }

    [JsonPropertyName("pre_release_spot_card_exchange_count")]
    [Key("pre_release_spot_card_exchange_count")]
    public int PreReleaseSpotCardExchangeCount { get; set; }

    [JsonPropertyName("pre_release_spot_card_exchange_limit")]
    [Key("pre_release_spot_card_exchange_limit")]
    public int PreReleaseSpotCardExchangeLimit { get; set; }
}
