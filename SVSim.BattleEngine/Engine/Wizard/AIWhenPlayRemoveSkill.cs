using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayRemoveSkill : AIWhenPlayTagArgument
{
	public AIWhenPlayRemoveSkill(string text)
		: base(text)
	{
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIRemoveSkillSimulationUtility.RemoveSkillAll(targetsFromField, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIRemoveSkillSimulationUtility.RemoveSkillRandom(targetsFromField, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			if (situation != null && situation.IsTargetExists(base.SelectType))
			{
				AIRemoveSkillSimulationUtility.RemoveSkillTargetSelect(base.SelectType, situation);
			}
			else
			{
				AIConsoleUtility.LogError("playRemoveSkillはplaySkip中のTARGET_SELECT未対応です");
			}
			break;
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			break;
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}
}
