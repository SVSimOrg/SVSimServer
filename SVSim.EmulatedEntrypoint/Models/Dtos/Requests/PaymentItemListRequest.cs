using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

/// <summary>
/// Request body for /payment_pc/item_list. Prod sends only the standard auth envelope
/// (viewer_id / steam_id / steam_session_ticket) — no additional fields.
/// </summary>
[MessagePackObject]
public class PaymentItemListRequest : BaseRequest
{
}
