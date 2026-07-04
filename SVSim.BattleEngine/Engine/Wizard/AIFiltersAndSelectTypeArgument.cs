using System.Collections.Generic;

namespace Wizard;

public class AIFiltersAndSelectTypeArgument : AIFiltersArgument
{
	public AIScriptTokenArgType SelectType { get; protected set; }

	public AIScriptTokenArgType ReferenceSelectedTargetType { get; protected set; }

	protected virtual int SELECT_TYPE_OFFSET => 1;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIFiltersAndSelectTypeArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count > NON_FILTER_FIRST_OFFSET)
		{
			InitSelectType();
		}
	}

	protected virtual void InitSelectType()
	{
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
	}

	protected void SetUpReferenceSelectedTargetInfoSelectType()
	{
		if (base.Filters == null && SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			ReferenceSelectedTargetType = AIScriptTokenArgType.NONE;
			return;
		}
		for (int i = 0; i < base.Filters.Count; i++)
		{
			if (base.Filters[i] is AIScriptArgumentToken { ArgumentType: var argumentType })
			{
				switch (argumentType)
				{
				case AIScriptTokenArgType.SELECTED_TARGET:
					ReferenceSelectedTargetType = AIScriptTokenArgType.TARGET_SELECT;
					return;
				case AIScriptTokenArgType.SECOND_SELECTED_TARGET:
					ReferenceSelectedTargetType = AIScriptTokenArgType.SECOND_TARGET_SELECT;
					return;
				}
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> filteredTargets = base.GetFilteredTargets(candidates, tagOwner, playPtn, situation, isBlockDead);
		if (filteredTargets != null && (SelectType == AIScriptTokenArgType.TARGET_SELECT || SelectType == AIScriptTokenArgType.SECOND_TARGET_SELECT))
		{
			AIVirtualCard compareCard = ((tagOwner.BeforeTransformedCardForSimulation != null) ? tagOwner.BeforeTransformedCardForSimulation : tagOwner);
			filteredTargets.RemoveAll((AIVirtualCard c) => c.IsSameCard(compareCard));
		}
		return filteredTargets;
	}
}
