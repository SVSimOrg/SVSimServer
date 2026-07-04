using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.UserMyPage;

/// <summary>
/// Body of <c>POST /user_mypage/update</c>. Client task: <c>MyPageSettingUpdateTask</c>
/// (Shadowverse_Code_2026-05-23/Wizard/MyPageSettingUpdateTask.cs). Note that
/// <c>select_type</c> is the only int on the wire — id fields are strings. Does not inherit
/// BaseRequest: the translation middleware stashes the auth tuple into HttpContext.Items
/// before the typed DTO deserialize, so the Steam handler reads them from there.
/// </summary>
[MessagePackObject]
public sealed class UserMyPageUpdateRequest
{
    /// <summary>BGType enum: 0=Deck, 1=CustomBG, 2=RandomBG. Client sends as an int.</summary>
    [JsonPropertyName("select_type")]
    [Key("select_type")]
    public int SelectType { get; set; }

    /// <summary>Chosen BG id when SelectType=CustomBG; empty or "0" otherwise.</summary>
    [JsonPropertyName("mypage_id")]
    [Key("mypage_id")]
    public string MyPageId { get; set; } = "0";

    /// <summary>Saved rotation pool, in slot order; client sends the full list on every call.</summary>
    [JsonPropertyName("mypage_id_list")]
    [Key("mypage_id_list")]
    public List<string> MyPageIdList { get; set; } = new();
}
