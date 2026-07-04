using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetDrewSkillFilter : ISkillCardFilter
{
	private BattleCardBase _ownerCard;

	private bool _isSelf;

	public SkillTargetDrewSkillFilter(IReadOnlyBattleCardInfo ownerCard, string option)
	{
		_ownerCard = ownerCard as BattleCardBase;
		_isSelf = option == "self";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (_isSelf && _ownerCard.SelfBattlePlayer.DrewSkillCard == card)
			{
				list.Add(card);
			}
		}
		return list;
	}
}
