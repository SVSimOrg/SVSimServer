public class SendIntervalTriggerStandard : SendIntervalTrigger
{
	public override void SendDataCheck(NetworkBattleManagerBase networkBattleManager, NetworkBattleDefine.NetworkBattleURI sendUri)
	{
		base.SendDataCheck(networkBattleManager, sendUri);
		if (ReceiveIntervalTrigger.IsEffectiveURI(sendUri))
		{
			(networkBattleManager as NetworkStandardBattleMgr).battleStopChecker.StartChecker();
		}
	}
}
