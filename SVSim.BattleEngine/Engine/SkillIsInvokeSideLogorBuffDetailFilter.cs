using System.Collections.Generic;
using Wizard.Battle;

public class SkillIsInvokeSideLogorBuffDetailFilter : ISkillCardFilter
{
	private IReadOnlyBattleCardInfo _owner;

	private bool _flag;

	public SkillIsInvokeSideLogorBuffDetailFilter(IReadOnlyBattleCardInfo owner, bool flag)
	{
		_owner = owner;
		_flag = flag;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			BattleCardBase battleCardBase = card as BattleCardBase;
			BattlePlayerBase.SideLogInfo sideLogSkill = battleCardBase.SelfBattlePlayer.SideLogSkill;
			bool flag = false;
			if (sideLogSkill != null)
			{
				if ((sideLogSkill.Skill != null && sideLogSkill.Skill.IsInvoked) == _flag)
				{
					flag = true;
				}
			}
			else if (battleCardBase != null && ((battleCardBase.IsClass && battleCardBase.SelfBattlePlayer.IsBuffDetail == _flag) || battleCardBase.IsBuffDetail == _flag))
			{
				flag = true;
			}
			if ((!_owner.IsInplay && !_owner.IsInHand) == _flag)
			{
				flag = true;
			}
			if (flag)
			{
				list.Add(card);
			}
		}
		return list;
	}
}
