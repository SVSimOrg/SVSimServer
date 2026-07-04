using System.Collections.Generic;

namespace Wizard;

public class AIAttackOrClashSpellboost : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _spellboostValue;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAttackOrClashSpellboost(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_spellboostValue = _exprList[_exprList.Count - 1];
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForSpellboost(candidates, tagOwner, base.Filters, playPtn, situation);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int count = (int)_spellboostValue.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISpellboostSimulationUtility.SpellboostAll(targetsFromField, count);
			}
		}
	}
}
