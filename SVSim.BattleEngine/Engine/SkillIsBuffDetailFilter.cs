using System.Collections.Generic;
using Wizard.Battle;

public class SkillIsBuffDetailFilter : ISkillCardFilter
{
	private bool _flag;

	public SkillIsBuffDetailFilter(bool flag)
	{
		_flag = flag;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			BattleCardBase battleCardBase = card as BattleCardBase;
			if (battleCardBase.SelfBattlePlayer.SideLogSkill != null)
			{
				return list;
			}
			if (battleCardBase != null && ((battleCardBase.IsClass && battleCardBase.SelfBattlePlayer.IsBuffDetail == _flag) || battleCardBase.IsBuffDetail == _flag))
			{
				list.Add(card);
			}
		}
		return list;
	}
}
