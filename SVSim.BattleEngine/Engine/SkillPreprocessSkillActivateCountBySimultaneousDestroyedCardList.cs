using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessSkillActivateCountBySimultaneousDestroyedCardList : SkillPreprocessBase
{
	private BattleCardBase _ownerCard;

	private bool _isPlayer;

	private string _cardType;

	public SkillPreprocessSkillActivateCountBySimultaneousDestroyedCardList(BattleCardBase ownerCard, string option)
	{
		_ownerCard = ownerCard;
		string[] array = option.Split(':');
		_isPlayer = array[0] == "me";
		_cardType = array[1];
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (!_ownerCard.AlreadyInactiveSkillActivateCountBySimultaneousDestroyedCardList)
		{
			if (option.DestroyedCard == null)
			{
				_ownerCard.ActiveSkillActivateCountBySimultaneousDestroyedCardList = false;
				return false;
			}
			_ownerCard.ActiveSkillActivateCountBySimultaneousDestroyedCardList = option.DestroyedCard.IsPlayer == _ownerCard.IsPlayer == _isPlayer;
			string cardType = _cardType;
			if (cardType != null && cardType == "unit")
			{
				_ownerCard.ActiveSkillActivateCountBySimultaneousDestroyedCardList &= option.DestroyedCard.IsUnit;
			}
		}
		return _ownerCard.ActiveSkillActivateCountBySimultaneousDestroyedCardList;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		_ownerCard.InactiveSkillActivateCountBySimultaneousDestroyedCardList();
		return NullVfx.GetInstance();
	}
}
