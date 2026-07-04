using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoDestroy : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _selectCount;

	private int _expectedSelectCountArgOffset;

	private static int SELECT_COUNT_OFFSET => 1;

	protected override int SELECT_TYPE_ARG_OFFSET => _expectedSelectCountArgOffset + 1;

	public AIOtherEvoDestroy(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeSelectCount();
		InitializeFilters();
		base.SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_ARG_OFFSET], base.LegalSelectTypes);
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent || candidate.IsIndestructible)
		{
			return false;
		}
		return IsCertainlyIncludeTarget(owner, candidate, situation);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AISkillSimulationUtility.DestroyAll(targets, field, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
		{
			int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
			AISkillSimulationUtility.DestroyRandom(targets, tagOwner, field, playPtn, situation, selectCount);
			break;
		}
		case AIScriptTokenArgType.TARGET_SELECT:
			break;
		}
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Destroy;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT
		};
	}

	protected override bool CheckIsCandidateSelectedBySelectType(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard candidate)
	{
		if (base.SelectType == AIScriptTokenArgType.RANDOM_MULTI_SELECT)
		{
			return targets.Count <= GetSelectCount(tagOwner, field, playPtn, situation);
		}
		return base.CheckIsCandidateSelectedBySelectType(targets, tagOwner, field, playPtn, situation, candidate);
	}

	protected virtual void InitializeSelectCount()
	{
		_selectCount = null;
		_expectedSelectCountArgOffset = SELECT_COUNT_OFFSET;
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[_exprList.Count - _expectedSelectCountArgOffset];
		if (aIPolishConvertedExpression.IsMathematicExpress())
		{
			_selectCount = aIPolishConvertedExpression;
		}
		if (_selectCount == null)
		{
			_expectedSelectCountArgOffset = SELECT_COUNT_OFFSET - 1;
		}
	}

	private int GetSelectCount(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_selectCount == null)
		{
			return 1;
		}
		return (int)_selectCount.EvalArg(owner, playPtn, field, situation);
	}
}
