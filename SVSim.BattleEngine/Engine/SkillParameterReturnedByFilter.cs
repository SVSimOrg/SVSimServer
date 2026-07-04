using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterReturnedByFilter : ISkillCardFilter
{
	private BattleCardBase _ownerCard;

	private bool _isPlayer;

	public SkillParameterReturnedByFilter(IReadOnlyBattleCardInfo ownerCard, SkillFilterCreator.ContentKeyword keyword)
	{
		_ownerCard = ownerCard as BattleCardBase;
		_isPlayer = keyword == SkillFilterCreator.ContentKeyword.self_ability;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.ReturnedSkill != null && readOnlyBattleCardInfo.ReturnedSkill.SkillPrm.ownerCard.SelfBattlePlayer.IsPlayer == _ownerCard.SelfBattlePlayer.IsPlayer == _isPlayer)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
