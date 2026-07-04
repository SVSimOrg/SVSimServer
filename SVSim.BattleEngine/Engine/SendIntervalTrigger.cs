public class SendIntervalTrigger
{
	public virtual void SendDataCheck(NetworkBattleManagerBase networkBattleManager, NetworkBattleDefine.NetworkBattleURI sendUri)
	{
		switch (sendUri)
		{
		case NetworkBattleDefine.NetworkBattleURI.Swap:
			if (!networkBattleManager.notMulliganEndToJudgeChecker.IsTimeOver())
			{
				networkBattleManager.IsSendSwap = true;
			}
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnStart:
			networkBattleManager.opponentNotTurnStartToWinChecker.StopChecker();
			networkBattleManager.notTurnStartToLoseChecker.StopChecker();
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnEnd:
			networkBattleManager.notTurnEndToLoseChecker.StopChecker();
			if (!networkBattleManager.BattlePlayer.IsExtraTurn)
			{
				networkBattleManager.opponentNotTurnStartToWinChecker.StartChecker();
			}
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnEndFinal:
			networkBattleManager.notTurnEndToLoseChecker.StopChecker();
			networkBattleManager.opponentNotTurnStartToWinChecker.StartChecker();
			break;
		case NetworkBattleDefine.NetworkBattleURI.Ready:
		case NetworkBattleDefine.NetworkBattleURI.TurnEndActions:
			break;
		}
	}
}
