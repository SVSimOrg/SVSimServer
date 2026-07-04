using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>
/// Response for POST /guild_chat/replay_detail.
///
/// Wire shape: the stored ReplayPayload JSON is forwarded FLAT as the entire data payload.
/// ChatReplayDetailTask.Parse() calls new ReplayDetailInfo(base.ResponseData["data"]) which
/// then accesses data["battleId"], data["seed"], data["vid1"], etc. directly (no Keys.Contains
/// guards on required fields). Wrapping the payload under a "replay_info" key would crash the
/// client. The controller returns the raw JsonElement directly via Ok(element).
///
/// C1 decision: Option (b) is superseded — client CAN handle a full payload; the bug was the
/// wrapper key. The flat pass-through here is Option (a) variant: emit stored fields at data level.
/// If GetReplayDetailAsync returns null (no stored replay), the controller emits result_code=2
/// to gate the client off unguarded parsing.
/// </summary>
// NOTE: This DTO is unused at runtime — the controller returns ActionResult directly. It is kept
// as documentation of the wire contract and for future typed deserialization tests.
public class GuildChatReplayDetailResponse
{
    // Fields are emitted flat at the data level, not under a wrapper key.
    // See ReplayDetailInfo.cs in the decompiled client for the full field list.
    // Key representative required fields (unguarded access in constructor):
    //   battleId, seed, fieldId, firstTurn, card_master_id,
    //   vid1/name1/charaId1/classId1/emblemId1/degreeId1/countryCode1/sleeveId1/battlePoint1/masterPoint1/rank1/isOfficial1/deck1
    //   vid2/name2/charaId2/classId2/emblemId2/degreeId2/countryCode2/sleeveId2/battlePoint2/masterPoint2/rank2/isOfficial2/deck2

    [JsonPropertyName("result_code")]
    public int ResultCode { get; set; } = 1;
}
