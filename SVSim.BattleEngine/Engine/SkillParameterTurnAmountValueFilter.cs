using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnAmountValueFilter : ISkillParameterSelectFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	private Dictionary<string, int> _searchedTurnAmountDictionary;

	public SkillParameterTurnAmountValueFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
		if (_turnPlayerInfo.IsAllTurn)
		{
			_searchedTurnAmountDictionary = new Dictionary<string, int>();
		}
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		IEnumerable<int> result = null;
		if (!_turnPlayerInfo.IsAllTurn)
		{
			return result;
		}
		IEnumerable<List<DamageCardParameterModifier>> enumerable = cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnDamageValueList(c, _turnPlayerInfo));
		IEnumerable<List<HealCardParameterModifier>> enumerable2 = cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnHealValueList(c, _turnPlayerInfo));
		_searchedTurnAmountDictionary.Clear();
		foreach (List<DamageCardParameterModifier> item in enumerable)
		{
			for (int num = 0; num < item.Count; num++)
			{
				if (_searchedTurnAmountDictionary.Keys.Contains(item[num].Turn.ToString() + item[num].IsSelfTurn))
				{
					_searchedTurnAmountDictionary[item[num].Turn.ToString() + item[num].IsSelfTurn] += item[num].Damage;
				}
				else
				{
					_searchedTurnAmountDictionary.Add(item[num].Turn.ToString() + item[num].IsSelfTurn, item[num].Damage);
				}
			}
		}
		foreach (List<HealCardParameterModifier> item2 in enumerable2)
		{
			for (int num2 = 0; num2 < item2.Count; num2++)
			{
				if (_searchedTurnAmountDictionary.Keys.Contains(item2[num2].Turn.ToString() + item2[num2].IsSelfTurn))
				{
					_searchedTurnAmountDictionary[item2[num2].Turn.ToString() + item2[num2].IsSelfTurn] -= item2[num2].Heal;
				}
				else
				{
					_searchedTurnAmountDictionary.Add(item2[num2].Turn.ToString() + item2[num2].IsSelfTurn, -item2[num2].Heal);
				}
			}
		}
		return _searchedTurnAmountDictionary.Values;
	}
}
