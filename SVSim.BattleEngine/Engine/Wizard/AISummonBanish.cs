using System.Collections.Generic;

namespace Wizard;

public class AISummonBanish : AIFiltersAndSelectTypeArgument
{

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AISummonBanish(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIScriptTokenArgType selectType = AIScriptTokenArgType.NONE;
		AIPolishConvertedExpression arg = _exprList[_exprList.Count - 1];
		if (IsLegalSelectType(arg, out selectType))
		{
			base.SelectType = selectType;
		}
		else
		{
			AIConsoleUtility.LogError($"AISummonBanish error!!! SelectType is {selectType}");
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBanishSimulationUtility.BanishAll(targetsFromField, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBanishSimulationUtility.BanishRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			default:
				AIConsoleUtility.LogError($"AISummonBanish error!!! SelectType is {base.SelectType}");
				break;
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
