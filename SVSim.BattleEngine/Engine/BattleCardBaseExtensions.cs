using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;
using Wizard.Battle.View;

public static class BattleCardBaseExtensions
{
	public static List<IBattleCardView> ConvertToViewList(this IList<BattleCardBase> battleCardBaseList)
	{
		return battleCardBaseList?.Select((BattleCardBase c) => c.BattleCardView).ToList();
	}

	public static BattleCardBase FindFromCardId(this IList<BattleCardBase> battleCardBaseList, IBattleCardUniqueID cardId)
	{
		if (battleCardBaseList == null)
		{
			return null;
		}
		for (int i = 0; i < battleCardBaseList.Count; i++)
		{
			BattleCardBase battleCardBase = battleCardBaseList[i];
			if (battleCardBase.EquelsID(cardId))
			{
				return battleCardBase;
			}
		}
		return null;
	}
}
