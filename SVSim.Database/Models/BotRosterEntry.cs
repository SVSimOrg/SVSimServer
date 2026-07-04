using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per AI bot opponent the rank-battle AI-fallback path can pick. Populated
/// from seeds/bot-roster.json by SVSim.Bootstrap.BotRosterImporter.
///
/// The Id (= AiId) MUST match a row in the client's baked-in master CSV
/// <c>data_dumps/client-assets/rm_ai_setting.csv</c>; if it doesn't, the client's
/// <c>RankMatchAISettingList.GetSettingData(aiId)</c> throws
/// <c>InvalidOperationException</c> at battle-start.
///
/// Cosmetic ids (sleeve / emblem / degree / field) MUST resolve in
/// <c>SBattleLoad.LoadOpponentAssets</c>; placeholder 1s left the client hanging on
/// "Waiting for opponent". Prod-verified values were captured from live prod traffic.
/// </summary>
public class BotRosterEntry : BaseEntity<int>
{
    /// <summary>Client AI catalog id (rm_ai_setting.csv enemy_ai_id). Also the PK.</summary>
    public int AiId { get => Id; set => Id = value; }

    public string CountryCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public int SleeveId { get; set; }
    public int EmblemId { get; set; }
    public int DegreeId { get; set; }
    public int FieldId { get; set; }
    public int IsOfficial { get; set; }

    public int ClassId { get; set; }
    public int CharaId { get; set; }

    public int Rank { get; set; }
    public int BattlePoint { get; set; }
    public int IsMasterRank { get; set; }
    public int MasterPoint { get; set; }
}
