using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class ClassExp
{
    [JsonPropertyName("level")]
    [Key("level")]
    public int Level { get; set; }
    [JsonPropertyName("necessary_exp")]
    [Key("necessary_exp")]
    public int NecessaryExp { get; set; }
    [JsonPropertyName("diff_exp")]
    [Key("diff_exp")]
    public int DiffExp { get; set; }
    [JsonPropertyName("accumulate_exp")]
    [Key("accumulate_exp")]
    public int AccumulateExp { get; set; }
}