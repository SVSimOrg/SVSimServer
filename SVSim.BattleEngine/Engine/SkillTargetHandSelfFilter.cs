using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetHandSelfFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetHandSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from c in battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoHandCards)
			where c == _ownerCard
			select c;
	}
}
