using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserLeaderSkin
{
    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int Id { get; set; }
    [JsonPropertyName("leader_skin_name")]
    [Key("leader_skin_name")] 
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }
    [JsonPropertyName("emote_id")]
    [Key("emote_id")]
    public int EmoteId { get; set; }
    [JsonPropertyName("is_owned")]
    [Key("is_owned")]
    public bool IsOwned { get; set; }

    public UserLeaderSkin(LeaderSkinEntry leaderSkin, bool isOwned)
    {
        this.Id = leaderSkin.Id;
        this.Name = leaderSkin.Name;
        // Class is nullable — class-agnostic skins (CSV class_chara_id=0) come in as null. Fall
        // back to the FK column (also nullable) and finally 0.
        this.ClassId = leaderSkin.Class?.Id ?? leaderSkin.ClassId ?? 0;
        this.EmoteId = leaderSkin.EmoteId;
        this.IsOwned = isOwned;
    }

    public UserLeaderSkin()
    {
    }
}