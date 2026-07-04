using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIEvoGiveBasicSkill : AIEvoTagArgument
{
	private readonly AIScriptTokenArgType _skillType;

	public AIEvoGiveBasicSkill(string text, AIScriptTokenArgType skillType)
		: base(text)
	{
		_skillType = skillType;
		RegisterDefaultTargetFiltersBySkillType();
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
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
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AISkillSimulationUtility.ExecuteTargetSelectGiveSkill(targetsFromField, tagOwner, field, playPtn, situation, _skillType, base.SelectType, selectCount);
				break;
			}
			default:
				AIConsoleUtility.LogError("AIEvoGiveBasicSkill SelectType Error!! SelectType cannot be " + base.SelectType);
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
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

	private void RegisterDefaultTargetFiltersBySkillType()
	{
		AIScriptTokenArgType skillType = _skillType;
		if ((uint)(skillType - 33) <= 1u || skillType == AIScriptTokenArgType.GUARD)
		{
			AIScriptArgumentToken followerToken = new AIScriptArgumentToken(AIScriptTokenArgType.FOLLOWER, isNot: false);
			if (base.Filters == null || !base.Filters.Any((AIScriptTokenBase f) => f.IsEqual(followerToken)))
			{
				base.Filters = AIParamQuery.AddElementToList(followerToken, base.Filters);
			}
		}
	}

	protected override List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
