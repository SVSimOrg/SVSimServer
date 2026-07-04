using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cute;
using UnityEngine;

namespace Wizard;

public static class CardMasterLocalFileUtility
{
	private static readonly string CARDMASTER_DIRECTORY_NAME = "cardmaster";

	private static string CardMasterDirPath => Path.Combine(Application.persistentDataPath, CARDMASTER_DIRECTORY_NAME);

	private static string GetCardMasterFilePath(CardMaster.CardMasterId cardMasterId)
	{
		return Path.Combine(CardMasterDirPath, $"card_master_{(int)cardMasterId}");
	}

	public static string GetCardMasterHash()
	{
		string cardMasterFilePath = GetCardMasterFilePath(CardMaster.CardMasterId.Default);
		string result = string.Empty;
		try
		{
			if (!File.Exists(cardMasterFilePath))
			{
				return result;
			}
			using StreamReader streamReader = new StreamReader(cardMasterFilePath);
			result = CryptAES.decryptForNode(streamReader.ReadLine());
		}
		catch (Exception ex)
		{
			WriteClientInfoLog("CardMasater: \"" + cardMasterFilePath + "\" の読み込みに失敗\n" + ex);
			result = string.Empty;
		}
		return result;
	}

	private static void WriteClientInfoLog(string log)
	{
		LocalLog.AccumulateTraceLog(log);
	}
}
