using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One mypage home-dialog popup from /mypage/index data.home_dialog_list. Id is authored in
/// the seed file (no stable wire ID; see banners.json for the same pattern). The dialog fires
/// once per viewer per server-process lifetime — see IHomeDialogSessionTracker.
/// </summary>
public class HomeDialogEntry : BaseEntity<int>
{
    public string TitleTextId { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;

    /// <summary>jsonb — List&lt;HomeDialogButtonSeed&gt; serialized verbatim. Deserialized in
    /// MyPageController via JsonbReadOptions.</summary>
    [Column(TypeName = "jsonb")]
    public string ButtonListJson { get; set; } = "[]";

    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>Wire "type" — client parser ignores it but prod sends "1". Nullable so we
    /// omit when unset; serialized as a string per <c>HomeDialog.Type</c> on the DTO.</summary>
    public int? Type { get; set; }

    /// <summary>Tiebreaker when multiple entries are active. Higher wins; ID asc breaks
    /// further ties. Each /mypage/index call emits the highest-priority unfired entry.</summary>
    public int Priority { get; set; }
}
