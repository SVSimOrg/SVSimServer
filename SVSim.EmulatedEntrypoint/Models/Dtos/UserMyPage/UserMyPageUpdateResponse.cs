using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.UserMyPage;

/// <summary>
/// Empty response payload. The client's <c>MyPageSettingUpdateTask.Parse()</c> is the default
/// pass-through; server just acknowledges.
/// </summary>
[MessagePackObject(true)]
public sealed class UserMyPageUpdateResponse
{
}
