using System.Collections.Generic;

namespace Wizard;

public class AIAttackAttackableCount : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _attackableCountArg;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAttackAttackableCount(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackableCountArg = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int attackableCount = (int)_attackableCountArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIAttackableCountSimulationUtility.ExecuteChangeAttackableCountAll(targetsFromField, attackableCount);
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
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
