using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserClass
{
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }
    
    [JsonPropertyName("is_available")]
    [Key("is_available")]
    public int IsAvailable { get; set; }
    
    [JsonPropertyName("level")]
    [Key("level")]
    public int Level { get; set; }
    
    [JsonPropertyName("exp")]
    [Key("exp")]
    public int Exp { get; set; }
    
    [JsonPropertyName("is_random_leader_skin")]
    [Key("is_random_leader_skin")]
    public int IsRandomLeaderSkin { get; set; }
    
    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }

    [JsonPropertyName("leader_skin_id_list")]
    [Key("leader_skin_id_list")]
    public List<int> LeaderSkinIds { get; set; } = new List<int>();
    
    [JsonPropertyName("default_leader_skin_id")]
    [Key("default_leader_skin_id")]
    public int DefaultLeaderSkinId { get; set; }

    public UserClass(ViewerClassData viewerClass, IReadOnlyCollection<int> ownedSkinIdsForClass)
    {
        this.ClassId = viewerClass.Class.Id;
        this.IsAvailable = 1;
        this.Level = viewerClass.Level;
        this.Exp = viewerClass.Exp;
        this.IsRandomLeaderSkin = viewerClass.IsRandomLeaderSkin ? 1 : 0;
        this.LeaderSkinId = viewerClass.LeaderSkin.Id;
        this.LeaderSkinIds = ownedSkinIdsForClass.ToList();
        this.DefaultLeaderSkinId = viewerClass.Class.DefaultLeaderSkin?.Id ?? 0;
    }

    public UserClass()
    {
    }
}