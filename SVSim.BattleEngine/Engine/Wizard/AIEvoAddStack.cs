using System.Collections.Generic;

namespace Wizard;

public class AIEvoAddStack : AIEvoTagArgument
{
	private AIPolishConvertedExpression _addStackCount;

	protected override int SELECT_TYPE_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => -1;

	public AIEvoAddStack(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		if (_exprList == null || _exprList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIEvoAddStack error!! _exprList is null or Count==0");
		}
		else
		{
			_addStackCount = _exprList[0];
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIWhiteRitualSimulationUtility.AddWhiteRitualSingle(GetAddStackCount(tagOwner, field, playPtn, situation), targetsFromField);
		}
	}

	private int GetAddStackCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_addStackCount == null)
		{
			return 0;
		}
		return (int)_addStackCount.EvalArg(tagOwner, playPtn, field, situation);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForWhiteRitualOnly(candidates, tagOwner, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override void CreateLegalSelectTypes()
	{
	}
}
