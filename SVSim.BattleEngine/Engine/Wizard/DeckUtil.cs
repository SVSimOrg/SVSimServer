using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Wizard;

public class DeckUtil
{
	public static string CreateDefaultDeckName(ClassSet classSet, bool useSubClass, ICollection<string> otherDeckNames, Format format, MyRotationInfo myRotationInfo)
	{
		return CreateUniqueDeckName(CreateBaseDeckName(classSet, useSubClass, format, myRotationInfo), otherDeckNames);
	}

	private static string CreateBaseDeckName(ClassSet classSet, bool useSubClass, Format format, MyRotationInfo myRotationInfo)
	{
		if (format == Format.MyRotation)
		{
			return Data.SystemText.Get("MyRotation_ID_02", UIUtil.GetMyRotationDefaultDeckClassName(classSet.MainClass), myRotationInfo.LastPackText);
		}
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		if (useSubClass)
		{
			return $"{UIUtil.GetShortClassName(classSet.MainClass)}/{UIUtil.GetShortClassName(classSet.SubClass)}";
		}
		if (format == Format.Rotation)
		{
			string shortName = Data.Master.CardSetNameMgr.Get(Data.Load.data.RotationLatestCardPackId.ToString()).ShortName;
			return Data.SystemText.Get("MyRotation_ID_02", UIUtil.GetMyRotationDefaultDeckClassName(classSet.MainClass), shortName);
		}
		return dataMgr.GetClanNameByKey((int)classSet.MainClass);
	}

	private static string CreateUniqueDeckName(string baseName, ICollection<string> otherDeckNames)
	{
		int num = 2;
		string text = baseName;
		while (otherDeckNames.Contains(text))
		{
			text = $"{baseName} ({num++})";
		}
		return text;
	}

	public static bool IsDefaultDeckName(ClassSet classSet, bool useSubClass, string deckName, Format format, MyRotationInfo myRotationInfo)
	{
		string text = CreateBaseDeckName(classSet, useSubClass, format, myRotationInfo);
		if (!deckName.StartsWith(text, StringComparison.Ordinal))
		{
			return false;
		}
		string text2 = deckName.Substring(text.Length);
		if (text2.Length == 0)
		{
			return true;
		}
		Match match = Regex.Match(text2, "^\\s\\(([0-9]+)\\)$");
		if (!match.Success)
		{
			return false;
		}
		if (!int.TryParse(match.Groups[1].Value, out var result))
		{
			return false;
		}
		return result >= 2;
	}
}
