using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

/// <summary>
/// Request body for /mypage/refresh. Carries only the standard auth envelope —
/// no <c>carrier</c> field, unlike MyPageIndexRequest. Confirmed against prod traffic
/// in data_dumps/captures/traffic_prod.ndjson: both refresh request bodies have exactly
/// <c>viewer_id / steam_id / steam_session_ticket</c>.
/// </summary>
[MessagePackObject]
public class MyPageRefreshRequest : BaseRequest
{
}
