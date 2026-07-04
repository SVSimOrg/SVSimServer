using System.Collections.Generic;

namespace Wizard;

public class AIEmoteSet
{
	private List<AIEmoteCmd> cmdList = new List<AIEmoteCmd>();

	public IEnumerable<AIEmoteCmd> EmoteCmds => cmdList;
}
