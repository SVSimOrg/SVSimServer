using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSelectRemainActionCountFromSelfFilter : ISkillParameterSelectFilter
{
	private IReadOnlyBattleCardInfo _originOwnerCard;

	public SkillParameterSelectRemainActionCountFromSelfFilter(IReadOnlyBattleCardInfo card)
	{
		_originOwnerCard = card;
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		List<int> list = new List<int>();
		if (_originOwnerCard != null)
		{
			for (int i = 0; i < cardInfos.Count(); i++)
			{
				BattleCardBase card = cardInfos.ElementAt(i) as BattleCardBase;
				IEnumerable<SkillPreprocessBase> source = card.Skills.Where((SkillBase s) => _originOwnerCard.EquelsID((!s.IsAttachedSkill || s.GetAttachSkill == null) ? card : s.GetAttachSkill.SkillPrm.ownerCard)).SelectMany((SkillBase s) => s.PreprocessList.Where((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction));
				for (int num = 0; num < source.Count(); num++)
				{
					SkillPreprocessRemoveAfterAction skillPreprocessRemoveAfterAction = source.ElementAt(num) as SkillPreprocessRemoveAfterAction;
					list.Add(skillPreprocessRemoveAfterAction.Count);
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(0);
		}
		return list;
	}
}
