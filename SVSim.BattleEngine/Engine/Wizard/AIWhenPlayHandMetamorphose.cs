using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayHandMetamorphose : AIWhenPlayTagArgument
{
	private readonly int METAMORPHOSE_ID_ARG_OFFSET = 1;

	public AIPolishConvertedExpression MetamorphoseId { get; private set; }

	protected override int SELECT_TYPE_OFFSET => METAMORPHOSE_ID_ARG_OFFSET + 1;

	public AIWhenPlayHandMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		MetamorphoseId = _exprList[_exprList.Count - METAMORPHOSE_ID_ARG_OFFSET];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int metamorphoseId = MetamorphoseId.EvalID();
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIMetamorphoseSimulationUtility.MetamorphoseHandAll(field, targetsFromField, metamorphoseId, tagOwner, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIMetamorphoseSimulationUtility.MetamorphoseHandRandom(field, targetsFromField, metamorphoseId, tagOwner, playPtn, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			if (!situation.IsTargetExists(base.SelectType))
			{
				AIConsoleUtility.LogError("AIWhenPlayHandMetamorphose.Execute() Error!! Targets of " + base.SelectType.ToString() + " is not exist!!!!!");
			}
			else
			{
				AIMetamorphoseSimulationUtility.MetamorphoseHandTarget(field, targetsFromField, metamorphoseId, situation, base.SelectType);
			}
			break;
		default:
			AIConsoleUtility.LogError("AIWhenPlayHandMetamorphose.Execute() Error!! SelectType=" + base.SelectType);
			break;
		}
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation, isBlockDead);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> filteredTargets = base.GetFilteredTargets(candidates, tagOwner, playPtn, situation, isBlockDead);
		filteredTargets?.RemoveAll((AIVirtualCard c) => !c.IsInHand || c.IsSameCard(tagOwner));
		return filteredTargets;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Metamorphose;
	}
}
