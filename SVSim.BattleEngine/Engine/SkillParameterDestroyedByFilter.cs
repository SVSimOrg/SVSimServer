using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterDestroyedByFilter : ISkillCardFilter
{
	private string _player;

	public SkillParameterDestroyedByFilter(SkillFilterCreator.ContentKeyword keyword)
	{
		switch (keyword)
		{
		case SkillFilterCreator.ContentKeyword.self_ability:
			_player = "me";
			break;
		case SkillFilterCreator.ContentKeyword.both_ability:
			_player = "both";
			break;
		default:
			_player = string.Empty;
			break;
		}
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.DestroyedBySkillList.Any((BattleCardBase.DestroyedBySkillInfo s) => s.Player == _player || _player == "both"))
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
