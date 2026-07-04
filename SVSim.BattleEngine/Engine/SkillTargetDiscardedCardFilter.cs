using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDiscardedCardFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetDiscardedCardFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoDiscards.Where((IReadOnlyBattleCardInfo pp) => pp.DiscardedSkill.SkillPrm.ownerCard == _ownerCard)));
		return list;
	}
}
