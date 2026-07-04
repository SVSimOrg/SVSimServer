using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Joins /load/index data.my_rotation_info.{setting, reprinted_base_card_ids, restricted_base_card_id_list}
/// on rotation_id. CardSetIdsCsv and AbilitiesCsv mirror the wire's pipe-delimited string format
/// (e.g. "10000|10001|10002"); the importer keeps them verbatim.
/// </summary>
public class MyRotationSettingEntry : BaseEntity<int>
{
    public int RotationId { get => Id; set => Id = value; }

    public string CardSetIdsCsv { get; set; } = string.Empty;

    public string AbilitiesCsv { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string ReprintedCardIds { get; set; } = "[]";

    [Column(TypeName = "jsonb")]
    public string RestrictedCardIds { get; set; } = "[]";
}
