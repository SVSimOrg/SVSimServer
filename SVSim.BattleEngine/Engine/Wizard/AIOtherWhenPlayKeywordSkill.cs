using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayKeywordSkill : AIOtherWhenPlayTagArgument
{
	private AIScriptTokenArgType _skillType;

	public AIOtherWhenPlayKeywordSkill(string text, AIScriptTokenArgType skillType)
		: base(text)
	{
		_skillType = skillType;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISkillSimulationUtility.GiveSkillToAll(targets, field, _skillType);
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		if (AISkillSimulationUtility.IsFollowerOnlySkillType(_skillType))
		{
			return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
		}
		return base.GetFilteredTargets(candidates, tagOwner, playPtn, situation, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}
