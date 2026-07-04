using System;
using System.Collections;
using UnityEngine;

namespace Cute;

public class TimeData
{
	private long serverTime;

	private long connectClientTime;

	public void Set(long setServerTime)
	{
		serverTime = setServerTime;
		connectClientTime = (long)TimeNativePlugin.GetDeviceOperatingTime();
	}

	public DateTime GetNowTime_UTC()
	{
		return TimeUtil.GetNowTime_UTC(serverTime, connectClientTime);
	}

	public float GetTimeLeftLong(long endTime)
	{
		return (float)TimeUtil.GetTimeLeft(serverTime, endTime, 0L).millisecond / 1000f;
	}
}
