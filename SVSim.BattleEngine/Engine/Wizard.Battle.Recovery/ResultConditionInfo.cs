using LitJson;

namespace Wizard.Battle.Recovery;

public class ResultConditionInfo : BattleConditionInfo
{
	public ResultConditionInfo(JsonData jsonData)
		: base(jsonData, useDefaultInPlayCardValue: false)
	{
	}
}
