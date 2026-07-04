using System.Collections.Generic;

namespace Wizard;

public class AIGetOn : AIFiltersArgument
{
	public AIGetOn(string text)
		: base(text)
	{
	}

	public bool CanGetOn(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.CheckMatchTargetFiltering(targetCard, null, base.Filters, playPtn, tagOwner, situation);
	}
}
