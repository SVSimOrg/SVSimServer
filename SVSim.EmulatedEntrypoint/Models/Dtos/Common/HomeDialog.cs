using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// One entry in /mypage/index data.home_dialog_list. Client parser
/// (Wizard/MyPageHomeDialogData.cs) only reads [0]; up to 3 buttons supported
/// (switch on 0/1/2/3 in MyPageHomeDialog.cs).
/// </summary>
[MessagePackObject]
public class HomeDialog
{
    /// <summary>Wire "type" — prod sends "1"; client parser ignores it. Stringly-typed.
    /// Null is omitted by the global WhenWritingNull policy.</summary>
    [JsonPropertyName("type")]           [Key("type")]           public string? Type { get; set; }

    /// <summary>Localization key resolved client-side via Data.SystemText.Get.</summary>
    [JsonPropertyName("title_text_id")]  [Key("title_text_id")]  public string TitleTextId { get; set; } = string.Empty;

    /// <summary>Asset name resolved via ResourcesManager.AssetLoadPathType.UiDownLoad.</summary>
    [JsonPropertyName("image")]          [Key("image")]          public string Image { get; set; } = string.Empty;

    [JsonPropertyName("button_list")]    [Key("button_list")]    public List<HomeDialogButtonDto> ButtonList { get; set; } = new();
}

[MessagePackObject]
public class HomeDialogButtonDto
{
    [JsonPropertyName("button_text_id")] [Key("button_text_id")] public string ButtonTextId { get; set; } = string.Empty;

    /// <summary>Scene id consumed by MyPageBannerBase.SceneChangeBySetting (e.g. "card_pack", "mission").</summary>
    [JsonPropertyName("scene")]          [Key("scene")]          public string Scene { get; set; } = string.Empty;

    /// <summary>Contextual id passed to the scene (e.g. parent_gacha_id "80032"). Stringly-typed on the wire.</summary>
    [JsonPropertyName("status")]         [Key("status")]         public string Status { get; set; } = string.Empty;
}
