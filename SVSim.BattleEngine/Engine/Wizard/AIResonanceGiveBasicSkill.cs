using System.Collections.Generic;

namespace Wizard;

public class AIResonanceGiveBasicSkill : AIFiltersAndSelectTypeArgument
{
	private readonly AIScriptTokenArgType _skillType;

	public AIResonanceGiveBasicSkill(string text, AIScriptTokenArgType skillType)
		: base(text)
	{
		_skillType = skillType;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISkillSimulationUtility.GiveSkillToAll(targetsFromField, field, _skillType);
		}
	}
}
