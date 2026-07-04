using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /payment_pc/* — Steam/PC store endpoints. Currently serves item_list (the storefront product
/// catalog); purchase flows (/payment_pc/finish etc.) are not yet implemented.
///
/// Route is explicit because the URL prefix doesn't match the controller name pattern
/// (SVSimController applies [Route("[controller]")] which would resolve to /payment).
/// </summary>
[Route("payment_pc")]
public class PaymentController : SVSimController
{
    /// <summary>"yyyy-MM-dd HH:mm:ss" — prod's PHP datetime convention on the wire.</summary>
    private const string WireDateFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly IGlobalsRepository _globalsRepository;

    public PaymentController(IGlobalsRepository globalsRepository)
    {
        _globalsRepository = globalsRepository;
    }

    [HttpPost("item_list")]
    public async Task<ActionResult<Dictionary<string, PaymentItemInfo>>> ItemList(PaymentItemListRequest request)
    {
        var items = await _globalsRepository.GetPaymentItems();
        return items.ToDictionary(
            row => row.StoreProductId.ToString(CultureInfo.InvariantCulture),
            row => BuildPaymentItemInfo(row));
    }

    /// <summary>
    /// Map a typed DB row to the all-strings wire shape prod uses. Typed columns let us query and
    /// validate cleanly server-side; PHP-stringification happens here at the wire boundary.
    /// </summary>
    private static PaymentItemInfo BuildPaymentItemInfo(PaymentItemEntry row) => new()
    {
        RecordId = row.Id.ToString(CultureInfo.InvariantCulture),
        Id = row.ProductId.ToString(CultureInfo.InvariantCulture),
        StoreProductId = row.StoreProductId.ToString(CultureInfo.InvariantCulture),
        Name = row.Name,
        Text = row.Text,
        // Prod price wire shape is e.g. "0.99" with up to 2 decimals. InvariantCulture renders the
        // .NET decimal as "0.99" / "10.99" cleanly without trailing zeros from a scale of 4+.
        Price = row.Price.ToString("0.##", CultureInfo.InvariantCulture),
        ChargeCrystalNum = row.ChargeCrystalNum.ToString(CultureInfo.InvariantCulture),
        FreeCrystalNum = row.FreeCrystalNum.ToString(CultureInfo.InvariantCulture),
        PurchaseLimit = row.PurchaseLimit.ToString(CultureInfo.InvariantCulture),
        SpecialShopFlag = row.SpecialShopFlag.ToString(CultureInfo.InvariantCulture),
        ImageName = row.ImageName,
        StartTime = row.StartTime.ToString(WireDateFormat, CultureInfo.InvariantCulture),
        EndTime = row.EndTime.ToString(WireDateFormat, CultureInfo.InvariantCulture),
        RemainingTime = row.RemainingTime.ToString(CultureInfo.InvariantCulture),
        IsResaleProduct = row.IsResaleProduct.ToString(CultureInfo.InvariantCulture),
        // Prod sends "" when no resale window is scheduled; otherwise the formatted date.
        ResaleStartDate = row.ResaleStartDate is { } d
            ? d.ToString(WireDateFormat, CultureInfo.InvariantCulture)
            : string.Empty,
        // TODO(payment-stub): per-viewer count of this product's purchases. Hardcoded to 0 until
        // viewer-purchase tracking lands. Fresh viewers always see 0 in prod anyway.
        PurchaseNumCurrent = 0,
    };
}
