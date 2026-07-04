using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetFightTargetFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetFightTargetFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.AttackTargetCard == null && option.AttackerCard == null)
		{
			return list;
		}
		list.Add((_ownerCard == option.AttackerCard) ? option.AttackTargetCard : option.AttackerCard);
		return list;
	}
}
