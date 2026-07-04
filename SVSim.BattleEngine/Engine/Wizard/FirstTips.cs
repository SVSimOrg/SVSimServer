using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class FirstTips : MonoBehaviour
{
	public enum TipsType
	{
		Deck = 0,
		Battle = 4,
		ColosseumInfo = 17,
		Quest = 25,
		AdditionalPuzzle = 26,
		Crossover = 28,
		MyRotationDeck = 33,
		BossRush = 34,
		Max = 46,
		MyPage = 1001	}

	protected enum Csv
	{
	}

	private static bool IsAllwaysDispaly(TipsType in_TipsType)
	{
		if (in_TipsType > TipsType.Max)
		{
			return true;
		}
		return false;
	}

	public static bool IsFirstTipsOpen(TipsType in_TipsType)
	{
		if (IsAllwaysDispaly(in_TipsType))
		{
			return true;
		}
		if (in_TipsType == TipsType.ColosseumInfo)
		{
			return true;
		}
		if ((Fix(long.Parse(PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.FIRST_TIPS))) & (1L << (int)in_TipsType)) != 0L)
		{
			return false;
		}
		return true;
	}

	public static long Fix(long value)
	{
		if (value < 0)
		{
			long num = Convert.ToInt64("0x00000000ffffffff", 16);
			value &= num;
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.FIRST_TIPS, value.ToString());
		}
		return value;
	}
}
