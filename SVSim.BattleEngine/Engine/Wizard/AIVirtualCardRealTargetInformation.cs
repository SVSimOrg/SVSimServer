using System.Collections.Generic;

namespace Wizard;

public class AIVirtualCardRealTargetInformation
{
	public AIVirtualCard Owner { get; private set; }

	public List<AIVirtualCard> TargetList { get; private set; }

	public AIVirtualCardRealTargetInformation(AIVirtualCard owner, List<AIVirtualCard> targets)
	{
		Owner = owner;
		TargetList = targets;
	}
}
