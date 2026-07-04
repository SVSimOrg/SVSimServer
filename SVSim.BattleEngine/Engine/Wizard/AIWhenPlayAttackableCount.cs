using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayAttackableCount : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _countArg;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlayAttackableCount(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_countArg = _exprList[_exprList.Count - 1];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int attackableCount = (int)_countArg.EvalArg(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIAttackableCountSimulationUtility.ExecuteChangeAttackableCountAll(targetsFromField, attackableCount);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				AIAttackableCountSimulationUtility.ExecuteChangeAttackableCountTargetSelect(attackableCount, base.SelectType, situation);
				break;
			default:
				AIConsoleUtility.LogError(string.Format("AIWhenPlayAttackableCount.Execute() Error!! SelectType={0} cardName={1}", base.SelectType, (tagOwner != null) ? tagOwner.CardName : ""));
				break;
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
