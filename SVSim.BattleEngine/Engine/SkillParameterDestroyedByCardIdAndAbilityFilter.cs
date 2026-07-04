using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterDestroyedByCardIdAndAbilityFilter : ISkillCardFilter
{
	private readonly string _parameterText;

	private readonly BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility _ability;

	private readonly string _player = string.Empty;

	public SkillParameterDestroyedByCardIdAndAbilityFilter(string parameterText)
	{
		string[] array = parameterText.Split(':');
		_parameterText = array[0];
		switch (array[1])
		{
		case "when_play":
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.WhenPlay;
			break;
		case "when_accelerate":
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.Accelerate;
			break;
		case "when_destroy":
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.WhenDestroy;
			break;
		default:
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.None;
			break;
		}
		if (array.Length >= 3)
		{
			_player = array[2].ToString();
		}
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		int id = option.ParseInt(_parameterText);
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo card = cards.ElementAt(i);
			for (int j = 0; j < card.DestroyedBySkillList.Count; j++)
			{
				BattleCardBase.DestroyedBySkillInfo destroyedBySkillInfo = card.DestroyedBySkillList.ElementAt(j);
				if (destroyedBySkillInfo.BaseCardId == id && destroyedBySkillInfo.Ability == _ability && (!(_player != string.Empty) || !(destroyedBySkillInfo.Player != _player)))
				{
					yield return card;
				}
			}
		}
	}
}
