using System;
using Cute;

public class OpponentNotTurnEndToWinChecker : NetworkBattleIntervalCheckerBase
{
	private NetworkBattleManagerBase networkBattleManager;

	private float _timeoutDisconnect;

	public event Action OnOpponentNotTurnEndToWin;

	public OpponentNotTurnEndToWinChecker(NetworkBattleManagerBase manager)
	{
		networkBattleManager = manager;
		_timeoutDisconnect = 125f;
	}

	public void SetTimeoutTime(bool isExtend)
	{
		if (isExtend)
		{
			_timeoutDisconnect = 150f;
		}
		else
		{
			_timeoutDisconnect = 125f;
		}
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if (!networkBattleManager.disconnectToLoseChecker.IsDisconnect() && (float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= _timeoutDisconnect)
		{
			this.OnOpponentNotTurnEndToWin.Call();
			StopChecker();
		}
	}
}
