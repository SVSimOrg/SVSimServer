using System.Collections.Generic;

namespace Wizard;

public struct PlayptnBaseStatsRateInfo
{
	public List<AIVirtualCard> Targets;

	public int Rate;

	public PlayptnBaseStatsRateInfo(List<AIVirtualCard> targets, int rate)
	{
		Targets = targets;
		Rate = rate;
	}
}
