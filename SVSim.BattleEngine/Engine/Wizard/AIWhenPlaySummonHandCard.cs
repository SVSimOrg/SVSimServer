using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlaySummonHandCard : AIWhenPlayTagArgument
{
	public AIWhenPlaySummonHandCard(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.TARGET_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (base.SelectType == AIScriptTokenArgType.TARGET_SELECT)
		{
			ExecuteTargetSelect(tagOwner, field, playPtn, situation);
		}
	}

	private void ExecuteTargetSelect(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(base.SelectType))
		{
			AISummonTokenUtility.ExecuteTargetSelectSummonToken(tagOwner, field, base.SelectType, situation);
		}
	}
}
