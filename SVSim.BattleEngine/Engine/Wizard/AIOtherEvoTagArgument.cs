using System.Collections.Generic;

namespace Wizard;

public abstract class AIOtherEvoTagArgument : AITriggerAndTargetFiltersTagBase
{
	public AIScriptTokenArgType SelectType { get; protected set; }

	protected virtual int SELECT_TYPE_ARG_OFFSET => 1;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_ARG_OFFSET;

	public abstract bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation);

	public AIOtherEvoTagArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		InitializeSelectType();
	}

	protected bool IsCertainlyIncludeTarget(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		AIVirtualField selfField = owner.SelfField;
		if (!CheckTriggerLegal(situation.Actor, owner, selfField.BestPlayPtn, situation))
		{
			return false;
		}
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		List<AIVirtualCard> targets = GetTargets(owner, selfField, bestPlayPtn, situation);
		if (targets != null && targets.Contains(candidate))
		{
			return CheckIsCandidateSelectedBySelectType(targets, owner, selfField, bestPlayPtn, situation, candidate);
		}
		return false;
	}

	protected virtual bool CheckIsCandidateSelectedBySelectType(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard candidate)
	{
		return SelectType switch
		{
			AIScriptTokenArgType.ALL_SELECT => true, 
			AIScriptTokenArgType.RANDOM_SELECT => targets.Count <= 1, 
			_ => false, 
		};
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	public virtual AIRemovalType GetRemovalType()
	{
		return AIRemovalType.None;
	}

	protected virtual void InitializeSelectType()
	{
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_ARG_OFFSET], base.LegalSelectTypes);
	}
}
