using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Practice;

[MessagePackObject]
public class PracticeOpponent
{
    /// <summary>Practice slot id (unique per entry; AI opponent identifier).</summary>
    [JsonPropertyName("practice_id")]
    [Key("practice_id")] public int PracticeId { get; set; }

    /// <summary>
    /// Text-table id resolved client-side via Data.Master.GetPracticeText(text_id).
    /// Stringified int 窶・client calls .ToString() before lookup. Sent as string to be safe.
    /// </summary>
    [JsonPropertyName("text_id")]
    [Key("text_id")] public string TextId { get; set; } = string.Empty;

    /// <summary>Class (leader) id the AI plays.</summary>
    [JsonPropertyName("class_id")]
    [Key("class_id")] public int ClassId { get; set; }

    /// <summary>Portrait / character id (which leader art the AI uses).</summary>
    [JsonPropertyName("chara_id")]
    [Key("chara_id")] public int CharaId { get; set; }

    /// <summary>Title-degree id shown next to the AI's name.</summary>
    [JsonPropertyName("degree_id")]
    [Key("degree_id")] public int DegreeId { get; set; }

    /// <summary>AI deck-strength tier (drives which preset deck the AI uses).</summary>
    [JsonPropertyName("ai_deck_level")]
    [Key("ai_deck_level")] public int AiDeckLevel { get; set; }

    /// <summary>AI decision-making tier.</summary>
    [JsonPropertyName("ai_logic_level")]
    [Key("ai_logic_level")] public int AiLogicLevel { get; set; }

    /// <summary>Starting HP for the AI side (often 20).</summary>
    [JsonPropertyName("ai_max_life")]
    [Key("ai_max_life")] public int AiMaxLife { get; set; } = 20;

    /// <summary>3D battle-field asset id (string on the wire; client int.TryParse's it).</summary>
    [JsonPropertyName("battle3dfield_id")]
    [Key("battle3dfield_id")] public string Battle3dFieldId { get; set; } = "1";

    /// <summary>true => entry disabled, client prepends maintenance suffix. Must always be emitted: client reads with `data["is_maintenance"] != null` but LitJson throws KeyNotFoundException on a missing key.</summary>
    [JsonPropertyName("is_maintenance")]
    [Key("is_maintenance")] public bool IsMaintenance { get; set; }

    /// <summary>true => entry is a special "campaign" practice (event-tied).</summary>
    [JsonPropertyName("is_campaign_practice")]
    [Key("is_campaign_practice")] public bool IsCampaignPractice { get; set; }
}
