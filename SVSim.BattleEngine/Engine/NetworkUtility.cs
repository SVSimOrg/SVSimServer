using System;
using Cute;

public static class NetworkUtility
{
	public static int GetTimeSpanSecond(long oldTimer)
	{
		if (oldTimer == 0L)
		{
			return 0;
		}
		long ticks = TimeUtil.GetAbsoluteTime().Ticks - oldTimer;
		return (int)new TimeSpan(ticks).TotalSeconds;
	}
}
