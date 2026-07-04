using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonKeywordSkill : AITriggerAndTargetFiltersTagBase
{
	private AIScriptTokenArgType _stopTiming;

	private AIScriptTokenArgType _skillType;

	public AIScriptTokenArgType SelectType { get; protected set; }

	protected virtual int StopTimingOffset => 1;

	private int DEFAULT_SELECT_TYPE_OFFSET => 2;

	protected virtual int SELECT_TYPE_OFFSET { get; set; }

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIOtherSummonKeywordSkill(string text, AIScriptTokenArgType skill)
		: base(text)
	{
		_skillType = skill;
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - StopTimingOffset]);
		int num = 0;
		if (!IsImplementedStopTiming(_stopTiming))
		{
			num = 1;
			_stopTiming = AIScriptTokenArgType.NONE;
		}
		SELECT_TYPE_OFFSET = DEFAULT_SELECT_TYPE_OFFSET - num;
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		if (!IsImplementedSelecvtType(SelectType))
		{
			SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
		InitializeFilters();
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			switch (SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.GiveSkillToAll(targets, field, _skillType);
				break;
			}
			SetSkillStopPreprocess(targets, field);
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

	private bool IsImplementedStopTiming(AIScriptTokenArgType timing)
	{
		if (timing != AIScriptTokenArgType.WHEN_LEAVE)
		{
			return timing == AIScriptTokenArgType.NONE;
		}
		return true;
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		if (AISkillSimulationUtility.IsFollowerOnlySkillType(_skillType))
		{
			return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
		}
		return base.GetFilteredTargets(candidates, tagOwner, playPtn, situation, isBlockDead);
	}

	public float GetQuickBonus(AIVirtualCard playCard, AIVirtualField field, List<int> playPtn, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		if (_skillType != AIScriptTokenArgType.QUICK)
		{
			return 0f;
		}
		if (!CheckTriggerLegal(playCard, tagOwner, playPtn, situation))
		{
			return 0f;
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(playCard, field.AllyHandCards, base.TargetFilters, playPtn, tagOwner, situation))
		{
			return AIInstantAttackUtility.EvalInstantAttack(playCard.Attack, playCard.Life, playCard.MaxAttackableCount, playPtn, playCard, situation);
		}
		return 0f;
	}

	protected void SetSkillStopPreprocess(List<AIVirtualCard> targets, AIVirtualField field)
	{
		if (targets != null && targets.Count > 0 && _stopTiming == AIScriptTokenArgType.WHEN_LEAVE)
		{
			WhenLeaveStop(targets, field);
		}
	}

	private void WhenLeaveStop(List<AIVirtualCard> targets, AIVirtualField field)
	{
		if (_skillType != AIScriptTokenArgType.UNTOUCHABLE)
		{
			return;
		}
		foreach (AIVirtualCard target in targets)
		{
			WhenLeaveStopUntouchable(target, field);
		}
	}

	private void WhenLeaveStopUntouchable(AIVirtualCard card, AIVirtualField field)
	{
		AIUntouchableStopPreprocessOption option = new AIUntouchableStopPreprocessOption(card);
		field.TagPreprocessContainer.AppendLeaveStopInfo(option, card);
	}
}
