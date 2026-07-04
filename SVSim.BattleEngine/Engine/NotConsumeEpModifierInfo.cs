using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class NotConsumeEpModifierInfo
{
	private string _info;

	private BattleCardBase _owner;

	private BattlePlayerPair _playerPair;

	private SkillOptionValue _optionValue;

	private SkillConditionCheckerOption _checkerOption;

	private ISkillBattlePlayerFilter _playerFilter;

	private ISkillTargetFilter _targetFilter;

	private List<ISkillCardFilter> _cardFilter;

	public BattleCardBase TargetCard { get; private set; }

	public NotConsumeEpModifierInfo(string info, BattleCardBase owner, SkillBase skill, BattleCardBase target, ValueWithOperator cardId)
	{
		TargetCard = target;
		_info = info;
		_owner = owner;
		_playerFilter = SkillFilterCreator.CreateBattlePlayerFilter(SkillFilterCreator.ContentKeyword.me);
		_targetFilter = new SkillTargetInPlayFilter();
		_cardFilter = new List<ISkillCardFilter>();
		_cardFilter.Add(new SkillUnitFilter());
		_cardFilter.Add(new SkillTribeFilter(SkillFilterCreator.ParseEnum(_info, CardBasePrm.TribeType.MAX), "="));
		if (cardId != null)
		{
			_cardFilter.Add(new SkillParameterBaseCardIdFilter(cardId.Value, cardId.Operator));
		}
		_playerPair = new BattlePlayerPair(owner.SelfBattlePlayer, owner.OpponentBattlePlayer);
		_optionValue = skill.OptionValue;
		_checkerOption = new SkillConditionCheckerOption();
	}

	public bool CheckNotConsumedCard(BattleCardBase evoCard)
	{
		if (TargetCard != null && (TargetCard == evoCard || (TargetCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle && TargetCard.Index == evoCard.Index && TargetCard.IsPlayer == evoCard.IsPlayer)))
		{
			return true;
		}
		if (evoCard.SkillApplyInformation.IsIndependent && evoCard != _owner)
		{
			return false;
		}
		IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = _playerFilter.Filtering(_playerPair);
		IEnumerable<IReadOnlyBattleCardInfo> enumerable = _targetFilter.Filtering(battlePlayerInfos, _checkerOption);
		foreach (ISkillCardFilter item in _cardFilter)
		{
			enumerable = item.Filtering(enumerable, _optionValue);
		}
		return enumerable.Any((IReadOnlyBattleCardInfo c) => c == evoCard);
	}
}
