using LitJson;
using Wizard;
using Wizard.Battle.Recovery;

public class FinishTaskBase : BaseTask
{
	protected int classId;

	public bool IsResponseDataExist(JsonData response)
	{
		JsonData jsonData = response["data"];
		if (jsonData == null || jsonData.Count == 0)
		{
			return false;
		}
		return true;
	}

	protected bool IsEffectiveErrorCode(int code)
	{
		if (resultCode != 1 && resultCode != 3502)
		{
			return true;
		}
		return false;
	}
}
