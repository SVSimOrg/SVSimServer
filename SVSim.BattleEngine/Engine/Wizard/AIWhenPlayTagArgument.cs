using System;
using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayTagArgument : AITargetSelectTagArgument
{
	public virtual bool IsImmediate => false;

	public AIWhenPlayTagArgument(string text)
		: base(text)
	{
	}

	public virtual void ExecuteForPlayPtnEvaluation(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
	}

	public virtual void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
	}

	public virtual void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		throw new NotImplementedException();
	}

	public virtual void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		throw new NotImplementedException();
	}

	public List<AIVirtualCard> GetTargetSelectCandidates(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation)
	{
		if (base.SelectType != AIScriptTokenArgType.TARGET_SELECT && base.SelectType != AIScriptTokenArgType.SECOND_TARGET_SELECT)
		{
			return null;
		}
		List<AIVirtualCard> list = GetTargetsFromField(owner, field, null, situation);
		if (list != null && list.Count > 0)
		{
			int selectCount = GetSelectCount(owner, owner.SelfField, null, situation);
			list = AITargetSelectFilteringUtility.GetLegalCandidates(owner, list, selectCount);
		}
		return list;
	}

	public virtual TargetSelectType GetTargetSelectType()
	{
		return TargetSelectType.Default;
	}

	public virtual AIRemovalType GetRemovalType()
	{
		return AIRemovalType.None;
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		if (candidateRange == null || candidateRange.Count <= 0)
		{
			return null;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>(candidateRange);
		int num;
		if (base.SelectType != AIScriptTokenArgType.TARGET_SELECT)
		{
			num = ((base.SelectType == AIScriptTokenArgType.SECOND_TARGET_SELECT) ? 1 : 0);
			if (num == 0)
			{
				goto IL_006d;
			}
		}
		else
		{
			num = 1;
		}
		AIVirtualCard compareCard = ((owner.BeforeTransformedCardForSimulation != null) ? owner.BeforeTransformedCardForSimulation : owner);
		list.RemoveAll((AIVirtualCard c) => c.IsSameCard(compareCard));
		goto IL_006d;
		IL_006d:
		List<AIVirtualCard> list2 = GetFilteredTargets(list, owner, playPtn, situation, isBlockDead);
		if (num != 0 && list2 != null && list2.Count > 0)
		{
			int selectCount = GetSelectCount(owner, field, playPtn, situation);
			list2 = AITargetSelectFilteringUtility.GetLegalCandidates(owner, list2, selectCount);
		}
		return list2;
	}

	public virtual void RegisterRuleBaseTargets(List<AIVirtualCard> candidates, AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualCard> targetList)
	{
		if (GetTargetSelectType() != TargetSelectType.NormalRuleBase)
		{
			AIConsoleUtility.LogError("AIWhenPlayTagArgument.GetRuleBaseTargets() error!! " + GetType()?.ToString() + " is not NormalRuleBase!!!!!");
		}
		if (base.SelectType != AIScriptTokenArgType.TARGET_SELECT && base.SelectType != AIScriptTokenArgType.SECOND_SELECTED_TARGET)
		{
			AIConsoleUtility.LogError("AIWhenPlayTagArgument.GetRuleBaseTargets() error!! SelectType = " + base.SelectType.ToString() + "!!!!!");
		}
	}

	public void UpdateSituationRemovalType(AISituationInfo situation)
	{
		if (situation != null && situation.SelectedTargets != null)
		{
			AIRemovalType removalType = GetRemovalType();
			if (base.ReferenceSelectedTargetType != AIScriptTokenArgType.NONE && removalType != AIRemovalType.None)
			{
				situation.SelectedTargets.UpdateRemovalType(base.ReferenceSelectedTargetType, removalType);
			}
		}
	}
}
