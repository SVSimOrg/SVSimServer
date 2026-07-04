using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillUniqueTribeCountFilter : SkillCardCountFilter
{
	public override int Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards)
	{
		List<CardBasePrm.TribeType> list = new List<CardBasePrm.TribeType>();
		for (int i = 0; i < cards.Count(); i++)
		{
			List<CardBasePrm.TribeType> tribe = cards.ElementAt(i).Tribe;
			for (int j = 0; j < tribe.Count(); j++)
			{
				CardBasePrm.TribeType tribeType = tribe[j];
				if (tribeType != CardBasePrm.TribeType.ALL && !list.Contains(tribeType))
				{
					list.Add(tribeType);
				}
			}
		}
		return list.Count;
	}
}
