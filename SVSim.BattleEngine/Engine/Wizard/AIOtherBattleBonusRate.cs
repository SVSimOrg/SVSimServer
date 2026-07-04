using System.Collections.Generic;

namespace Wizard;

public class AIOtherBattleBonusRate : AIBonusArgumentWithIgnoreInBattle
{
	private int ValueArgumentIndexOffset = 1;

	private int MinimumArgumentCount = 2;

	public List<AIScriptTokenBase> Filters { get; private set; }

	public AIOtherBattleBonusRate(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count < MinimumArgumentCount)
		{
			return;
		}
		Filters = GetFilters(_exprList.GetRange(0, _exprList.Count - ValueArgumentIndexOffset));
		int num = -1;
		for (int i = 0; i < Filters.Count; i++)
		{
			if (Filters[i].Type == AIScriptTokenType.ARG && ((AIScriptArgumentToken)Filters[i]).ArgumentType == AIScriptTokenArgType.NONE)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			Filters = Filters.GetRange(0, num + 1);
		}
	}

	public float GetBonusValue(AIVirtualCard tagOwner, AIVirtualCard target, List<AIVirtualCard> allCandidates, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (useIgnoreInBattle && base.IsIgnoreInBattle)
		{
			return 1f;
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(target, allCandidates, Filters, playPtn, tagOwner, null))
		{
			return _bonusValueArg.EvalArg(tagOwner, playPtn, field);
		}
		return 1f;
	}
}
