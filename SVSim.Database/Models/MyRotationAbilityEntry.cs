using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class MyRotationAbilityEntry : BaseEntity<int>
{
    public int AbilityId { get => Id; set => Id = value; }

    /// <summary>Raw ability blob from /load/index data.my_rotation_info.abilities[abilityId].</summary>
    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}";
}
