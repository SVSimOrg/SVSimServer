using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnDamageValueFilter : ISkillParameterSelectFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	private Dictionary<string, int> _searchedTurnDamageDictionary;

	public SkillParameterTurnDamageValueFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
		if (_turnPlayerInfo.IsAllTurn)
		{
			_searchedTurnDamageDictionary = new Dictionary<string, int>();
		}
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		IEnumerable<int> enumerable = null;
		if (_turnPlayerInfo.IsAllTurn)
		{
			IEnumerable<List<DamageCardParameterModifier>> enumerable2 = cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnDamageValueList(c, _turnPlayerInfo));
			_searchedTurnDamageDictionary.Clear();
			foreach (List<DamageCardParameterModifier> item in enumerable2)
			{
				for (int num = 0; num < item.Count; num++)
				{
					if (_searchedTurnDamageDictionary.Keys.Contains(item[num].Turn.ToString() + item[num].IsSelfTurn))
					{
						_searchedTurnDamageDictionary[item[num].Turn.ToString() + item[num].IsSelfTurn] += item[num].Damage;
					}
					else
					{
						_searchedTurnDamageDictionary.Add(item[num].Turn.ToString() + item[num].IsSelfTurn, item[num].Damage);
					}
				}
			}
			return _searchedTurnDamageDictionary.Values;
		}
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnDamageValue(c, _turnPlayerInfo));
	}
}
