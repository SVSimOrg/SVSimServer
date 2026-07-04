using System;
using Cute;

public class NotTurnEndToLoseChecker : NetworkBattleIntervalCheckerBase
{
	private NetworkBattleManagerBase networkBattleManager;

	public event Action OnNotTurnEndToLose;

	public NotTurnEndToLoseChecker(NetworkBattleManagerBase manager)
	{
		networkBattleManager = manager;
	}

	public override void StopChecker()
	{
		if (!((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 125f))
		{
			base.StopChecker();
		}
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if (!networkBattleManager.disconnectToLoseChecker.IsDisconnect() && (float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 125f)
		{
			this.OnNotTurnEndToLose.Call();
			InitTimer();
			StopChecker();
		}
	}
}
