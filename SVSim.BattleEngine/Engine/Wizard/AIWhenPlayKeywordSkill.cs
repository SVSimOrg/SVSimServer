using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayKeywordSkill : AIWhenPlayTagArgument
{
	private readonly AIScriptTokenArgType _skillType;

	protected override bool _isSelectCountImplemented => true;

	public AIWhenPlayKeywordSkill(string text, AIScriptTokenArgType skill)
		: base(text)
	{
		_skillType = skill;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.GiveSkillToAll(targetsFromField, field, _skillType);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AISkillSimulationUtility.GiveSkillRandom(targetsFromField, selectCount, field, _skillType);
				break;
			}
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AISkillSimulationUtility.ExecuteTargetSelectGiveSkill(targetsFromField, tagOwner, field, playPtn, situation, _skillType, base.SelectType, GetSelectCount(tagOwner, field, playPtn, situation));
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		if (AISkillSimulationUtility.IsFollowerOnlySkillType(_skillType))
		{
			return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
		}
		return base.GetFilteredTargets(candidates, tagOwner, playPtn, situation, isBlockDead);
	}
}
