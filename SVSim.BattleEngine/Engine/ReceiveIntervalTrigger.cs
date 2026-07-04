using Wizard;

public class ReceiveIntervalTrigger
{

	public virtual void ReceiveDataCheck(NetworkBattleManagerBase networkBattleManager, NetworkBattleData networkBattleData, bool isPlayer, bool isExTurn)
	{
		NetworkBattleReceiver.ReceiveData receiveData = networkBattleData.GetReceiveData();
		if (IsEffectiveURI(receiveData.dataUri))
		{
			if (isPlayer)
			{
				networkBattleManager.disconnectToDispChecker.EraseDisp();
			}
			else
			{
				networkBattleManager.judgeResultFailedToRetryChecker.StopChecker();
			}
			if (receiveData.dataUri != NetworkBattleDefine.NetworkBattleURI.RecoveryStart)
			{
				if (isPlayer)
				{
					if (networkBattleManager.recoveryToDispChecker != null && networkBattleManager.recoveryToDispChecker.isDisp)
					{
						networkBattleManager.recoveryToDispChecker.EraseDisp();
					}
				}
				else if (networkBattleManager.opponentRecoveryToDispChecker != null && networkBattleManager.opponentRecoveryToDispChecker.isDisp)
				{
					networkBattleManager.opponentRecoveryToDispChecker.EraseDisp();
				}
			}
		}
		switch (receiveData.dataUri)
		{
		case NetworkBattleDefine.NetworkBattleURI.Ready:
			networkBattleManager.notMulliganEndToJudgeChecker.StopChecker();
			if (!networkBattleManager.InstanceNetworkAgent.GetTurnState())
			{
				networkBattleManager.opponentNotTurnStartToWinChecker.StartChecker();
			}
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnStart:
			networkBattleManager.opponentNotTurnEndToWinChecker.StartChecker();
			networkBattleManager.opponentNotTurnStartToWinChecker.StopChecker();
			networkBattleManager.notTurnEndToLoseChecker.StopChecker();
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnEnd:
			networkBattleManager.opponentNotTurnEndToWinChecker.StopChecker();
			if (networkBattleManager.BattleEnemy.IsExtraTurn)
			{
				networkBattleManager.opponentNotTurnStartToWinChecker.StartChecker();
			}
			else
			{
				networkBattleManager.notTurnStartToLoseChecker.StartChecker();
			}
			break;
		case NetworkBattleDefine.NetworkBattleURI.Judge:
			if (isExTurn)
			{
				networkBattleManager.notTurnStartToLoseChecker.StartChecker();
			}
			break;
		case NetworkBattleDefine.NetworkBattleURI.RecoveryStart:
			if (receiveData.isSelf)
			{
				break;
			}
			if (isPlayer)
			{
				if (networkBattleManager.recoveryToDispChecker != null && !networkBattleManager.recoveryToDispChecker.isDisp)
				{
					networkBattleManager.recoveryToDispChecker.CreateDisp();
				}
				break;
			}
			networkBattleManager.opponentNotTurnStartToWinChecker.SetTimeoutTime(isExtend: true);
			networkBattleManager.opponentNotTurnEndToWinChecker.SetTimeoutTime(isExtend: true);
			if (!networkBattleManager.opponentRecoveryToDispChecker.isDisp)
			{
				networkBattleManager.opponentRecoveryToDispChecker.CreateDisp();
			}
			break;
		case NetworkBattleDefine.NetworkBattleURI.RecoveryEnd:
			if (!isPlayer)
			{
				networkBattleManager.opponentNotTurnStartToWinChecker.SetTimeoutTime(isExtend: false);
				networkBattleManager.opponentNotTurnEndToWinChecker.SetTimeoutTime(isExtend: false);
			}
			break;
		}
	}

	public static bool IsEffectiveURI(NetworkBattleDefine.NetworkBattleURI uri)
	{
		if (uri == NetworkBattleDefine.NetworkBattleURI.TurnStart || uri == NetworkBattleDefine.NetworkBattleURI.TurnEndActions || uri == NetworkBattleDefine.NetworkBattleURI.TurnEnd || uri == NetworkBattleDefine.NetworkBattleURI.PlayActions || uri == NetworkBattleDefine.NetworkBattleURI.BattleFinish || uri == NetworkBattleDefine.NetworkBattleURI.TurnEndFinal || uri == NetworkBattleDefine.NetworkBattleURI.JudgeResult || uri == NetworkBattleDefine.NetworkBattleURI.Retire || uri == NetworkBattleDefine.NetworkBattleURI.RecoveryStart || uri == NetworkBattleDefine.NetworkBattleURI.RecoveryEnd)
		{
			return true;
		}
		return false;
	}
}
