using System;
using System.Globalization;

namespace Wizard;

public class ConvertTime
{
	public enum FORMAT
	{
		TIME_DATE_LONG	}

	private static string[] FORMAT_TEXT_ID = new string[7] { "System_TimeDateLong", "System_TimeDateShort", "System_DateLong", "System_DateShort", "System_Time", "System_YearMonth", "System_0068" };

	private static string ToLocal(DateTime dateTime, string outputFormat)
	{
		TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
		DateTime dateTime2 = dateTime + utcOffset;
		CultureInfo cultureInfo = new CultureInfo(Data.SystemText.Get("System_CultureInfo"), useUserOverride: false);
		return dateTime2.ToString(outputFormat, cultureInfo);
	}

	public static DateTime ToLocalByDateTime(DateTime dateTime)
	{
		TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
		return dateTime + utcOffset;
	}

	public static string ToLocal(DateTime dateTime, FORMAT outputFormat = FORMAT.TIME_DATE_LONG)
	{
		return ToLocal(dateTime, Data.SystemText.Get(FORMAT_TEXT_ID[(int)outputFormat])).Replace("12:00 AM", "12 midnight").Replace("12:00 PM", "12 noon").Replace("AM", "a.m.")
			.Replace("PM", "p.m.");
	}

	public static string ToLocal(DateTime beginDateTime, DateTime endDateTime, FORMAT outputFormat = FORMAT.TIME_DATE_LONG)
	{
		string text = ToLocal(beginDateTime, outputFormat);
		string text2 = ToLocal(endDateTime, outputFormat);
		return Data.SystemText.Get("System_Between", text, text2);
	}

	public static string GetLocalPeriod(DateTime beginDateTime, string endDateTime, FORMAT outputFormat = FORMAT.TIME_DATE_LONG)
	{
		string text = ToLocal(beginDateTime, outputFormat);
		string text2 = endDateTime;
		if (DateTime.TryParse(endDateTime, out var result))
		{
			text2 = ToLocal(result, outputFormat);
			return Data.SystemText.Get("System_Between", text, text2);
		}
		return Data.SystemText.Get("System_0064", text, text2);
	}

	public static DateTime UnixTimeToDateTime(int unixTime)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
	}

	public static double DateTimeToUnixTime(DateTime dateTime)
	{
		DateTime value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return dateTime.Subtract(value).TotalSeconds;
	}

	public static DateTime? GetDateTime(string endDateTime)
	{
		if (DateTime.TryParse(endDateTime, out var result))
		{
			return result;
		}
		return null;
	}

	public static TimeSpan GetTimeSpan(long startUnixTime, DateTime endDate)
	{
		long num = (long)DateTimeToUnixTime(endDate);
		if (num < startUnixTime)
		{
			num = startUnixTime;
		}
		return TimeSpan.FromSeconds(num - startUnixTime);
	}

	public static string GetRemainingTime(TimeSpan timeSpan)
	{
		string empty = string.Empty;
		if (timeSpan.Days >= 1)
		{
			return Data.SystemText.Get("Mission_0061", timeSpan.Days.ToString());
		}
		if (timeSpan.Hours >= 1)
		{
			return Data.SystemText.Get("Mission_0060", timeSpan.Hours.ToString());
		}
		if (timeSpan.Minutes >= 59)
		{
			return Data.SystemText.Get("Mission_0060", "1");
		}
		int minutes = timeSpan.Minutes;
		minutes++;
		return Data.SystemText.Get("Mission_0062", minutes.ToString());
	}
}
