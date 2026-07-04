using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetTurnTokenDrawSkillIdFilter : ISkillTargetFilter
{
	private int _id;

	public SkillTargetTurnTokenDrawSkillIdFilter(string id)
	{
		_id = int.Parse(id);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(from c in battlePlayerInfo.SkillInfoTurnDrawTokenCardsWithId
				where c.Id == _id
				select c.Card);
		}
		return list;
	}
}
