using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// shop_notification on /mypage/index, consumed by
/// ShopNotification.SetShopNotification (Wizard/ShopNotification.cs:30). All four
/// sub-keys are directly indexed; each is passed to ShopAppealInfo, which early-
/// returns when `data.Count == 0`. We mirror prod's heterogeneous shape — three
/// empty arrays and one object — so that the wire matches and the client's
/// length-check fires on the empty cases.
/// </summary>
[MessagePackObject]
public class ShopNotification
{
    [JsonPropertyName("card_pack")]
    [Key("card_pack")]
    public ShopCardPackAppeal CardPack { get; set; } = new();

    /// <summary>Prod 2026-05-23: <c>[]</c>. Client treats Count==0 as "no notification".</summary>
    [JsonPropertyName("build_deck")]
    [Key("build_deck")]
    public List<object> BuildDeck { get; set; } = new();

    /// <summary>Prod 2026-05-23: <c>[]</c>.</summary>
    [JsonPropertyName("sleeve")]
    [Key("sleeve")]
    public List<object> Sleeve { get; set; } = new();

    /// <summary>Prod 2026-05-23: <c>[]</c>.</summary>
    [JsonPropertyName("leader_skin")]
    [Key("leader_skin")]
    public List<object> LeaderSkin { get; set; } = new();
}

/// <summary>
/// card_pack sub-object — drives the free-gacha campaign badge. Both fields are
/// TryGetValue on the client; emitting both as false matches prod's idle shape.
/// </summary>
[MessagePackObject]
public class ShopCardPackAppeal
{
    [JsonPropertyName("is_open_free_gacha_campaign")]
    [Key("is_open_free_gacha_campaign")]
    public bool IsOpenFreeGachaCampaign { get; set; }

    [JsonPropertyName("can_free_gacha")]
    [Key("can_free_gacha")]
    public bool CanFreeGacha { get; set; }
}
