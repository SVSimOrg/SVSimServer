using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Placeholder for an Avatar/Hero mode schedule entry. The 2026-05-23 prod capture had an empty
/// schedules list, so the entry shape is TBD — fill in fields when an active Avatar window is
/// captured. AvatarBattleAllInfo.Parse on the client side is the parser to read for shape.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class AvatarSchedule
{
}
