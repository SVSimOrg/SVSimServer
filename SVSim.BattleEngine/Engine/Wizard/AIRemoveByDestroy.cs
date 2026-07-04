using System.Collections.Generic;

namespace Wizard;

public class AIRemoveByDestroy : AIFiltersArgument
{
	public AIRemoveByDestroy(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
	}

	public bool IsRemoveByDestroy(AIVirtualCard targetCard, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.CheckMatchTargetFiltering(targetCard, null, base.Filters, playPtn, tagOwner, situation);
	}
}
