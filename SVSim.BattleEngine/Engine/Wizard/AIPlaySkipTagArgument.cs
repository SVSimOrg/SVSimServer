using System.Collections.Generic;

namespace Wizard;

public class AIPlaySkipTagArgument : AIScriptArgumentExpressions
{
	public AIPlaySkipTagArgument(string text)
		: base(text)
	{
	}

	public virtual PlaySkipInformation CreatePlaySkipInformation(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return new PlaySkipInformation();
	}
}
