using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class NotBeAttackedInfo
{
	private string _info;

	private BattlePlayerPair _playerPair;

	private SkillOptionValue _optionValue;

	private SkillConditionCheckerOption _checkerOption;

	private ISkillBattlePlayerFilter _playerFilter;

	private ISkillTargetFilter _targetFilter;

	private List<ISkillCardFilter> _cardFilter;

	private static readonly string[] _comparisonOperatorNames = new string[5] { ">=", "<=", ">", "<", "=" };

	private static readonly string OFFENSE = "offense";

	public NotBeAttackedInfo(string info, BattleCardBase owner, SkillBase skill)
	{
		_info = info;
		_info = _info.Replace("(", "");
		_info = _info.Replace(")", "");
		_playerFilter = SkillFilterCreator.CreateBattlePlayerFilter(SkillFilterCreator.ContentKeyword.both);
		_targetFilter = new SkillTargetInPlayFilter();
		_cardFilter = new List<ISkillCardFilter>();
		_cardFilter.Add(new SkillUnitFilter());
		if (_info.Contains(OFFENSE))
		{
			_info = _info.Replace(OFFENSE, "");
			string comparisonOperator = GetComparisonOperator(ref _info);
			_cardFilter.Add(new SkillParameterOffenseFilter(_info, comparisonOperator));
		}
		_playerPair = new BattlePlayerPair(owner.SelfBattlePlayer, owner.OpponentBattlePlayer);
		_optionValue = skill.OptionValue;
		_checkerOption = new SkillConditionCheckerOption();
	}

	private string GetComparisonOperator(ref string info)
	{
		for (int i = 0; i < _comparisonOperatorNames.Length; i++)
		{
			if (info.Contains(_comparisonOperatorNames[i]))
			{
				info = info.Replace(_comparisonOperatorNames[i], "");
				return _comparisonOperatorNames[i];
			}
		}
		return string.Empty;
	}

	public bool CheckAttacked(BattleCardBase attackCard)
	{
		IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = _playerFilter.Filtering(_playerPair);
		IEnumerable<IReadOnlyBattleCardInfo> enumerable = _targetFilter.Filtering(battlePlayerInfos, _checkerOption);
		foreach (ISkillCardFilter item in _cardFilter)
		{
			enumerable = item.Filtering(enumerable, _optionValue);
		}
		return !enumerable.Any((IReadOnlyBattleCardInfo c) => c.IsPlayer == attackCard.IsPlayer && c.Index == attackCard.Index);
	}
}
