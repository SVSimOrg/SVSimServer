using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessFixedGenericValue : SkillPreprocessBase
{
	private readonly int _value;

	private readonly string _operator;

	private readonly IReadOnlyBattleCardInfo _card;

	public SkillPreprocessFixedGenericValue(IReadOnlyBattleCardInfo card, string operatorStr, int value)
	{
		_card = card;
		_operator = operatorStr;
		_value = value;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (_card.SkillApplyInformation.SkillGenericValueArray == null)
		{
			return false;
		}
		int num = _card.SkillApplyInformation.SkillGenericValueArray[0];
		if (_operator == ">=")
		{
			return num >= _value;
		}
		if (_operator == ">")
		{
			return num > _value;
		}
		if (_operator == "<")
		{
			return num < _value;
		}
		if (_operator == "<=")
		{
			return num <= _value;
		}
		if (_operator == "!=")
		{
			return num != _value;
		}
		return num == _value;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
