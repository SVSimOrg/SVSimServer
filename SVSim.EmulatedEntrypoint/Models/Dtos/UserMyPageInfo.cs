using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// user_mypage_info — wrapper around the active home-screen background
/// configuration. Client constructs MyPageBGInfo(user_mypage_setting) at
/// MyPageTask.cs:176.
/// </summary>
[MessagePackObject]
public class UserMyPageInfo
{
    [JsonPropertyName("user_mypage_setting")]
    [Key("user_mypage_setting")]
    public MyPageBgSetting UserMyPageSetting { get; set; } = new();
}

/// <summary>
/// Active mypage background selection. Shape from prod 2026-06-09 (capture line 12/56).
/// All three fields ship as strings on the wire even though the underlying ids are integers.
/// </summary>
[MessagePackObject]
public class MyPageBgSetting
{
    [JsonPropertyName("mypage_id")]
    [Key("mypage_id")]
    public string MyPageId { get; set; } = "0";

    /// <summary>BGType enum as decimal string: "0"=Deck, "1"=CustomBG, "2"=RandomBG.</summary>
    [JsonPropertyName("select_type")]
    [Key("select_type")]
    public string SelectType { get; set; } = "0";

    [JsonPropertyName("mypage_id_list")]
    [Key("mypage_id_list")]
    public List<string> MyPageIdList { get; set; } = new();
}
