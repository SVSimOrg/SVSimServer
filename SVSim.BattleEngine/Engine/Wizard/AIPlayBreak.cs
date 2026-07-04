using System.Collections.Generic;

namespace Wizard;

public class AIPlayBreak : AIFiltersArgument
{
	public AIPlayBreak(string text)
		: base(text)
	{
	}

	public bool IsPlayBreak(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (AIFilteringUtility.CheckMatchTargetFiltering(owner, field.AllyHandCards, base.Filters, playPtn, owner, situation))
		{
			return true;
		}
		return false;
	}
}
