using System.Collections.Generic;

namespace Wizard;

public class AILastwordRemoveSkill : AIFiltersAndSelectTypeArgument
{
	public AILastwordRemoveSkill(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIRemoveSkillSimulationUtility.RemoveSkillAll(targetsFromField, situation);
			}
			else
			{
				AIConsoleUtility.LogError("AILastwordRemoveSkill : 未対応のSelectTypeが使用されています");
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}
