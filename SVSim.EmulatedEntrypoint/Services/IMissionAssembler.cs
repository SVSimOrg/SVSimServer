using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Builds the MissionInfoDataDto from (viewer, catalog, counters). One place — reused by
/// all four endpoints. Reads catalog through repos, batches counter reads.
/// </summary>
public interface IMissionAssembler
{
    Task<MissionInfoDataDto> BuildAsync(Viewer viewer, CancellationToken ct = default);
}
