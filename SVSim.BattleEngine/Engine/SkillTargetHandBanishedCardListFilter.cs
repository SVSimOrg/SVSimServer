using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetHandBanishedCardListFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoBanishCards.Where((IReadOnlyBattleCardInfo pp) => !(pp is NullBattleCard) && pp.BanishedInfo.Place == BattleCardBase.BanishInfo.BanishPlace.Hand)));
		return list;
	}
}
