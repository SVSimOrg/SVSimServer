using System.Collections.Generic;

namespace Wizard;

public class AIEvoHandMetamorphose : AIEvoTagArgument
{
	private readonly int METAMORPHOSE_ID_ARG_OFFSET = 1;

	public AIPolishConvertedExpression MetamorphoseId { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIEvoHandMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		MetamorphoseId = _exprList[_exprList.Count - METAMORPHOSE_ID_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
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
				AIMetamorphoseSimulationUtility.MetamorphoseHandTarget(field, targetsFromField, metamorphoseId, situation, base.SelectType);
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
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
