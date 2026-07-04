using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetInPlaySelfFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo m_ownerCard;

	public SkillTargetInPlaySelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		m_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from c in battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards)
			where c == m_ownerCard
			select c;
	}
}
