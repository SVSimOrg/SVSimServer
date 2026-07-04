using System.Collections.Generic;

namespace Wizard;

public class AIPlaySkipWithActionIfEvo : AIPlaySkipWithFilteredTargets
{
	private AIScriptTokenArgType _action;

	private readonly int ACTION_TYPE_ARG = 1;

	public AIPlaySkipWithActionIfEvo(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_action = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - ACTION_TYPE_ARG]);
		base.Filters = GetFilters(_exprList.GetRange(0, _exprList.Count - ACTION_TYPE_ARG));
	}

	public override PlaySkipInformation CreatePlaySkipInformation(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		PlaySkipWithActionIfEvoInformation playSkipWithActionIfEvoInformation = new PlaySkipWithActionIfEvoInformation(_action);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null)
		{
			playSkipWithActionIfEvoInformation.AddEvolutionPermittedCards(targetsFromField);
		}
		return playSkipWithActionIfEvoInformation;
	}
}
