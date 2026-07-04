using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillCardCountFilter
{
	public virtual int Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards)
	{
		return cards.Count();
	}
}
