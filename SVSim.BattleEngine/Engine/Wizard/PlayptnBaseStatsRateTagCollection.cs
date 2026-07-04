using System.Collections.Generic;

namespace Wizard;

public class PlayptnBaseStatsRateTagCollection : TagCollection
{
	public PlayptnBaseStatsRateTagCollection()
		: base(TagCollectionType.PlayptnBaseStatsRate)
	{
	}

	private PlayptnBaseStatsRateTagCollection(PlayptnBaseStatsRateTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayptnBaseStatsRateTagCollection(this);
	}

	public List<PlayptnBaseStatsRateInfo> GetExecuteInfo(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, List<AIVirtualCard> candidates)
	{
		if (!base.HasTag)
		{
			return null;
		}
		List<PlayptnBaseStatsRateInfo> list = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!aIPlayTag.CheckCondition(tagOwner, playPtn, field, null))
			{
				continue;
			}
			AIPlayptnBaseStatsRate aIPlayptnBaseStatsRate = aIPlayTag.ArgumentExpressions as AIPlayptnBaseStatsRate;
			List<AIVirtualCard> filteredTargets = aIPlayptnBaseStatsRate.GetFilteredTargets(candidates, tagOwner, playPtn, null);
			if (filteredTargets != null || filteredTargets.Count > 0)
			{
				int rateValue = aIPlayptnBaseStatsRate.GetRateValue(tagOwner, field, playPtn);
				PlayptnBaseStatsRateInfo item = new PlayptnBaseStatsRateInfo(filteredTargets, rateValue);
				if (list == null)
				{
					list = new List<PlayptnBaseStatsRateInfo>();
				}
				list.Add(item);
			}
		}
		return list;
	}
}
