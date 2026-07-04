using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayModifyConsumeEp : AIWhenPlayTagArgument
{
	protected override bool _isSelectCountImplemented => true;

	public AIWhenPlayModifyConsumeEp(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.ModifierNotConsumeEpALL(targetsFromField);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AISkillSimulationUtility.ModifierNotConsumeEpTargets(targetsFromField, base.SelectType, situation);
				break;
			default:
				AIConsoleUtility.LogError("AIWhenPlayModifyConsumeEp.Execute() : Ileagal SelectType [" + base.SelectType.ToString() + "] Error!");
				break;
			}
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

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
