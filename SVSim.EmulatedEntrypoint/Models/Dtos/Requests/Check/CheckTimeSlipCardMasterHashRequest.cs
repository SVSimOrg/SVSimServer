using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Check;

/// <summary>
/// Empty request envelope. Prod ships only the standard BaseRequest fields
/// (viewer_id / steam_id / steam_session_ticket) — verified in
/// traffic_prod_taketwo_selections.ndjson + traffic_prod_tradeables_capture.ndjson.
/// </summary>
[MessagePackObject]
public class CheckTimeSlipCardMasterHashRequest : BaseRequest { }
