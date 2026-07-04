using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlaySpellboost : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _boostCount;

	private readonly int BOOST_COUNT_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlaySpellboost(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_boostCount = _exprList[_exprList.Count - BOOST_COUNT_ARG_OFFSET];
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

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			ExecuteSpellboost(tagOwner, field, targetsFromField, playPtn, situation);
		}
	}

	private void ExecuteSpellboost(AIVirtualCard tagOwner, AIVirtualField field, List<AIVirtualCard> targets, List<int> playPtn, AISituationInfo situation)
	{
		if (targets != null && targets.Count > 0)
		{
			int boostCount = GetBoostCount(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISpellboostSimulationUtility.SpellboostAll(targets, boostCount);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				ExecuteTargetSelectSpellboost(tagOwner, boostCount, targets, situation, field, playPtn);
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	private void ExecuteTargetSelectSpellboost(AIVirtualCard owner, int boostCount, List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualField field, List<int> playPtn)
	{
		if (situation != null)
		{
			if (situation.IsTargetExists(base.SelectType))
			{
				AISpellboostSimulationUtility.SpellboostTarget(situation, boostCount, targets, base.SelectType);
			}
			else
			{
				SpellboostTargetPrediction(owner, boostCount, targets, situation, field, playPtn);
			}
		}
	}

	private void SpellboostTargetPrediction(AIVirtualCard owner, int boostCount, List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualField field, List<int> playPtn)
	{
		List<AIVirtualCard> candidates = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(targets, owner, playPtn);
		SelectBestTargetToSpellboost(candidates, field, situation, boostCount, base.SelectType);
	}

	private void SelectBestTargetToSpellboost(List<AIVirtualCard> candidates, AIVirtualField field, AISituationInfo situation, int boostCount, AIScriptTokenArgType whichTarget)
	{
		AIVirtualCard target = AISpellboostSimulationUtility.SelectBestTargetForSpellboost(field, boostCount, candidates);
		situation.SetSingleTargetInInfo(target, TargetSelectType.Default, whichTarget);
		AISpellboostSimulationUtility.SpellboostTarget(situation, boostCount, candidates, whichTarget);
	}

	public override void ExecuteForPlayPtnEvaluation(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(owner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			ExecuteSpellboost(owner, field, targetsFromField, playPtn, situation);
		}
	}

	public override void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		AIVirtualCard card = playInfo.Card;
		List<int> playPtn = record.PlayPtn;
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(card, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			ExecuteSpellboost(card, field, targetsFromField, playPtn, situation);
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForSpellboost(candidates, tagOwner, base.Filters, playPtn, situation);
	}

	public override TargetSelectType GetTargetSelectType()
	{
		return TargetSelectType.NormalRuleBase;
	}

	public override void RegisterRuleBaseTargets(List<AIVirtualCard> candidates, AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualCard> targetList)
	{
		base.RegisterRuleBaseTargets(candidates, actor, field, situation, ref targetList);
		int boostCount = GetBoostCount(actor, null, field, situation);
		AIVirtualCard aIVirtualCard = AISpellboostSimulationUtility.SelectBestTargetForSpellboost(field, boostCount, candidates);
		if (aIVirtualCard != null)
		{
			targetList = AIParamQuery.AddElementToList(aIVirtualCard, targetList);
		}
	}

	public int GetBoostCount(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		return (int)_boostCount.EvalArg(tagOwner, playPtn, field, situation);
	}
}
