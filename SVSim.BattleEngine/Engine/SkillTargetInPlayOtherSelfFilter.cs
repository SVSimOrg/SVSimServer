using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetInPlayOtherSelfFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo m_ownerCard;

	public SkillTargetInPlayOtherSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		m_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from c in battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards)
			where c != m_ownerCard
			select c;
	}
}
