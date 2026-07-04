using LitJson;

namespace Wizard.Battle.Operation;

public class ChangeAIOperationCommand : IOperationCommand
{
	public string Logic { get; private set; }

	public int QueueCount { get; private set; }

	public ChangeAIOperationCommand(JsonData actionJsonData)
	{
		Logic = actionJsonData["logic"].ToString();
		QueueCount = actionJsonData["queue_count"].ToInt();
	}

	public void Operation(BattleManagerBase battleMgr)
	{
	}
}
