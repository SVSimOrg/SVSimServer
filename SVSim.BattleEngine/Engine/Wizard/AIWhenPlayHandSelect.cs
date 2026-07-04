using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayHandSelect : AIWhenPlaySelect
{
	public AIWhenPlayHandSelect(string text)
		: base(text)
	{
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}
}
