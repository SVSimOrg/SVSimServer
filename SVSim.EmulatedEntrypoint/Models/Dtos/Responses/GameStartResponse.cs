using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

/// <summary>
/// Wire-shape mirrors production's <c>/check/game_start</c> response. Several fields here are
/// NOT read by <c>Cute/GameStartCheckTask.Parse</c> (<c>now_viewer_id</c>, <c>now_name</c>,
/// <c>now_rank</c> 窶・those are consumed by sibling tasks); they're included because prod sends
/// them and the boot worked when we matched prod exactly. Removing them is a regression risk
/// even though the parse-time decompile says they're unused.
/// </summary>
[MessagePackObject]
public class GameStartResponse
{
    /// <summary>The signed-in viewer's internal id. Prod always sends.</summary>
    [JsonPropertyName("now_viewer_id")]
    [Key("now_viewer_id")]
    public long NowViewerId { get; set; }

    /// <summary>
    /// Whether the user has set a data-transfer password. Prod sends a non-null bool;
    /// <c>GameStartCheckTask.Parse</c> gates the read with <c>Keys.Contains</c>.
    /// </summary>
    [JsonPropertyName("is_set_transition_password")]
    [Key("is_set_transition_password")]
    public bool IsSetTransitionPassword { get; set; }

    /// <summary>Viewer display name. Not read by GameStartCheckTask but sent by prod.</summary>
    [JsonPropertyName("now_name")]
    [Key("now_name")]
    public string NowName { get; set; } = string.Empty;

    /// <summary>
    /// Per-format rank-name map keyed by deck-format id ("1", "2", "4" observed in prod).
    /// Stub for now until rank state is persisted; pinned to RankName_010 / RankName_017
    /// (matches prod's shape).
    /// </summary>
    [JsonPropertyName("now_rank")]
    [Key("now_rank")]
    public Dictionary<string, string> NowRank { get; set; } = new();

    /// <summary>
    /// Tutorial progress 窶・**sent as a string on the wire** ("100" = tutorial complete).
    /// <c>GameStartCheckTask.Parse</c> calls <c>.ToInt()</c> so LitJson coerces.
    /// </summary>
    [JsonPropertyName("now_tutorial_step")]
    [Key("now_tutorial_step")]
    public string NowTutorialStep { get; set; } = "100";

    /// <summary>
    /// Linked social accounts. Per-entry shape in <see cref="TransitionAccountData"/>.
    /// </summary>
    [JsonPropertyName("transition_account_data")]
    [Key("transition_account_data")]
    public List<TransitionAccountData> TransitionAccountData { get; set; } = new();

    /// <summary>
    /// When present, client overwrites <c>Certification.ViewerId</c> with this value. Optional
    /// 窶・leave null to omit. The serialization pipeline (JSON + msgpack via the translation
    /// middleware) drops null properties end-to-end, so the client sees the key as absent.
    /// </summary>
    [JsonPropertyName("rewrite_viewer_id")]
    [Key("rewrite_viewer_id")]
    public long? RewriteViewerId { get; set; }

    /// <summary>
    /// Presence indicates the user has applied for account deletion (value ignored by client at
    /// this stage). Optional 窶・leave null to omit.
    /// </summary>
    [JsonPropertyName("account_delete_reservation_status")]
    [Key("account_delete_reservation_status")]
    public int? AccountDeleteReservationStatus { get; set; }

    // --- Agreement / consent state (all required) ---

    /// <summary><c>PlayerStaticData.AgreementState</c> enum.</summary>
    [JsonPropertyName("tos_state")]
    [Key("tos_state")]
    public int TosState { get; set; }

    /// <summary><c>PlayerStaticData.AgreementState</c> enum.</summary>
    [JsonPropertyName("policy_state")]
    [Key("policy_state")]
    public int PolicyState { get; set; }

    /// <summary><c>PlayerStaticData.AgreementState</c> enum.</summary>
    [JsonPropertyName("kor_authority_state")]
    [Key("kor_authority_state")]
    public int KorAuthorityState { get; set; }

    /// <summary>Current Terms of Service document id.</summary>
    [JsonPropertyName("tos_id")]
    [Key("tos_id")]
    public int TosId { get; set; }

    /// <summary>Current Privacy Policy document id.</summary>
    [JsonPropertyName("policy_id")]
    [Key("policy_id")]
    public int PolicyId { get; set; }

    /// <summary>Current Korean authority consent document id.</summary>
    [JsonPropertyName("kor_authority_id")]
    [Key("kor_authority_id")]
    public int KorAuthorityId { get; set; }
}
