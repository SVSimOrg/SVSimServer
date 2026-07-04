using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Per-link entry in <c>transition_account_data</c>. Production sends three string fields per
/// entry even though <c>GameStartCheckTask.Parse</c> only reads <c>social_account_type</c>.
/// The extra two are read by adjacent tasks (<c>GetGameDataByTransitionCode</c>,
/// <c>GetGameDataBySocialAccountTask</c>) 窶・kept here so the wire matches prod regardless of
/// which task ends up consuming the payload.
/// </summary>
[MessagePackObject]
public class TransitionAccountData
{
    /// <summary>
    /// The social provider's account id (e.g. SteamID as a string). Sent as string on the wire.
    /// </summary>
    [JsonPropertyName("social_account_id")]
    [Key("social_account_id")]
    public string? SocialAccountId { get; set; }

    /// <summary>
    /// <c>Cute/CuteNetworkDefine.ACCOUNT_TYPE</c> enum, **sent as string** on the wire even
    /// though it's numeric. <c>GameStartCheckTask.Parse</c> calls <c>.ToInt()</c> on it so
    /// LitJson coerces transparently 窶・but matching prod's string form makes us safer against
    /// future client paths that might compare it as a literal.
    /// 1=GooglePlay, 2=GameCenter, 3=Facebook, 4=DMM, 5=Steam, 6=AppleID.
    /// </summary>
    [JsonPropertyName("social_account_type")]
    [Key("social_account_type")]
    public string? SocialAccountType { get; set; }

    /// <summary>
    /// The viewer id this social connection is linked to. Sent as string.
    /// </summary>
    [JsonPropertyName("connected_viewer_id")]
    [Key("connected_viewer_id")]
    public string? ConnectedViewerId { get; set; }
}
