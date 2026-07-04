using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDeckSelfFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetDeckSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from c in battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoDeckCards)
			where c == _ownerCard
			select c;
	}
}
