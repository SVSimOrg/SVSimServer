public class NetworkRecoveryBattleData : NetworkWatchBattleData
{
	public NetworkRecoveryBattleData(NetworkBattleManagerBase battleMgr)
		: base(battleMgr)
	{
	}

	public override void BeforeSettingReceiveData()
	{
		SetEnemyFirstTurn();
		base.BeforeSettingReceiveData();
	}

	protected override ReplaceReceivedCard CreateReplaceReceivedCard(CardDataModel cardData)
	{
		return new RecoveryReplaceReceivedCard(_battleMgr, cardData);
	}
}
