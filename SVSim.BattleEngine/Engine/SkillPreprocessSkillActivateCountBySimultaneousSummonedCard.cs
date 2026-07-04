using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessSkillActivateCountBySimultaneousSummonedCard : SkillPreprocessBase
{
	private BattleCardBase _ownerCard;

	private bool _isPlayer;

	private string _cardType;

	private string _tribeName;

	public SkillPreprocessSkillActivateCountBySimultaneousSummonedCard(BattleCardBase ownerCard, string option)
	{
		_ownerCard = ownerCard;
		string[] array = option.Split(':');
		_isPlayer = array[0] == "me";
		_cardType = array[1];
		_tribeName = array[2];
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (!_ownerCard.AlreadyInactiveSkillActivateCountBySimultaneousSummonedCard)
		{
			if (option.SummonedCard == null)
			{
				_ownerCard.ActiveSkillActivateCountBySimultaneousSummonedCard = false;
				return false;
			}
			_ownerCard.ActiveSkillActivateCountBySimultaneousSummonedCard = option.SummonedCard.IsPlayer == _ownerCard.IsPlayer == _isPlayer;
			string cardType = _cardType;
			if (cardType != null && cardType == "unit")
			{
				_ownerCard.ActiveSkillActivateCountBySimultaneousSummonedCard &= option.SummonedCard.IsUnit;
			}
			_ownerCard.ActiveSkillActivateCountBySimultaneousSummonedCard &= option.SummonedCard.IsTribe(SkillFilterCreator.ParseEnum(_tribeName, CardBasePrm.TribeType.MAX));
		}
		return _ownerCard.ActiveSkillActivateCountBySimultaneousSummonedCard;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		_ownerCard.InactiveSkillActivateCountBySimultaneousSummonedCard();
		return NullVfx.GetInstance();
	}
}
