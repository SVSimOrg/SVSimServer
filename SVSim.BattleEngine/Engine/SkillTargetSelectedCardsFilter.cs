using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetSelectedCardsFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.SelectedCards == null)
		{
			return list;
		}
		List<IReadOnlyBattleCardInfo> list2 = new List<IReadOnlyBattleCardInfo>();
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoHandCards));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoDeckCards));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoCemeterys));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoBanishCards));
		foreach (SkillConditionCheckerOption.SkillAndSelectTarget card in option.SelectedCards)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = list2.FirstOrDefault((IReadOnlyBattleCardInfo c) => c.EquelsID(card.SelectCard));
			if (readOnlyBattleCardInfo != null)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
