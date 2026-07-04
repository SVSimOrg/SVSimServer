using System.Collections.Generic;

namespace Wizard;

public class AIAttackOrClashRemoveSkill : AIWhenAttackOrWhenFightTagArgument
{
	public AIAttackOrClashRemoveSkill(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIScriptTokenArgType selectType = base.SelectType;
			if (selectType != AIScriptTokenArgType.RANDOM_SELECT && selectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIRemoveSkillSimulationUtility.RemoveSkillAll(targetsFromField, situation);
			}
		}
	}
}
