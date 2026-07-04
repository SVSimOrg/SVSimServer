using System.Collections.Generic;

namespace Wizard;

public class AILastwordDestroy : AIFiltersAndSelectTypeArgument
{
	public AILastwordDestroy(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, null);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.DestroyAll(targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
