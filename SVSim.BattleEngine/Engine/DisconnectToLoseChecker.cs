using System;
using Cute;
using UnityEngine;
using Wizard;

public class DisconnectToLoseChecker : NetworkBattleIntervalCheckerBase
{

	private bool _isAlreadyTriedSocketReplace;

	private bool _isSocketOpenDisconnectLog;

	public event Action OnDisconnectLose;

	public event Action OnBeforeDisconnectLose;

	public event Action OnDisconnectCheck;

	public bool IsDisconnect()
	{
		if (!base.isStop && ((float)GetDisconnectTime() >= 10f || Application.internetReachability == NetworkReachability.NotReachable))
		{
			LocalLog.SetDisconnectLog("IsDisconnect time" + GetDisconnectTime() + "internetReachability" + Application.internetReachability);
			return true;
		}
		return false;
	}

	public bool IsSelfDisconnectLose()
	{
		if (IsSelfDisConnectOnTimeout() || _networkBattleManagerBase.InstanceNetworkAgent.IsReceiveSelfDisconnect)
		{
			return true;
		}
		return false;
	}

	public bool IsSelfDisConnectOnTimeout()
	{
		if ((float)GetDisconnectTime() >= 125f)
		{
			return true;
		}
		return false;
	}

	private bool IsSelfDisconnectLoseCheck()
	{
		if ((float)GetDisconnectTime() >= 65f)
		{
			return true;
		}
		return false;
	}

	public override void StopChecker()
	{
		base.StopChecker();
	}

	public override void StartChecker(string log = "")
	{
		if (!IsSelfDisconnectLose())
		{
			if (IsSelfDisconnectLoseCheck())
			{
				this.OnDisconnectCheck.Call();
			}
			base.StartChecker();
		}
		if (_isAlreadyTriedSocketReplace)
		{
			if (this.OnBeforeDisconnectLose != null)
			{
				LocalLog.AccumulateLastTraceLog("SocketReplace Success");
			}
			_isAlreadyTriedSocketReplace = false;
		}
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if (!_isAlreadyTriedSocketReplace && (float)GetDisconnectTime() >= 50f)
		{
			if (!_isSocketOpenDisconnectLog && _networkBattleManagerBase.InstanceNetworkAgent != null && _networkBattleManagerBase.InstanceNetworkAgent.IsOpen())
			{
				_isSocketOpenDisconnectLog = true;
			}
			_isAlreadyTriedSocketReplace = true;
			if (this.OnBeforeDisconnectLose != null)
			{
				this.OnBeforeDisconnectLose.Call();
			}
		}
		if (IsSelfDisconnectLose())
		{
			this.OnDisconnectLose.Call();
			StopChecker();
		}
	}

	public int GetDisconnectTime()
	{
		return NetworkUtility.GetTimeSpanSecond(base.startTick);
	}
}
