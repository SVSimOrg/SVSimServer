using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayImmediateKeywordSkill : AIWhenChangeInplayTagArgument
{
	protected AIScriptTokenArgType _stopTiming;

	private readonly AIScriptTokenArgType _skillType;

	protected virtual int StopTimingOffset => 1;

	protected override int SELECT_TYPE_OFFSET => 2;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIChangeInplayImmediateKeywordSkill(string text, AIScriptTokenArgType skill)
		: base(text, isImmediate: true)
	{
		_skillType = skill;
		base.SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		if (!IsImplementedSelecvtType(base.SelectType))
		{
			base.SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
		List<AIPolishConvertedExpression> range = _exprList.GetRange(0, _exprList.Count - NON_FILTER_FIRST_OFFSET);
		base.Filters = GetFilters(range);
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - StopTimingOffset]);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISkillProcessInformation processInfo, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.GiveSkillToAll(targetsFromField, field, _skillType);
			}
			ChangeInplayStopPreprocess(targetsFromField, field);
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

	protected void ChangeInplayStopPreprocess(List<AIVirtualCard> targets, AIVirtualField field)
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
