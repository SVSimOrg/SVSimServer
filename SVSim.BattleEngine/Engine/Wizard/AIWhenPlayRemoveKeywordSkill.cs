using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayRemoveKeywordSkill : AIWhenPlayTagArgument
{
	private readonly AIScriptTokenArgType _skillType;

	public AIWhenPlayRemoveKeywordSkill(string text, AIScriptTokenArgType skill)
		: base(text)
	{
		_skillType = skill;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISkillSimulationUtility.RemoveKeywordSkillToAll(targetsFromField, _skillType);
		}
	}

	public List<AIVirtualCard> RemoveGuardPredictionInPlayOut(List<AIVirtualCard> targetList, AIVirtualCard owner, List<int> playPtn, AISituationInfo situation)
	{
		if (_skillType != AIScriptTokenArgType.GUARD || base.SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return null;
		}
		List<AIVirtualCard> filteredTargets = GetFilteredTargets(targetList, owner, playPtn, situation);
		if (filteredTargets == null || filteredTargets.Count <= 0)
		{
			return null;
		}
		filteredTargets.RemoveAll((AIVirtualCard c) => !c.IsGuard);
		AISkillSimulationUtility.RemoveKeywordSkillToAll(filteredTargets, _skillType);
		return filteredTargets;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}
