using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityCantAttackAllFilter : ISkillCardFilter
{
	private readonly bool _hasCantAttack;

	public SkillAbilityCantAttackAllFilter(string op)
	{
		_hasCantAttack = op == "=";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.SkillApplyInformation.IsSkillCantAtkAll == _hasCantAttack)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
