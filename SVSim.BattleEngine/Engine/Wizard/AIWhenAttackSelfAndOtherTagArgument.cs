using System.Collections.Generic;

namespace Wizard;

public class AIWhenAttackSelfAndOtherTagArgument : AITriggerAndTargetFiltersTagBase
{
	public virtual bool IsActivateWhenEvalInstantAttack => false;

	public AIWhenAttackSelfAndOtherTagArgument(string text)
		: base(text)
	{
	}

	public virtual void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
	}
}
