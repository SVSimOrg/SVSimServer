using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoSubtractCountdown : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _countdownAmount;

	protected override int SELECT_TYPE_ARG_OFFSET => 2;

	public AIOtherEvoSubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_countdownAmount = _exprList[_exprList.Count - 1];
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.AllyInplayCards;
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForCountdownAmuletOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int countdownAmount = GetCountdownAmount(tagOwner, field, playPtn, situation);
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISubtractCountdownSimulationUtility.SubtractCountdownAll(targets, countdownAmount, situation);
		}
	}

	private int GetCountdownAmount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_countdownAmount == null)
		{
			AIConsoleUtility.LogError("AIOtherEvoSubtractCountdown error!! _countdownAmount is null");
			return 0;
		}
		return (int)_countdownAmount.EvalArg(tagOwner, playPtn, field, situation);
	}
}
