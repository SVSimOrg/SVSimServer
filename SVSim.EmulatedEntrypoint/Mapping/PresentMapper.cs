using System.Globalization;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Mapping;

internal static class PresentMapper
{
    /// <summary>
    /// Project a ViewerPresent row onto the wire DTO. Field-by-field stringification matches
    /// the prod capture at data_dumps/captures/traffic_event_crate_free_pack.ndjson:
    ///   - present_id, reward_type, reward_detail_id, reward_count, condition_number,
    ///     present_limit_type — STRINGS on the wire.
    ///   - reward_limit_time, item_type — INTS on the wire.
    ///   - create_time — "yyyy-MM-dd HH:mm:ss" string, gift's row-creation time (NOT now()).
    /// </summary>
    public static PresentDto ToWire(ViewerPresent row) => new()
    {
        PresentId        = row.PresentId,
        RewardType       = row.RewardType.ToString(CultureInfo.InvariantCulture),
        RewardDetailId   = row.RewardDetailId.ToString(CultureInfo.InvariantCulture),
        RewardCount      = row.RewardCount.ToString(CultureInfo.InvariantCulture),
        ConditionNumber  = row.ConditionNumber.ToString(CultureInfo.InvariantCulture),
        PresentLimitType = row.PresentLimitType.ToString(CultureInfo.InvariantCulture),
        RewardLimitTime  = (int)row.RewardLimitTime,
        CreateTime       = row.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        ItemType         = row.ItemType,
        Message          = row.Message,
    };
}
