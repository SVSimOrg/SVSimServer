using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace Wizard;

public class AvatarBattleAllInfo
{
	public class PeriodData
	{
	}

	private Dictionary<string, AvatarBattleInfo> _avatarBattleDictionary = new Dictionary<string, AvatarBattleInfo>();

	public AvatarBattleInfo Get(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (_avatarBattleDictionary.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}
}
