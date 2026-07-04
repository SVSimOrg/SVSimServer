namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

/// <summary>
/// <c>POST /tool/signup</c> response. The interesting outputs (viewer_id, short_udid, udid) all
/// live in <c>data_headers</c>; the <c>data</c> payload is empty. <c>SignUpTask.Parse</c> never
/// reads <c>data</c>.
/// </summary>
public class SignupResponse
{
}
