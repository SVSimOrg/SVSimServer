using System;
using LitJson;
using UnityEngine;

namespace Wizard;

public class TreasureBoxCp
{

	private bool _treasureBoxCpNotExist = true;

	private double _receiveServerUnixTime;

	private double _receiveSinceTime;

	public bool IsReceivable { get; private set; }

	public double StartUnixTime { get; private set; }

	public double EndUnixTime { get; private set; }

	public int CurrentGrade { get; private set; }

	public int NextGrade { get; private set; }

	public int MaxGrade { get; private set; }

	public int CurrentMemory { get; private set; }

	public int MaxMemory { get; private set; }

	public void Parse(JsonData data, JsonData headerData)
	{
		_treasureBoxCpNotExist = false;
		IsReceivable = false;
		if (data.Keys.Contains("is_receivable"))
		{
			IsReceivable = data["is_receivable"].ToInt() == 1;
		}
		StartUnixTime = ConvertTime.DateTimeToUnixTime(DateTime.Parse(data["start_time"].ToString()));
		EndUnixTime = ConvertTime.DateTimeToUnixTime(DateTime.Parse(data["end_time"].ToString()));
		CurrentGrade = data["current_grade"].ToInt();
		NextGrade = data["next_grade"].ToInt();
		CurrentMemory = data["current_memory"].ToInt();
		MaxGrade = data["max_grade"].ToInt();
		MaxMemory = data["max_memory"].ToInt();
		_receiveServerUnixTime = headerData["servertime"].ToDouble();
		_receiveSinceTime = Time.realtimeSinceStartup;
	}
}
