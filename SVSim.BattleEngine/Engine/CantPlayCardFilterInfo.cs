using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class CantPlayCardFilterInfo
{
	private BattlePlayerPair _playerPair;

	private SkillOptionValue _optionValue;

	private SkillConditionCheckerOption _checkerOption;

	private ISkillBattlePlayerFilter _playerFilter;

	private ISkillTargetFilter _targetFilter;

	private List<ISkillCardFilter> _cardFilterList;

	private ValueWithOperator _cardId;

	private bool _hasCardIdFilter;

	public bool IsCantPlaySpell { get; private set; }

	public bool IsCantPlayField { get; private set; }

	public CantPlayCardFilterInfo(BattleCardBase owner, SkillBase skill)
	{
		_playerFilter = SkillFilterCreator.CreateBattlePlayerFilter(SkillFilterCreator.ContentKeyword.me);
		_targetFilter = new SkillTargetHandFilter();
		_cardFilterList = CreateCardFilterList(skill.OptionValue);
		_playerPair = new BattlePlayerPair(owner.SelfBattlePlayer, owner.OpponentBattlePlayer);
		_optionValue = skill.OptionValue;
		_checkerOption = new SkillConditionCheckerOption();
		_hasCardIdFilter = _cardFilterList.Any((ISkillCardFilter c) => c is SkillParameterBaseCardIdFilter);
		IsCantPlaySpell = _cardFilterList.Any((ISkillCardFilter c) => c is SkillSpellFilter);
		IsCantPlayField = _cardFilterList.Any((ISkillCardFilter c) => c is SkillFieldFilter);
	}

	private List<ISkillCardFilter> CreateCardFilterList(SkillOptionValue option)
	{
		List<ISkillCardFilter> list = new List<ISkillCardFilter>();
		switch (option.GetString(SkillFilterCreator.ContentKeyword.card_type))
		{
		case "unit":
			list.Add(new SkillUnitFilter());
			break;
		case "class":
			list.Add(new SkillClassFilter());
			break;
		case "spell":
			list.Add(new SkillSpellFilter());
			break;
		case "field":
			list.Add(new SkillFieldFilter());
			break;
		case "all":
			list.Add(new SkillAllCardFilter());
			break;
		}
		ValueWithOperator valueWithOperator = option.GetValueWithOperator(SkillFilterCreator.ContentKeyword.base_card_id);
		if (valueWithOperator != null)
		{
			_cardId = new ValueWithOperator(option.ParseInt(valueWithOperator.Value).ToString(), valueWithOperator.Operator);
			list.Add(new SkillParameterBaseCardIdFilter(_cardId.Value, _cardId.Operator));
		}
		return list;
	}

	public bool CheckCantPlay(BattleCardBase targetCard)
	{
		IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = _playerFilter.Filtering(_playerPair);
		IEnumerable<IReadOnlyBattleCardInfo> enumerable = _targetFilter.Filtering(battlePlayerInfos, _checkerOption);
		for (int i = 0; i < _cardFilterList.Count; i++)
		{
			enumerable = _cardFilterList[i].Filtering(enumerable, _optionValue);
		}
		return enumerable.Any((IReadOnlyBattleCardInfo c) => c.IsPlayer == targetCard.IsPlayer && c.Index == targetCard.Index);
	}

	public bool CheckCantPlayTransformId(BattleCardBase originalCard)
	{
		if (!_hasCardIdFilter || _cardId == null)
		{
			return false;
		}
		Skill_transform accelerateOrCrystallizeTransformSkill = originalCard.GetAccelerateOrCrystallizeTransformSkill();
		if (accelerateOrCrystallizeTransformSkill == null)
		{
			return false;
		}
		return SkillCompareFuncCreator.Create(_cardId.Operator)(_optionValue.ParseInt(_cardId.Value), accelerateOrCrystallizeTransformSkill.TransformId);
	}
}
