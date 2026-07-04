using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace Wizard;

public class MyRotationAllInfo
{
	public class PeriodData
	{

		public double StartUnixTime { get; set; }

		public double EndUnixTime { get; set; }
	}

	private Dictionary<string, MyRotationInfo> _myRotationDictionary = new Dictionary<string, MyRotationInfo>();

	public PeriodData FreeMatchPeriod = new PeriodData();

	private bool _myRotationScheduleExist;

	private float _receiveSinceTime;

	private double _receiveServerUnixTime;

	public List<MyRotationInfo> MyRotationInfoList { get; } = new List<MyRotationInfo>();

	public List<MyRotationAbilityGroup> AbilityGroup { get; private set; } = new List<MyRotationAbilityGroup>();

	public List<string> DisableCardPackIdList { get; private set; } = new List<string>();

	public bool IsMyRotationEnable => IsWithinPeriod(FreeMatchPeriod);

	public MyRotationInfo Get(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (_myRotationDictionary.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	private bool IsWithinPeriod(PeriodData period)
	{
		if (!_myRotationScheduleExist)
		{
			return false;
		}
		double num = _receiveServerUnixTime + (double)Time.realtimeSinceStartup - (double)_receiveSinceTime;
		if (num >= period.EndUnixTime)
		{
			return false;
		}
		return num > period.StartUnixTime;
	}
}
