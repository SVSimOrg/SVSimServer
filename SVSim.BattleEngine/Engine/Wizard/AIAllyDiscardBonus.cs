using System.Collections.Generic;

namespace Wizard;

public class AIAllyDiscardBonus : AIBonusArgumentWithIgnoreInBattle
{
	private List<AIScriptTokenBase> _filters;

	public AIAllyDiscardBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_filters = GetFilters(_exprList.GetRange(0, _exprList.Count - _valueIndexOffset));
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation = null)
	{
		AIDiscardInfo discardInfo = situation.DiscardInfo;
		if (discardInfo != null && discardInfo.IsValuable)
		{
			field.SimulationExtraBonus += GetBonusValue(tagOwner, playPtn, situation, useIgnoreInBattle) * (float)discardInfo.TargetList.Count;
		}
	}

	public override float GetBonusValue(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool useIgnoreInBattle)
	{
		if (!AIFilteringUtility.CheckMatchTargetFiltering(situation.Actor, null, _filters, playPtn, tagOwner, situation))
		{
			return 0f;
		}
		return base.GetBonusValue(tagOwner, playPtn, situation, useIgnoreInBattle);
	}
}
