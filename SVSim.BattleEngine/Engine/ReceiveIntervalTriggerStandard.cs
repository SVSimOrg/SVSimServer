public class ReceiveIntervalTriggerStandard : ReceiveIntervalTrigger
{
	public override void ReceiveDataCheck(NetworkBattleManagerBase networkBattleManager, NetworkBattleData networkBattleData, bool isPlayer, bool isExTurn)
	{
		base.ReceiveDataCheck(networkBattleManager, networkBattleData, isPlayer, isExTurn);
		if (ReceiveIntervalTrigger.IsEffectiveURI(networkBattleData.GetReceiveData().dataUri))
		{
			(networkBattleManager as NetworkStandardBattleMgr).battleStopChecker.StartChecker();
		}
	}
}
