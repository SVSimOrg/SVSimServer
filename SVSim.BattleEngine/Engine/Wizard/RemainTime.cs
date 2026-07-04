using System;
using UnityEngine;

namespace Wizard;

public class RemainTime
{
	private double _endTime;

	private double _serverUnixTime;

	private float _sinceTime;

	public DateTime TimeInLocal { get; private set; }

	public string LocalTime { get; private set; }

	public RemainTime()
	{
	}

	public RemainTime(string endTime, double server)
	{
		_sinceTime = Time.realtimeSinceStartup;
		DateTime dateTime = DateTime.Parse(endTime);
		_endTime = ConvertTime.DateTimeToUnixTime(dateTime);
		_serverUnixTime = server;
		LocalTime = ConvertTime.ToLocal(dateTime).ToString();
		TimeInLocal = ConvertTime.ToLocalByDateTime(dateTime);
	}
}
