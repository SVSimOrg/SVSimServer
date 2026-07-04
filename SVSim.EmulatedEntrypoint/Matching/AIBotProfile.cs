namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// Cosmetic + identity metadata for an AI opponent. Used to compose
/// <c>oppo_info</c> in the <c>/ai_&lt;fmt&gt;_rank_battle/start</c> response.
/// The wire keys are camelCase (sleeveId, emblemId, etc.) — the DTO handles
/// the JSON serialization; this record is the internal-facing shape.
/// </summary>
public sealed record AIBotProfile(
    int AiId,
    string CountryCode,
    string UserName,
    int SleeveId,
    int EmblemId,
    int DegreeId,
    int FieldId,
    int IsOfficial,
    int ClassId,
    int CharaId,
    int Rank,
    int BattlePoint,
    int IsMasterRank,
    int MasterPoint);
