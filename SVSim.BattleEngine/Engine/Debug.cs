using System;
using System.Diagnostics;
using UnityEngine;
using Wizard;

public static class Debug
{

	public static void LogError(object message, UnityEngine.Object context = null)
	{
		LocalLog.AccumulateTraceLog(message.ToString());
	}
}
