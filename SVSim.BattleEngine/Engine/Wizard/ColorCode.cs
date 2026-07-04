using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public static class ColorCode
{

	private static Dictionary<eColorCodeId, Color> _colorCodeDic = new Dictionary<eColorCodeId, Color>();

	public static Color Get(eColorCodeId id)
	{
		if (_colorCodeDic.TryGetValue(id, out var value))
		{
			return value;
		}
		return Color.red;
	}
}
