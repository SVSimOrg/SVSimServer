using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per AI opponent shown on the practice (solo-play) opponent select screen.
/// Populated from seeds/practice-opponents.json by SVSim.Bootstrap.PracticeOpponentImporter.
///
/// The (<see cref="ClassId"/>, <see cref="AiDeckLevel"/>) pair MUST exist in the client's
/// baked-in master CSV `ai/practice_ai_setting`; if it doesn't, the client's
/// PracticeAISettingDataSet.GetSettingData throws InvalidOperationException and the
/// difficulty-select dialog crashes BEFORE /practice/start is sent. Prod's catalog is
/// the safe source of truth — we can't see the CSV directly.
/// </summary>
public class PracticeOpponentEntry : BaseEntity<int>
{
    /// <summary>Practice slot id (Id = practice_id from the wire; also unique).</summary>
    public int PracticeId { get => Id; set => Id = value; }

    /// <summary>Text-table key resolved client-side via Data.Master.GetPracticeText.</summary>
    public string TextId { get; set; } = string.Empty;

    /// <summary>Class (leader) id the AI plays.</summary>
    public int ClassId { get; set; }

    /// <summary>Portrait / character id (leader art).</summary>
    public int CharaId { get; set; }

    /// <summary>Title-degree id shown next to the AI name. -1 when unset.</summary>
    public int DegreeId { get; set; }

    /// <summary>AI deck-strength tier; key into the client's practice_ai_setting CSV.</summary>
    public int AiDeckLevel { get; set; }

    /// <summary>AI decision-making tier.</summary>
    public int AiLogicLevel { get; set; }

    /// <summary>Starting HP for the AI side (typically 20; 10 for the easiest "tutorial" rows).</summary>
    public int AiMaxLife { get; set; }

    /// <summary>3D battlefield asset id (string on the wire; client int.TryParse's it).</summary>
    public string Battle3dFieldId { get; set; } = "1";

    /// <summary>true => entry shown but disabled with a maintenance suffix.</summary>
    public bool IsMaintenance { get; set; }

    /// <summary>true => entry is a special event-tied "campaign" practice.</summary>
    public bool IsCampaignPractice { get; set; }
}
