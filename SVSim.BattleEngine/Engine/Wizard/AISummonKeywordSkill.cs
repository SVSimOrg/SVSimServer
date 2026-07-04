using System.Collections.Generic;

namespace Wizard;

public class AISummonKeywordSkill : AIFiltersArgument
{
	private readonly AIScriptTokenArgType _skillType;

	public AIScriptTokenArgType SelectType { get; protected set; }

	protected virtual int SELECT_TYPE_OFFSET => 1;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AISummonKeywordSkill(string text, AIScriptTokenArgType skill)
		: base(text)
	{
		_skillType = skill;
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		if (!IsImplementedSelecvtType(SelectType))
		{
			SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
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
			switch (SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.GiveSkillToAll(targetsFromField, field, _skillType);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				break;
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	private bool IsImplementedSelecvtType(AIScriptTokenArgType type)
	{
		return type == AIScriptTokenArgType.ALL_SELECT;
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
