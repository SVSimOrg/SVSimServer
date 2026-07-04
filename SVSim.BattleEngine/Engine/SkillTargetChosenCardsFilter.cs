using System.Collections.Generic;
using Wizard;
using Wizard.Battle;

public class SkillTargetChosenCardsFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.ChosenCards == null)
		{
			return list;
		}
		foreach (int chosenCard in option.ChosenCards)
		{
			if (CardMaster.GetInstanceForBattle().CardExists(chosenCard))
			{
				BattleCardBase item = option.PlayedCard.SelfBattlePlayer.BattleMgr.CreateTransformCardRegisterVfx(option.PlayedCard, chosenCard, option.PlayedCard.IsPlayer, null, isRecoveryFinish: false, isChoice: true);
				list.Add(item);
			}
		}
		return list;
	}
}
