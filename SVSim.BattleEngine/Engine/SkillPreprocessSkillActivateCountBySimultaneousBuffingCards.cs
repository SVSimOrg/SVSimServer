using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessSkillActivateCountBySimultaneousBuffingCards : SkillPreprocessBase
{
	private BattleCardBase _ownerCard;

	private bool _isPlayer;

	private bool _isInplaySelf;

	public SkillPreprocessSkillActivateCountBySimultaneousBuffingCards(BattleCardBase ownerCard, string option)
	{
		_ownerCard = ownerCard;
		string[] array = option.Split(':');
		_isPlayer = array[0] == "me";
		_isInplaySelf = array[1] == "inplay_self";
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (!_ownerCard.AlreadyInactiveSkillActivateCountBySimultaneousBuffingCards)
		{
			if (option.InplayBuffingCards == null)
			{
				_ownerCard.ActiveSkillActivateCountBySimultaneousBuffingCards = false;
				return false;
			}
			IEnumerable<IReadOnlyBattleCardInfo> source = option.InplayBuffingCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == _ownerCard.IsPlayer == _isPlayer);
			if (_isInplaySelf)
			{
				_ownerCard.ActiveSkillActivateCountBySimultaneousBuffingCards = source.Contains(_ownerCard);
			}
		}
		return _ownerCard.ActiveSkillActivateCountBySimultaneousBuffingCards;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		_ownerCard.InactiveSkillActivateCountBySimultaneousBuffingCards();
		return NullVfx.GetInstance();
	}
}
