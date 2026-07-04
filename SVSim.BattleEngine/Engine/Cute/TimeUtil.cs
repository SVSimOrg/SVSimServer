using System;
// TODO(engine-cleanup-pass2): 11 of 12 methods unrun in baseline
//   Type: Cute.TimeUtil
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Cute;

public class TimeUtil
{
	public struct TimeLeftParam
	{
		public int day;

		public int hour;

		public int minute;

		public int second;

		public int millisecond;

		public int count;

		public bool isEnd;

		public bool isCharge;

		public TimeLeftParam(long unixTime, long consumingTime)
		{
			if (unixTime <= 0)
			{
				isEnd = true;
				count = 0;
				day = 0;
				hour = 0;
				minute = 0;
				second = 0;
				millisecond = 0;
				isCharge = true;
				return;
			}
			isEnd = false;
			if (consumingTime == 0L)
			{
				count = 0;
				isCharge = false;
			}
			else
			{
				count = (int)(unixTime / consumingTime) + 1;
				unixTime = unixTime - (count - 1) * consumingTime + 1;
				if (unixTime == consumingTime)
				{
					isCharge = true;
				}
				else
				{
					isCharge = false;
				}
			}
			day = (int)unixTime / 86400;
			hour = (int)unixTime / 3600 % 24;
			minute = (int)unixTime / 60 % 60;
			second = (int)unixTime % 60;
			millisecond = (int)unixTime % 1000;
		}

		public override string ToString()
		{
			return $"回数{count:D2}残り{day:D2}日 {hour:D2}:{minute:D2}:{second:D2}";
		}
	}

	private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);

	public static DateTime GetNowTime_UTC(long serverTime, long connectClientTime)
	{
		return UNIX_EPOCH.AddSeconds((float)serverTime + TimeNativePlugin.GetDeviceOperatingTime() - (float)connectClientTime);
	}

	public static DateTime GetAbsoluteTime()
	{
		return UNIX_EPOCH.AddSeconds(TimeNativePlugin.GetDeviceOperatingTime());
	}

	public static TimeLeftParam GetTimeLeft(long nowTime, long endTime, long consumingTime = 0L)
	{
		return new TimeLeftParam(endTime - nowTime, consumingTime);
	}

	public static TimeSpan GetElapsedTimeByTimeSpan(DateTime baseDateTime, DateTime dateTime)
	{
		return baseDateTime - dateTime;
	}
}
