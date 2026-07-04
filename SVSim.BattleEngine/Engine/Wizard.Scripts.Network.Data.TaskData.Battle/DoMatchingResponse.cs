using LitJson;

namespace Wizard.Scripts.Network.Data.TaskData.Battle;

public class DoMatchingResponse
{
	public int battleState;

	public int matchingState;

	public DoMatchingResponse()
	{
	}

	public DoMatchingResponse(JsonData data)
	{
		battleState = data["battle_state"].ToInt();
		matchingState = data["matching_state"].ToInt();
	}
}
