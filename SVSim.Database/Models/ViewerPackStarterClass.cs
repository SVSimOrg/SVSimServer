using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Records a viewer's class choice for a <see cref="Enums.PackCategory.RotationStarterCardPack"/>.
/// One row per (Viewer, Pack) — the choice is one-shot per pack per
/// /pack/set_rotation_starter_class spec. Surfaces as <c>selected_class_id</c> on the parent
/// PackConfig in the next /pack/info response so the client's starter-pack dialog can
/// pre-select on revisit. <see cref="PackId"/> = parent_gacha_id; <see cref="ClassId"/> is 1..8.
/// </summary>
[Owned]
public class ViewerPackStarterClass
{
    public int PackId { get; set; }
    public int ClassId { get; set; }
}
