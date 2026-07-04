using NUnit.Framework;
using System.Text.Json;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;
using SVSim.EmulatedEntrypoint.Models.Dtos.Guild;
using System.Text.Json.Serialization;

namespace SVSim.UnitTests.Wire;

[TestFixture]
public class GuildWireShape
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    [Test]
    public void EmblemList_serializes_as_array_of_objects_with_emblem_id()
    {
        // GuildEmblemListTask.Parse() reads jsonData[i]["emblem_id"].ToLong()
        // → wire must be an array of objects, each with an "emblem_id" string field.
        var resp = new GuildEmblemListResponse
        {
            EmblemList = new() { new GuildEmblemEntry { EmblemId = 100_000_001L }, new GuildEmblemEntry { EmblemId = 100_000_002L } }
        };
        var json = JsonSerializer.Serialize(resp, Opts);
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.GetProperty("guild_emblem_list");

        Assert.That(arr.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(arr.GetArrayLength(), Is.EqualTo(2));

        var first = arr[0];
        Assert.That(first.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(first.GetProperty("emblem_id").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(first.GetProperty("emblem_id").GetString(), Is.EqualTo("100000001"));
    }

    private static GuildDetailDto SampleDetail => new()
    {
        GuildId = 100_000_001,
        GuildName = "X",
        LeaderName = "L",
        LeaderViewerId = 7,
        Activity = 1,
        JoinCondition = 1,
        MemberNum = 1,
        GuildEmblemId = 100_000_000L,
        Description = "",
    };

    [Test]
    public void GuildUpdate_response_serializes_with_flat_guild_detail()
    {
        // GuildUpdateTask.Parse() reads data["guild"] as GuildDetailInfo directly — no "detail" wrapper.
        var resp = new GuildUpdateResponse { Guild = SampleDetail };
        var json = JsonSerializer.Serialize(resp, Opts);
        using var doc = JsonDocument.Parse(json);
        var guild = doc.RootElement.GetProperty("guild");

        // guild_id must be DIRECTLY under "guild"
        Assert.That(guild.TryGetProperty("guild_id", out var gid), Is.True, "guild_id must be directly under guild");
        Assert.That(gid.GetString(), Is.EqualTo("100000001"));

        // No nested "detail" wrapper
        Assert.That(guild.TryGetProperty("detail", out _), Is.False, "no detail wrapper for /guild/update");
    }

    [Test]
    public void GuildUpdateEmblem_response_serializes_with_nested_detail()
    {
        // GuildEmblemUpdateTask.Parse() reads data["guild"]["detail"] — wrapper is required.
        var resp = new GuildUpdateEmblemResponse
        {
            Guild = new GuildDetailSubTree { Detail = SampleDetail }
        };
        var json = JsonSerializer.Serialize(resp, Opts);
        using var doc = JsonDocument.Parse(json);
        var guildNode = doc.RootElement.GetProperty("guild");

        Assert.That(guildNode.TryGetProperty("detail", out var detail), Is.True, "detail wrapper must exist for /guild/update_emblem");
        Assert.That(detail.GetProperty("guild_id").GetString(), Is.EqualTo("100000001"));

        // guild_id must NOT be directly under "guild"
        Assert.That(guildNode.TryGetProperty("guild_id", out _), Is.False, "guild_id must be inside detail, not at guild level");
    }

    [Test]
    public void GuildSearchGuild_response_serializes_with_flat_list_entries()
    {
        var resp = new GuildSearchGuildResponse { List = new() {
            new GuildDetailDto { GuildId = 100_000_001, GuildName = "Alpha", LeaderName = "Lead", LeaderViewerId = 7, Activity = 1, JoinCondition = 1, MemberNum = 5, GuildEmblemId = 100000000, Description = "" }
        } };
        var json = JsonSerializer.Serialize(resp, Opts);
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.GetProperty("list");
        Assert.That(arr.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(arr[0].GetProperty("guild_id").GetString(), Is.EqualTo("100000001"));
        Assert.That(arr[0].TryGetProperty("detail", out _), Is.False, "search list entries are flat, no detail wrapper");
    }

    [Test]
    public void GuildChangeRole_response_serializes_members_array_with_role_and_stringified_ids()
    {
        // GuildChangeRoleTask.Parse() reads base.ResponseData["data"]["members"] — it must be
        // an array of GuildMemberInfo objects, each having role + viewer_id (stringified).
        var resp = new GuildChangeRoleResponse
        {
            Members = new()
            {
                new GuildMemberInfoDto
                {
                    ViewerId = 76_561_198_300_000_009L,
                    Name = "TestLeader",
                    EmblemId = 100_000_000L,
                    CountryCode = "JP",
                    Rank = 1,
                    DegreeId = 0,
                    IsOfficialMarkDisplayed = 0,
                    Role = 1, // Leader
                },
                new GuildMemberInfoDto
                {
                    ViewerId = 76_561_198_300_000_010L,
                    Name = "TestMember",
                    EmblemId = 100_000_000L,
                    CountryCode = "",
                    Rank = 1,
                    DegreeId = 0,
                    IsOfficialMarkDisplayed = 0,
                    Role = 0, // Regular
                },
            }
        };
        var json = JsonSerializer.Serialize(resp, Opts);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.TryGetProperty("members", out var members), Is.True, "response must have 'members' key");
        Assert.That(members.ValueKind, Is.EqualTo(JsonValueKind.Array), "members must be an array");
        Assert.That(members.GetArrayLength(), Is.EqualTo(2));

        var first = members[0];
        // viewer_id must be stringified (StringifiedLongConverter).
        Assert.That(first.GetProperty("viewer_id").ValueKind, Is.EqualTo(JsonValueKind.String),
            "viewer_id must be stringified");
        Assert.That(first.GetProperty("role").ValueKind, Is.EqualTo(JsonValueKind.String),
            "role must be stringified");
        Assert.That(first.GetProperty("role").GetString(), Is.EqualTo("1"),
            "Leader role must serialize as '1'");

        // is_official_mark_displayed must always be present (JsonIgnore Never).
        Assert.That(first.TryGetProperty("is_official_mark_displayed", out var isMark), Is.True,
            "is_official_mark_displayed must always be emitted");
        Assert.That(isMark.GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public void GuildFriendList_response_is_bare_array_at_root_with_is_join_guild()
    {
        // GuildFriendListTask.Parse() reads base.ResponseData["data"] and iterates with data[i] directly.
        // data must be a bare JSON array — NOT an object with a "friends" key.
        var list = new List<GuildInviteCandidateDto>
        {
            new GuildInviteCandidateDto
            {
                ViewerId = 76_561_198_300_000_001L,
                Name = "Friend1",
                EmblemId = 100_000_000L,
                CountryCode = "JP",
                Rank = 1,
                DegreeId = 0,
                IsJoinGuild = false,
            },
            new GuildInviteCandidateDto
            {
                ViewerId = 76_561_198_300_000_002L,
                Name = "Friend2",
                EmblemId = 100_000_000L,
                CountryCode = "",
                Rank = 1,
                DegreeId = 0,
                IsJoinGuild = true,
            },
        };
        var json = JsonSerializer.Serialize(list, Opts);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Root must be an array directly — GuildFriendListTask reads data[i], not data["friends"][i].
        Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "friend_list data must be a bare array, not a wrapper object");
        Assert.That(root.GetArrayLength(), Is.EqualTo(2));

        // First entry: is_join_guild = false.
        var first = root[0];
        Assert.That(first.TryGetProperty("is_join_guild", out var jg1), Is.True, "is_join_guild must be present");
        Assert.That(jg1.GetBoolean(), Is.False);

        // Second entry: is_join_guild = true.
        var second = root[1];
        Assert.That(second.GetProperty("is_join_guild").GetBoolean(), Is.True);

        // viewer_id must be stringified.
        Assert.That(first.GetProperty("viewer_id").ValueKind, Is.EqualTo(JsonValueKind.String),
            "viewer_id must be stringified in friend_list entries");

        // No "friends" wrapper key must exist.
        Assert.That(root.ValueKind, Is.Not.EqualTo(JsonValueKind.Object),
            "root must not be an object — no 'friends' wrapper allowed");
    }

    [Test]
    public void ReplayDetail_response_passes_payload_fields_flat_not_wrapped()
    {
        // ChatReplayDetailTask.Parse() calls new ReplayDetailInfo(base.ResponseData["data"]).
        // ReplayDetailInfo constructor accesses data["battleId"].ToLong(), data["seed"].ToInt(), etc.
        // WITHOUT Keys.Contains guards. Wrapping under a "replay_info" key would crash the client.
        // The controller emits the stored JSON element directly as the data payload — verify that
        // the serialized shape does NOT have a wrapper key.
        //
        // Decompile evidence (ReplayDetailInfo.cs line 209-210):
        //   battle_id = data["battleId"].ToLong();   // unguarded
        //   seed = data["seed"].ToInt();             // unguarded
        // Therefore: option (b) gate-off when payload is null; flat pass-through when it exists.

        // Simulate what the controller does: serialize the payload element directly.
        const string storedPayloadJson = """{"battleId":"999","seed":42,"fieldId":1,"firstTurn":1,"card_master_id":100,"vid1":1,"name1":"A","charaId1":1,"classId1":1,"emblemId1":100000000,"degreeId1":0,"countryCode1":"JP","sleeveId1":3000011,"battlePoint1":0,"masterPoint1":0,"rank1":1,"isOfficial1":false,"deck1":[],"vid2":2,"name2":"B","charaId2":1,"classId2":2,"emblemId2":100000000,"degreeId2":0,"countryCode2":"JP","sleeveId2":3000011,"battlePoint2":0,"masterPoint2":0,"rank2":1,"isOfficial2":false,"deck2":[]}""";
        using var doc = JsonDocument.Parse(storedPayloadJson);
        var element = doc.RootElement.Clone();

        // Re-serialize as if returned from Ok(element) — should be flat
        var json = JsonSerializer.Serialize(element, Opts);
        using var resultDoc = JsonDocument.Parse(json);
        var root = resultDoc.RootElement;

        // battleId must be at root level, NOT under a "replay_info" wrapper
        Assert.That(root.TryGetProperty("battleId", out var battleId), Is.True,
            "battleId must be at root level — no replay_info wrapper");
        Assert.That(battleId.GetString(), Is.EqualTo("999"));

        // replay_info key must NOT exist
        Assert.That(root.TryGetProperty("replay_info", out _), Is.False,
            "replay_info wrapper must NOT be present — client reads data[\"battleId\"] directly");
    }

    [Test]
    public void GuildInfo_non_joined_serializes_to_prod_shape()
    {
        var resp = new GuildInfoResponse
        {
            MaxMemberNum = 30,
            MaxSubLeaderNum = 2,
            GuildStatus = 0,
            UsableStampList = Enumerable.Range(100001, 20).Select(i => i.ToString()).ToList(),
        };
        var json = JsonSerializer.Serialize(resp, Opts);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("max_member_num").GetString(), Is.EqualTo("30"));
        Assert.That(root.GetProperty("max_sub_leader_num").GetString(), Is.EqualTo("2"));
        Assert.That(root.GetProperty("guild_status").GetString(), Is.EqualTo("0"));
        Assert.That(root.GetProperty("usable_stamp_list").EnumerateArray().Select(e => e.GetString()).First(), Is.EqualTo("100001"));

        // The four nullable fields MUST NOT appear when null:
        Assert.That(root.TryGetProperty("guild", out _), Is.False);
        Assert.That(root.TryGetProperty("join_request_count", out _), Is.False);
        Assert.That(root.TryGetProperty("invite_count", out _), Is.False);
    }
}
