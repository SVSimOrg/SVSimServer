using System.Collections.Generic;

namespace Wizard;

public class AILastwordSubtractCountdown : AIFiltersAndSelectTypeArgument
{
	private readonly int SUBTRACT_ARG_OFFSET = 1;

	public AIPolishConvertedExpression SubtractValue { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AILastwordSubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SubtractValue = _exprList[_exprList.Count - SUBTRACT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int value = (int)SubtractValue.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISubtractCountdownSimulationUtility.SubtractCountdownAll(targetsFromField, value, situation);
			}
			else
			{
				AIConsoleUtility.LogError($"AILastwordSubtractCountdown.Execute(): Unsupported select type. type:{base.SelectType}");
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForCountdownAmuletOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
