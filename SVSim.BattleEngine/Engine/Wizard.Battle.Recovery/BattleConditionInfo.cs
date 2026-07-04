using LitJson;

namespace Wizard.Battle.Recovery;

public class BattleConditionInfo
{
	public BattleConditionPlayerInfo PlayerInfo { get; private set; }

	public BattleConditionEnemyInfo EnemyInfo { get; private set; }

	public BattleConditionInfo(JsonData jsonData, bool useDefaultInPlayCardValue)
	{
		PlayerInfo = new BattleConditionPlayerInfo(jsonData["player"], useDefaultInPlayCardValue);
		EnemyInfo = new BattleConditionEnemyInfo(jsonData["enemy"], useDefaultInPlayCardValue);
	}
}
