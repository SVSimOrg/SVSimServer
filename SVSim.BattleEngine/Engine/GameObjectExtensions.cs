using System.Linq;
using UnityEngine;

public static class GameObjectExtensions
{
	public static BattleCardBase GetBattleCard(this GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		BattleCardBase battleCardBase = null;
		BattleCardBase battleCardBase2 = null;
		battleCardBase = null; // Pre-Phase-5b: static lookup unreachable headless
		battleCardBase2 = null; // Pre-Phase-5b: static lookup unreachable headless
		if (battleCardBase != null)
		{
			return battleCardBase;
		}
		return battleCardBase2;
	}
}
