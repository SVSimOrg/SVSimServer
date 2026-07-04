using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetReturnSkillFilter : ISkillCardFilter
{
	private BattleCardBase _ownerCard;

	private bool _isSelf;

	public SkillTargetReturnSkillFilter(IReadOnlyBattleCardInfo ownerCard, string option)
	{
		_ownerCard = ownerCard as BattleCardBase;
		_isSelf = option == "self";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			SkillBase returnedSkill = _ownerCard.ReturnedSkill;
			if (_isSelf && !returnedSkill.IsAttachedSkill && returnedSkill.SkillPrm.ownerCard == cards.ElementAt(i))
			{
				list.Add(cards.ElementAt(i));
			}
		}
		return list;
	}
}
