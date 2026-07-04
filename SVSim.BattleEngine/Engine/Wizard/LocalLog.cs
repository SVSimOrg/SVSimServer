using System;
using System.IO;
using System.Text;
using Cute;
using UnityEngine;
using Wizard.RoomMatch;
// TODO(engine-cleanup-pass2): 37 of 55 methods unrun in baseline
//   Type: Wizard.LocalLog
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class LocalLog
{
	// HEADLESS-PATCH (engine-port): all public mutating entry points + the private file-write
	// helpers are gated by a single static lock so concurrent battle setups (fixture-parallel
	// tests, parallel SessionBattleEngine.Setup() calls) don't corrupt the StringBuilder /
	// string accumulators (FormatException, lost frames) or interleave writes to the four
	// scratch log files. Logging is not the hot path; global serialization is acceptable.
	private static readonly object _gate = new object();

	public enum TRACELOG_TYPE
	{
		TRACE_ALL_LOG,
		TRACE_LOG,
		TRACE_LAST_LOG	}

	public enum RecordType
	{
	}

	private static string AccumulateLogPath = Application.persistentDataPath + "/accumulate_log";

	private static string AccumulateSettingLogPath = Application.persistentDataPath + "/setting_info_log";

	private static string LastAccumulate1LogPath = Application.persistentDataPath + "/last_accumulate_log1";

	private static string LastAccumulate2LogPath = Application.persistentDataPath + "/last_accumulate_log2";

	private static string InquiryLogPath = Application.persistentDataPath + "/inquiry_log";

	private static string _failureWriteClientLog = "";

	private static int nowTraceTurn = 0;

	private static int currentTurn = -1;

	private static string _loadResourceLog = "";

	private static string _gungnirLog = "";

	private static string _socketFrameLog = "";

	private static string _disconnectLog = "";

	public static bool _isSendGungnirLog = false;

	public static bool _isSendSocketFrameLog = false;

	private static bool _isLastTraceLogTimeAdd = false;

	private static StringBuilder _lastTraceLogStringBuilder = null;

	[RuntimeInitializeOnLoadMethod]
	public static void CreateLogFile()
	{
		lock (_gate)
		{
			CreateLocalLogFile(AccumulateLogPath);
			CreateLocalLogFile(AccumulateSettingLogPath);
			CreateLocalLogFile(LastAccumulate1LogPath);
			CreateLocalLogFile(LastAccumulate2LogPath);
			CreateLocalLogFile(InquiryLogPath);
		}
	}

	public static void CreateLocalLogFile(string filePath)
	{
		lock (_gate)
		{
			FileStream fileStream = null;
			try
			{
				if (!File.Exists(filePath))
				{
					using (fileStream = File.Create(filePath))
					{
						fileStream.Close();
						return;
					}
				}
			}
			catch
			{
				string text = "FailedToCreateFile:" + filePath;
				fileStream?.Dispose();
				_failureWriteClientLog = _failureWriteClientLog + text + "\n";
			}
		}
	}

	public static void SendLastTraceLog(Action onSended)
	{
		MakeTreceLogToSend(TRACELOG_TYPE.TRACE_LAST_LOG, onSended);
	}

	private static void MakeTreceLogToSend(TRACELOG_TYPE logType, Action onSended)
	{
		lock (_gate)
		{
			MakeTreceLogToSendLocked(logType, onSended);
		}
	}

	private static void MakeTreceLogToSendLocked(TRACELOG_TYPE logType, Action onSended)
	{
		if (string.IsNullOrEmpty(Certification.Udid) /* Pre-Phase-5 chunk 39: Certification.ViewerId dropped — LocalLog is static-scope */)
		{
			onSended.Call();
			return;
		}
		string log = string.Empty;
		string text = string.Empty;
		string log2 = string.Empty;
		string log3 = string.Empty;
		bool flag = false;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = string.Empty;
		string text6 = string.Empty;
		switch (logType)
		{
		case TRACELOG_TYPE.TRACE_ALL_LOG:
			text2 = ReadLogFile(AccumulateLogPath);
			text2 += _failureWriteClientLog;
			text3 = ReadLogFile(LastAccumulate1LogPath);
			text4 = ReadLogFile(LastAccumulate2LogPath);
			text5 = ReadLogFile(AccumulateSettingLogPath);
			text6 = ReadLogFile(InquiryLogPath);
			break;
		case TRACELOG_TYPE.TRACE_LOG:
			text2 = ReadLogFile(AccumulateLogPath);
			text2 += _failureWriteClientLog;
			text5 = ReadLogFile(AccumulateSettingLogPath);
			break;
		case TRACELOG_TYPE.TRACE_LAST_LOG:
			text3 = ReadLogFile(LastAccumulate1LogPath);
			text4 = ReadLogFile(LastAccumulate2LogPath);
			break;
		}
		if (!string.IsNullOrEmpty(text2) || !string.IsNullOrEmpty(text3) || !string.IsNullOrEmpty(text4) || !string.IsNullOrEmpty(text5) || !string.IsNullOrEmpty(text6))
		{
			flag = true;
		}
		if (flag)
		{
			if (GetBattleAndViewIdText() != string.Empty)
			{
				log = GetBattleAndViewIdText() + "\n";
			}
			switch (logType)
			{
			case TRACELOG_TYPE.TRACE_ALL_LOG:
				log = text2;
				if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach; log skips battle-id/socket-id
				{
					text = text + "bId0\n"; /* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
				}
				text += AppendLastLog(text3, text4);
				log2 = text5;
				log3 = "InquiryLog:" + text6;
				break;
			case TRACELOG_TYPE.TRACE_LOG:
				log = text2;
				log2 = text5;
				break;
			case TRACELOG_TYPE.TRACE_LAST_LOG:
				text += AppendLastLog(text3, text4);
				if (_isSendGungnirLog)
				{
					text = text + "\nGungnirLog:==================\n" + _gungnirLog;
					InitGungnirLog();
				}
				if (_isSendSocketFrameLog)
				{
					text = text + "\nSocketFrameLog:==================\n" + _socketFrameLog;
					InitSocketFrameLog();
				}
				if (!string.IsNullOrEmpty(_disconnectLog))
				{
					text = text + "\nDisconnectLog:==================\n" + _disconnectLog;
					InitDisconnectLog();
				}
				break;
			}
			switch (logType)
			{
			case TRACELOG_TYPE.TRACE_LAST_LOG:
				if (IsLogNullOrEmpty(text))
				{
					onSended.Call();
				}
				else
				{
					SendTraceLog(logType, string.Empty, GetShapedLog(text), string.Empty, string.Empty, onSended);
				}
				break;
			case TRACELOG_TYPE.TRACE_LOG:
				if (IsLogNullOrEmpty(log) && IsLogNullOrEmpty(log2))
				{
					onSended.Call();
				}
				else
				{
					SendTraceLog(logType, GetShapedLog(log), string.Empty, GetShapedLog(log2), string.Empty, onSended);
				}
				break;
			case TRACELOG_TYPE.TRACE_ALL_LOG:
				if (IsLogNullOrEmpty(log) && IsLogNullOrEmpty(text) && IsLogNullOrEmpty(log2) && IsLogNullOrEmpty(log3))
				{
					onSended.Call();
				}
				else
				{
					SendTraceLog(logType, GetShapedLog(log), GetShapedLog(text), GetShapedLog(log2), GetShapedLog(log3), onSended);
				}
				break;
			}
		}
		else
		{
			onSended.Call();
		}
	}

	public static void RecordResouseLoadError(int errorFlag)
	{
		lock (_gate)
		{
			UIManager.ViewScene currentScene = UIManager.GetInstance().GetCurrentScene();
			string text = (false /* Pre-Phase-5 (chunk 38): RTA has no static reach */ ? "NetworkBattle" : currentScene.ToString());
			AccumulateTraceLogLocked("ResourcesManager ParallelAssetListExec Error in " + text + " : " + errorFlag);
		}
	}

	public static void RecordTurnEndIfLoadErrorOccured()
	{
		lock (_gate)
		{
			if (ExistResourceLoadErrorInNetWorkBattleLocked())
			{
				AccumulateTraceLogLocked("TurnEnd After LoadError");
			}
		}
	}

	private static bool ExistResourceLoadErrorInNetWorkBattleLocked()
	{
		return ReadLogFile(AccumulateLogPath).Contains("ResourcesManager ParallelAssetListExec Error in NetworkBattle");
	}

	private static string AppendLastLog(string append_log1, string append_log2)
	{
		string text = "";
		if (nowTraceTurn != 0)
		{
			text += append_log1;
			return text + append_log2;
		}
		text += append_log2;
		return text + append_log1;
	}

	private static bool IsLogNullOrEmpty(string log)
	{
		return string.IsNullOrEmpty(log.Replace("\n", "").Replace("\r", ""));
	}

	private static string GetShapedLog(string log)
	{
		log = log.Replace("\n", "&&").Replace("\r", "");
		string text = "ID:" + 0 + " {\n" + log + " \n}";
		text.Replace("\n", "\\n");
		return text;
	}

	private static void SendTraceLog(TRACELOG_TYPE logType, string log, string last_log, string setting_log, string inquiry_log, Action onSended)
	{
		bool showLoadingIcon = false;
		SendTraceLogTask sendTraceLogTask = new SendTraceLogTask(logType);
		if (logType == TRACELOG_TYPE.TRACE_LAST_LOG)
		{
			sendTraceLogTask.SetParameter(last_log);
		}
		else if (logType == TRACELOG_TYPE.TRACE_LOG)
		{
			sendTraceLogTask.SetParameter(log, setting_log);
		}
		else
		{
			showLoadingIcon = true;
			sendTraceLogTask.SetParameter(log + inquiry_log, last_log, setting_log);
		}
		sendTraceLogTask.SkipCuteHttpStatusErrorPopup();
		sendTraceLogTask.SkipCuteTimeOutPopup();
		sendTraceLogTask.SkipAllCuteResultCodeCheckErrorPopup();
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(sendTraceLogTask, delegate
		{
			ClearLogByType(logType);
			onSended.Call();
		}, delegate
		{
			onSended.Call();
		}, delegate
		{
			onSended.Call();
		}, encrypt: true, useJson: false, showLoadingIcon));
	}

	public static void AccumulateTraceLog(string log)
	{
		lock (_gate)
		{
			AccumulateTraceLogLocked(log);
		}
	}

	private static void AccumulateTraceLogLocked(string log)
	{
		AccumulateLog(GetBattleAndViewIdText() + log, "TraceLog");
	}

	public static void AccumulateTraceInquiryLog(string log)
	{
		lock (_gate)
		{
			AccumulateLog(log, "TraceInquiryLog");
			OrganizeInquiryLog();
		}
	}

	public static void AccumulateSettingLog()
	{
		lock (_gate)
		{
			if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach
			{
				string battleAndViewIdText = GetBattleAndViewIdText();
				string text = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_BATTLE_EFFECT) ? "1" : "0");
				string text2 = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.CONFIRM_TURN_END) ? "1" : "0");
				string text3 = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.CONFIRM_EVOLVE) ? "1" : "0");
				string text4 = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.IS_SELECT_WSS) ? "1" : "0");
				AccumulateLog(battleAndViewIdText + "BattleSetting:" + text + text2 + text3 + text4, "TraceSettingLog");
			}
		}
	}

	public static void AddGungnirLog(string log)
	{
		lock (_gate)
		{
			if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach; log skips battle-id/socket-id
			{
				DateTime dateTime = DateTime.Now.ToUniversalTime();
				log = dateTime.Day + "/" + dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + ":" + dateTime.Millisecond.ToString("000") + ":[0]" + log + "\n"; /* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
				_gungnirLog += log;
			}
		}
	}

	public static void InitGungnirLog()
	{
		lock (_gate)
		{
			_gungnirLog = "";
			if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach
			{
				/* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
			}
			_isSendGungnirLog = false;
		}
	}

	public static void InitSocketFrameLog()
	{
		lock (_gate)
		{
			_socketFrameLog = "";
			if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach
			{
				/* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
			}
			_isSendSocketFrameLog = false;
		}
	}

	public static void UpdateLoadResourceLog(string log)
	{
		lock (_gate)
		{
			_loadResourceLog = log;
		}
	}

	public static void SetDisconnectLog(string log)
	{
		lock (_gate)
		{
			if (_disconnectLog == "")
			{
				DateTime dateTime = DateTime.Now.ToUniversalTime();
				log = dateTime.Day + "/" + dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + ":" + dateTime.Millisecond.ToString("000") + ":[0]" + log + "\n"; /* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
				_disconnectLog = log;
			}
		}
	}

	public static void InitDisconnectLog()
	{
		lock (_gate)
		{
			_disconnectLog = "";
		}
	}

	public static void AccumulateLastTraceLog(string log)
	{
		lock (_gate)
		{
			if (_lastTraceLogStringBuilder == null)
			{
				_lastTraceLogStringBuilder = new StringBuilder();
			}
			else
			{
				_lastTraceLogStringBuilder.Append("\n");
			}
			float num = 0f;
			if (num != 0f && num / (float)SystemInfo.systemMemorySize > 0.8f)
			{
				string text = "";
				log += text;
			}
			if (!_isLastTraceLogTimeAdd)
			{
				DateTime dateTime = DateTime.Now.ToUniversalTime();
				_lastTraceLogStringBuilder.AppendFormat("{0}/{1}:{2}:{3}:{4:000}:{5}", dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond.ToString("000"), "\n" + log);
				_isLastTraceLogTimeAdd = true;
			}
			else
			{
				_lastTraceLogStringBuilder.Append(log);
			}
		}
	}

	public static void SubmitAccumulateLastTraceLog()
	{
		lock (_gate)
		{
			SubmitAccumulateLastTraceLogLocked();
		}
	}

	private static void SubmitAccumulateLastTraceLogLocked()
	{
		if (_lastTraceLogStringBuilder != null)
		{
			string key = "";
			if (nowTraceTurn == 0)
			{
				key = "LastTraceLog1";
			}
			else if (nowTraceTurn == 1)
			{
				key = "LastTraceLog2";
			}
			AccumulateLog(_lastTraceLogStringBuilder.ToString(), key, isTimeEnable: false);
			_isLastTraceLogTimeAdd = false;
			_lastTraceLogStringBuilder = null;
		}
	}

	public static void SetLastTraceLogTurn(int turn)
	{
		lock (_gate)
		{
			if (currentTurn != turn)
			{
				string log = "Turn" + turn + " " + GetBattleAndViewIdText() + "====\n";
				string text = "";
				if (turn % 2 == 0)
				{
					text = "LastTraceLog1";
					nowTraceTurn = 0;
				}
				else
				{
					text = "LastTraceLog2";
					nowTraceTurn = 1;
				}
				if (turn != 0)
				{
					ClearLog(text);
				}
				WriteAccumulateTraceLog(text, log);
				currentTurn = turn;
			}
		}
	}

	private static string ReadLogFile(string filePath)
	{
		SubmitAccumulateLastTraceLog();
		string result = "";
		StreamReader streamReader = null;
		try
		{
			if (!File.Exists(filePath))
			{
				CreateLogFile();
			}
			using (streamReader = new StreamReader(filePath))
			{
				result = streamReader.ReadToEnd();
				streamReader.Close();
				streamReader.Dispose();
			}
		}
		catch (Exception ex)
		{
			streamReader?.Dispose();
			string text = "FailedToReadFile:" + filePath + "(" + ex?.ToString() + ")";
			Debug.LogError(text);
			_failureWriteClientLog = _failureWriteClientLog + text + "\n";
		}
		return result;
	}

	private static void WriteAccumulateTraceLog(string key, string log)
	{
		string text = "";
		switch (key)
		{
		default:
			return;
		case "TraceLog":
			text = AccumulateLogPath;
			break;
		case "LastTraceLog1":
			text = LastAccumulate1LogPath;
			break;
		case "LastTraceLog2":
			text = LastAccumulate2LogPath;
			break;
		case "TraceSettingLog":
			text = AccumulateSettingLogPath;
			break;
		case "TraceInquiryLog":
			text = InquiryLogPath;
			break;
		}
		try
		{
			if ((float)new FileInfo(text).Length / 1024f > 500f)
			{
				return;
			}
		}
		catch
		{
			return;
		}
		StreamWriter streamWriter = null;
		try
		{
			using (streamWriter = new StreamWriter(text, append: true))
			{
				streamWriter.WriteLine(log);
				streamWriter.Flush();
				streamWriter.Close();
			}
		}
		catch (Exception ex)
		{
			string text2 = "FailedToWriteFile" + ex.ToString();
			Debug.LogError(text2);
			streamWriter?.Dispose();
			if (key == "TraceLog")
			{
				_failureWriteClientLog = _failureWriteClientLog + text2 + " " + log + "\n";
			}
		}
	}

	private static void OrganizeInquiryLog()
	{
		try
		{
			FileInfo fileInfo = new FileInfo(InquiryLogPath);
			if (fileInfo != null && (float)fileInfo.Length / 1024f > 5f)
			{
				string text = ReadLogFile(InquiryLogPath);
				string log = text.Substring((int)((float)text.Length - 2560f));
				ClearInquiryLogKey();
				AccumulateTraceInquiryLog(log);
			}
		}
		catch
		{
		}
	}

	private static void AccumulateLog(string log, string key, bool isTimeEnable = true)
	{
		if (!string.IsNullOrEmpty(Environment.UserName) && log.IndexOf("Users") >= 0)
		{
			if (log.IndexOf(Environment.UserName) >= 0)
			{
				log = log.Replace(Environment.UserName, "Environment_UserName");
			}
			if (log.IndexOf(Environment.UserName.ToLower()) >= 0)
			{
				log = log.Replace(Environment.UserName.ToLower(), "Environment_UserName");
			}
			if (log.IndexOf(Environment.UserName.ToUpper()) >= 0)
			{
				log = log.Replace(Environment.UserName.ToUpper(), "Environment_UserName");
			}
		}
		_ = Toolbox.DebugManager != null;
		if (isTimeEnable)
		{
			DateTime dateTime = DateTime.Now.ToUniversalTime();
			log = dateTime.Day + "/" + dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + ":" + dateTime.Millisecond.ToString("000") + ":" + log;
		}
		try
		{
			WriteAccumulateTraceLog(key, log);
		}
		catch
		{
		}
	}

	private static void ClearLogByType(TRACELOG_TYPE logType)
	{
		lock (_gate)
		{
			switch (logType)
			{
			case TRACELOG_TYPE.TRACE_ALL_LOG:
				_failureWriteClientLog = "";
				ClearTraceLogLocked();
				ClearLastLogKeyLocked();
				ClearInquiryLogKey();
				break;
			case TRACELOG_TYPE.TRACE_LOG:
				_failureWriteClientLog = "";
				ClearTraceLogLocked();
				break;
			case TRACELOG_TYPE.TRACE_LAST_LOG:
				ClearLastLogKeyLocked();
				break;
			}
		}
	}

	private static void ClearTraceLogLocked()
	{
		ClearLog("TraceLog");
		ClearLog("TraceSettingLog");
	}

	private static void ClearInquiryLogKey()
	{
		ClearLog("TraceInquiryLog");
	}

	private static void ClearLog(string key)
	{
		string text = "";
		switch (key)
		{
		default:
			return;
		case "TraceLog":
			text = AccumulateLogPath;
			break;
		case "LastTraceLog1":
			text = LastAccumulate1LogPath;
			break;
		case "LastTraceLog2":
			text = LastAccumulate2LogPath;
			break;
		case "TraceSettingLog":
			text = AccumulateSettingLogPath;
			break;
		case "TraceInquiryLog":
			text = InquiryLogPath;
			break;
		}
		StreamWriter streamWriter = null;
		try
		{
			using (streamWriter = new StreamWriter(text))
			{
				streamWriter.WriteLine("");
				streamWriter.Flush();
				streamWriter.Close();
			}
		}
		catch
		{
			Debug.LogError("Failed to clear log");
			streamWriter?.Dispose();
		}
	}

	private static void ClearLastLogKeyLocked()
	{
		ClearLog("LastTraceLog1");
		ClearLog("LastTraceLog2");
		currentTurn = -1;
	}

	private static string GetBattleAndViewIdText()
	{
		string battleIdText = GetBattleIdText();
		string viewIdText = GetViewIdText();
		if (battleIdText.IsNotNullOrEmpty() || viewIdText.IsNotNullOrEmpty())
		{
			return "[bid " + battleIdText + " vid " + viewIdText + "]";
		}
		return string.Empty;
	}

	private static string GetBattleIdText()
	{
		if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach; log skips battle-id/socket-id
		{
			long battleId = 0; /* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
			if (battleId == -1)
			{
				return string.Empty;
			}
			return battleId.ToString();
		}
		return string.Empty;
	}

	private static string GetViewIdText()
	{
		if (false) // Pre-Phase-5 (chunk 38): RTA has no static reach; log skips battle-id/socket-id
		{
			int viewId = 0; /* Pre-Phase-5 chunk 38: dead branch (RTA static removed) */
			if (viewId == 0)
			{
				return string.Empty;
			}
			return viewId.ToString();
		}
		return string.Empty;
	}
}
