using System.Collections.Generic;

namespace Wizard;

public class AIEvoHandSelect : AIEvoTagArgument
{
	public override bool IsImmediate => true;

	public AIEvoHandSelect(string text)
		: base(text)
	{
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}
}
