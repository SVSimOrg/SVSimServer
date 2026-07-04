using System.Globalization;
using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserInfo
{
    /// <summary>Wire format prod uses for the two datetime fields here. No 'T', no fractions, no zone.</summary>
    private const string ProdDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    [JsonPropertyName("device_type")]
    [Key("device_type")]
    public int DeviceType { get; set; }
    [JsonPropertyName("name")]
    [Key("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("country_code")]
    [Key("country_code")]
    public string CountryCode { get; set; } = string.Empty;
    [JsonPropertyName("max_friend")]
    [Key("max_friend")]
    public int MaxFriend { get; set; }
    /// <summary>
    /// Wire format <c>"yyyy-MM-dd HH:mm:ss"</c> (space-separated, no 'T', no Z, no fractions).
    /// Null for fresh accounts that have never played — prod omits/nulls this rather than
    /// emitting <c>DateTime.MinValue</c> with .NET's default ISO-8601-with-Z serialization,
    /// which can crash the client's DateTime parser.
    /// </summary>
    [JsonPropertyName("last_play_time")]
    [Key("last_play_time")]
    public string? LastPlayTime { get; set; }
    [JsonPropertyName("is_received_two_pick_mission")]
    [Key("is_received_two_pick_mission")]
    public int HasReceivedPickTwoMission { get; set; }
    /// <summary>
    /// Birth date as yyyy-MM-dd. Parser does .ToString() on this field (LoadDetail.cs:203).
    /// Format verified against live capture pending.
    /// </summary>
    [JsonPropertyName("birth")]
    [Key("birth")]
    public string Birthday { get; set; } = string.Empty;
    [JsonPropertyName("selected_emblem_id")]
    [Key("selected_emblem_id")]
    public long SelectedEmblemId { get; set; }
    [JsonPropertyName("selected_degree_id")]
    [Key("selected_degree_id")]
    public int SelectedDegreeId { get; set; }
    /// <summary>Same format/null rules as <see cref="LastPlayTime"/>.</summary>
    [JsonPropertyName("mission_change_time")]
    [Key("mission_change_time")]
    public string? MissionChangeTime { get; set; }
    [JsonPropertyName("mission_receive_type")]
    [Key("mission_receive_type")]
    public int MissionReceiveType { get; set; }
    [JsonPropertyName("is_official")]
    [Key("is_official")]
    public int IsOfficial { get; set; }
    [JsonPropertyName("is_official_mark_displayed")]
    [Key("is_official_mark_displayed")]
    public int IsOfficialMarkDisplayed { get; set; }

    public UserInfo()
    {
    }

    public UserInfo(int deviceType, Viewer viewer)
    {
        this.DeviceType = deviceType;
        this.Name = viewer.DisplayName;
        this.CountryCode = viewer.Info.CountryCode;
        this.MaxFriend = viewer.Info.MaxFriends;
        this.LastPlayTime = FormatProdDateTime(viewer.LastLogin);
        this.HasReceivedPickTwoMission = viewer.MissionData.HasReceivedPickTwoMission ? 1 : 0;
        this.Birthday = viewer.Info.BirthDate.ToString("yyyy-MM-dd");
        this.SelectedEmblemId = viewer.Info.SelectedEmblem.Id;
        this.SelectedDegreeId = viewer.Info.SelectedDegree.Id;
        this.MissionChangeTime = FormatProdDateTime(viewer.MissionData.MissionChangeTime);
        this.MissionReceiveType = viewer.MissionData.MissionReceiveType;
        this.IsOfficial = viewer.Info.IsOfficial ? 1 : 0;
        this.IsOfficialMarkDisplayed = viewer.Info.IsOfficialMarkDisplayed ? 1 : 0;
    }

    private static string? FormatProdDateTime(DateTime dt)
        => dt == default ? null : dt.ToString(ProdDateTimeFormat, CultureInfo.InvariantCulture);
}
