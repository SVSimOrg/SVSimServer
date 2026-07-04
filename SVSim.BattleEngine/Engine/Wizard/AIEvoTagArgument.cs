using System.Collections.Generic;

namespace Wizard;

public abstract class AIEvoTagArgument : AITargetSelectTagArgument
{
	public virtual bool IsImmediate => false;

	protected virtual bool IsSelfTargetForbidden => true;

	public virtual TargetSelectType SimulationTargetSelectType => TargetSelectType.Default;

	public AIEvoTagArgument(string text)
		: base(text)
	{
	}

	public abstract bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation);

	protected bool IsCertainlyIncludeTarget(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		AIVirtualField selfField = owner.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(owner, selfField, bestPlayPtn, situation);
		if (targetsFromField != null && targetsFromField.Contains(candidate))
		{
			return CheckIsCandidateSelectedBySelectType(targetsFromField, situation, candidate);
		}
		return false;
	}

	protected virtual bool CheckIsCandidateSelectedBySelectType(List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualCard candidate)
	{
		return base.SelectType switch
		{
			AIScriptTokenArgType.ALL_SELECT => true, 
			AIScriptTokenArgType.RANDOM_SELECT => targets.Count <= 1, 
			AIScriptTokenArgType.TARGET_SELECT => situation.IsFirstTarget(candidate), 
			AIScriptTokenArgType.SECOND_TARGET_SELECT => situation.IsSecondTarget(candidate), 
			_ => false, 
		};
	}

	public List<AIVirtualCard> GetTargetSelectCandidates(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation)
	{
		if (base.SelectType != AIScriptTokenArgType.TARGET_SELECT && base.SelectType != AIScriptTokenArgType.SECOND_TARGET_SELECT)
		{
			return null;
		}
		return GetTargetsFromField(owner, field, field.BestPlayPtn, situation);
	}

	public virtual void RegisterRuleBaseTargets(List<AIVirtualCard> candidates, AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualCard> targetList)
	{
		if (SimulationTargetSelectType != TargetSelectType.NormalRuleBase)
		{
			AIConsoleUtility.LogError("AIEvoTagArgument.RegisterRuleBaseTargets() error!! " + GetType()?.ToString() + " is not NormalRuleBase!!!!!");
		}
		if (base.SelectType != AIScriptTokenArgType.TARGET_SELECT && base.SelectType != AIScriptTokenArgType.SECOND_SELECTED_TARGET)
		{
			AIConsoleUtility.LogError("AIEvoTagArgument.RegisterRuleBaseTargets() error!! SelectType = " + base.SelectType.ToString() + "!!!!!");
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool flag = base.SelectType == AIScriptTokenArgType.TARGET_SELECT || base.SelectType == AIScriptTokenArgType.SECOND_TARGET_SELECT;
		if (base.Filters == null || base.Filters.Count <= 0)
		{
			AIConsoleUtility.LogError("AIEvoTagArgument:GetTargets() Missing Target Filter Error! CardName[" + ((tagOwner != null) ? tagOwner.CardName : "") + "]");
			return null;
		}
		List<AIVirtualCard> list = GetBaseFilteringCards(candidates, tagOwner, tagOwner.SelfField, playPtn, situation, isBlockDead);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		if (flag && IsSelfTargetForbidden)
		{
			list.Remove(tagOwner);
		}
		if (flag && list != null && list.Count > 0)
		{
			list = AITargetSelectFilteringUtility.GetLegalCandidates(tagOwner, list, 1);
		}
		return list;
	}

	protected virtual List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		return AIFilteringUtility.MultipleFiltering(candidates, base.Filters, tagOwner, playPtn, situation, isBlockDead);
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

	public virtual AIRemovalType GetRemovalType()
	{
		return AIRemovalType.None;
	}
}
