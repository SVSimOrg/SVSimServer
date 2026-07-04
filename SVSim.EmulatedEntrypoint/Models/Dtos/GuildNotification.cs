using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// guild_notification on /mypage/index. Consumed by
/// MyPageNotifications.GuildNotification.SetGuildNotification (GuildNotification.cs:30-38),
/// which reads guild_id / guild_room_message_id via `var x = json["guild_id"]; if (x != null) ...`
/// — the LitJson indexer throws KeyNotFoundException on a missing key, so these
/// must reach the client as explicit nulls when there's no guild. Override the
/// global WhenWritingNull so they survive serialization. Prod's wire matches:
/// `"guild_notification":{"guild_id":null,"guild_room_message_id":null,...}`.
/// See [[project-wire-null-policy]] for the broader pattern.
/// </summary>
[MessagePackObject]
public class GuildNotification
{
    [JsonPropertyName("guild_id")]
    [Key("guild_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long? GuildId { get; set; }

    [JsonPropertyName("guild_room_message_id")]
    [Key("guild_room_message_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long? GuildRoomMessageId { get; set; }

    [JsonPropertyName("is_join_request")]
    [Key("is_join_request")]
    public bool IsJoinRequest { get; set; }

    [JsonPropertyName("is_invited")]
    [Key("is_invited")]
    public bool IsInvited { get; set; }
}
