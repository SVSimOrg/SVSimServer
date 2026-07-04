using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Cute;
using UnityEngine;

namespace Wizard;

public static class UIUtil
{

	public static StringBuilder _tempStringBuilder = new StringBuilder(1024);

	public static StringBuilder GetTempStringBuilder()
	{
		_tempStringBuilder.Length = 0;
		return _tempStringBuilder;
	}

	public static void AddPositionY(Transform targetTransform, float addY)
	{
		Vector2 vector = targetTransform.localPosition;
		vector.y += addY;
		targetTransform.localPosition = vector;
	}

	public static void SetLocalPositionY(Transform targetTransform, float y)
	{
		Vector3 localPosition = targetTransform.localPosition;
		localPosition.y = y;
		targetTransform.localPosition = localPosition;
	}

	public static string GetFormatName(Format format)
	{
		return format switch
		{
			Format.Rotation => Data.SystemText.Get("Common_0154"), 
			Format.Unlimited => Data.SystemText.Get("Common_0155"), 
			Format.PreRotation => Data.SystemText.Get("Common_0163"), 
			Format.Sealed => Data.SystemText.Get("BattleName_Sealed"), 
			Format.Hof => Data.SystemText.Get("Colosseum_0108"), 
			Format.Crossover => Data.SystemText.Get("Common_0166"), 
			Format.MyRotation => Data.SystemText.Get("Common_0178"), 
			Format.Avatar => Data.SystemText.Get("HeroesBattle_0001"), 
			_ => string.Empty, 
		};
	}

	public static string GetShortClassName(CardBasePrm.ClanType clan)
	{
		switch (clan)
		{
		case CardBasePrm.ClanType.MIN:
			return Data.SystemText.Get("Common_0170");
		case CardBasePrm.ClanType.ROYAL:
			return Data.SystemText.Get("Common_0171");
		case CardBasePrm.ClanType.WITCH:
			return Data.SystemText.Get("Common_0172");
		case CardBasePrm.ClanType.DRAGON:
			return Data.SystemText.Get("Common_0173");
		case CardBasePrm.ClanType.NECRO:
			return Data.SystemText.Get("Common_0174");
		case CardBasePrm.ClanType.VAMPIRE:
			return Data.SystemText.Get("Common_0175");
		case CardBasePrm.ClanType.BISHOP:
			return Data.SystemText.Get("Common_0176");
		case CardBasePrm.ClanType.NEMESIS:
			return Data.SystemText.Get("Common_0177");
		default:
			Debug.LogError($"unsupported clan type : {clan}");
			return string.Empty;
		}
	}

	public static string GetMyRotationDefaultDeckClassName(CardBasePrm.ClanType clan)
	{
		switch (clan)
		{
		case CardBasePrm.ClanType.MIN:
			return Data.SystemText.Get("Common_0179");
		case CardBasePrm.ClanType.ROYAL:
			return Data.SystemText.Get("Common_0180");
		case CardBasePrm.ClanType.WITCH:
			return Data.SystemText.Get("Common_0181");
		case CardBasePrm.ClanType.DRAGON:
			return Data.SystemText.Get("Common_0182");
		case CardBasePrm.ClanType.NECRO:
			return Data.SystemText.Get("Common_0183");
		case CardBasePrm.ClanType.VAMPIRE:
			return Data.SystemText.Get("Common_0184");
		case CardBasePrm.ClanType.BISHOP:
			return Data.SystemText.Get("Common_0185");
		case CardBasePrm.ClanType.NEMESIS:
			return Data.SystemText.Get("Common_0186");
		default:
			Debug.LogError($"unsupported clan type : {clan}");
			return string.Empty;
		}
	}

	public static void AdjustClassInfoPartsSize(ClassInfoParts classInfoParts, FlexibleGrid grid, int widthMax)
	{
		grid.Reposition();
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(grid.transform, considerInactive: false);
		while (bounds.size.x > (float)widthMax)
		{
			int num = classInfoParts.ClassNameLabel.fontSize - 1;
			if (num <= 0)
			{
				Debug.LogError("invalid font size");
				break;
			}
			classInfoParts.ClassNameLabel.fontSize = num;
			if (classInfoParts.SubClassNameLabel != null)
			{
				classInfoParts.SubClassNameLabel.fontSize = num;
			}
			grid.Reposition();
			bounds = NGUIMath.CalculateRelativeWidgetBounds(grid.transform, considerInactive: false);
		}
		UIManager.GetInstance().StartCoroutine(grid.RepositionNextFrame());
	}

	public static string ExtractStringAlphabet(string str)
	{
		return Regex.Replace(str, "[^a-zA-z]", string.Empty);
	}

	public static int ExtractStringNumber(string str)
	{
		return int.Parse(Regex.Replace(str, "[^0-9]", string.Empty));
	}
}
