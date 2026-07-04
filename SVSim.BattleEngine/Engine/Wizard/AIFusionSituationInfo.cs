using System.Collections.Generic;

namespace Wizard;

public class AIFusionSituationInfo : AISituationInfo
{
	public List<AIVirtualCard> Targets;

	public float Priority { get; private set; }

	public List<AIScriptTokenBase> Range { get; private set; }

	public AIPolishConvertedExpression PriorityExpression { get; private set; }

	public AIFusionSituationInfo(AIVirtualCard actor, List<AIVirtualCard> targets)
		: base(actor, null, null, AIOperationType.FUSION)
	{
		Targets = targets;
	}

	public void UpdatePriority(List<int> playPtn)
	{
		Priority = PriorityExpression.EvalArg(base.Actor, playPtn, base.Actor.SelfField, this);
	}

	public bool InitializeFusionParameter(AIVirtualField field, List<int> playPtn)
	{
		return base.Actor.TagCollectionContainer.FusionTags.InitializeFusionSituationParameter(base.Actor, field, playPtn, this);
	}

	public void SetParameter(List<AIScriptTokenBase> range, AIPolishConvertedExpression priority)
	{
		Range = range;
		PriorityExpression = priority;
	}
}
