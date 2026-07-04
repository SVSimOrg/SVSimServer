using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class PackConfigDto
{
    [JsonPropertyName("parent_gacha_id")]
    [Key("parent_gacha_id")]
    public int ParentGachaId { get; set; }

    [JsonPropertyName("base_pack_id")]
    [Key("base_pack_id")]
    public int BasePackId { get; set; }

    [JsonPropertyName("override_draw_effect_pack_id")]
    [Key("override_draw_effect_pack_id")]
    public int OverrideDrawEffectPackId { get; set; }

    [JsonPropertyName("override_ui_effect_pack_id")]
    [Key("override_ui_effect_pack_id")]
    public int OverrideUiEffectPackId { get; set; }

    [JsonPropertyName("gacha_type")]
    [Key("gacha_type")]
    public int GachaType { get; set; }

    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")]
    public int SleeveId { get; set; } = 3000011;

    [JsonPropertyName("special_sleeve_id")]
    [Key("special_sleeve_id")]
    public int SpecialSleeveId { get; set; }

    [JsonPropertyName("commence_date")]
    [Key("commence_date")]
    public string CommenceDate { get; set; } = string.Empty;

    [JsonPropertyName("complete_date")]
    [Key("complete_date")]
    public string CompleteDate { get; set; } = string.Empty;

    [JsonPropertyName("cardpack_banner_list")]
    [Key("cardpack_banner_list")]
    public List<PackBannerDto> CardpackBannerList { get; set; } = new();

    [JsonPropertyName("gacha_detail")]
    [Key("gacha_detail")]
    public string GachaDetail { get; set; } = string.Empty;

    [JsonPropertyName("child_gacha_info")]
    [Key("child_gacha_info")]
    public List<PackChildGachaDto> ChildGachaInfo { get; set; } = new();

    [JsonPropertyName("open_count")]
    [Key("open_count")]
    public int OpenCount { get; set; }

    [JsonPropertyName("open_count_limit")]
    [Key("open_count_limit")]
    public int OpenCountLimit { get; set; }

    [JsonPropertyName("is_hide")]
    [Key("is_hide")]
    public int IsHide { get; set; }

    [JsonPropertyName("pack_category")]
    [Key("pack_category")]
    public int PackCategory { get; set; }

    /// <summary>
    /// Null when the pack has no gacha-point participation. The key MUST be present on the wire
    /// (explicit null) — client at PackInfoTask.cs:126 does <c>if (jsonData2["gacha_point"] != null)</c>,
    /// a direct LitJson key access that throws KeyNotFoundException when the key is absent
    /// (only protects against null *value*, not missing *key*). Override the global
    /// WhenWritingNull per [[project_wire_null_policy]] memory.
    /// </summary>
    [JsonPropertyName("gacha_point")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("gacha_point")]
    public PackGachaPointDto? GachaPoint { get; set; }

    [JsonPropertyName("is_pre_release")]
    [Key("is_pre_release")]
    public bool IsPreRelease { get; set; }

    [JsonPropertyName("exists_purchase_reward")]
    [Key("exists_purchase_reward")]
    public bool ExistsPurchaseReward { get; set; }

    [JsonPropertyName("is_new")]
    [Key("is_new")]
    public bool IsNew { get; set; }

    /// <summary>
    /// Prod sends an object <c>{"sales_period_time":"..."}</c> when set and an array <c>[]</c>
    /// when unset. v1 always emits an empty object when the field is null on the entity —
    /// matches the active-window case and the client tolerates both shapes via
    /// <c>ShopExpirtyInfo</c>'s LitJson parser. Revisit if a capture proves otherwise.
    ///
    /// TODO(2026-05-28): the prod tutorial capture has each active pack with
    /// <c>"sales_period_info": {"sales_period_time": "&lt;complete_date&gt;"}</c> — i.e., the
    /// pack's <c>complete_date</c> echoed inside the object. Our controller emits <c>{}</c>
    /// which the client tolerates (the tutorial flow doesn't filter on this field), but for
    /// wire fidelity we should populate it from <c>PackConfigEntry.CompleteDate</c>. While
    /// doing that, also retype this field from <c>Dictionary&lt;string, string?&gt;</c> to a
    /// typed <c>PackSalesPeriodInfoDto { string SalesPeriodTime }</c> — the current dict
    /// shape is the lazy-key anti-pattern documented in
    /// <c>feedback_no_lazy_response_dicts</c>. Deferred from the tutorial-bringup pass
    /// because it doesn't gate any observable flow.
    /// </summary>
    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public Dictionary<string, string?> SalesPeriodInfo { get; set; } = new();

    [JsonPropertyName("poster_type")]
    [Key("poster_type")]
    public int PosterType { get; set; }

    /// <summary>
    /// The viewer's already-chosen class for a RotationStarterCardPack (1..8). Null when the
    /// viewer has not yet locked a choice via /pack/set_rotation_starter_class. Client at
    /// PackInfoTask.cs:86 reads via <c>TryGetValue</c>, so a missing key is safe (don't emit
    /// when null — global WhenWritingNull is sufficient here).
    /// </summary>
    [JsonPropertyName("selected_class_id")]
    [Key("selected_class_id")]
    public int? SelectedClassId { get; set; }
}
