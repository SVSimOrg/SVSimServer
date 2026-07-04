using System.Collections.Generic;
using Wizard.Battle;

public class SkillIsOwnerFilter : ISkillCardFilter
{
	private bool _isOwner;

	public SkillIsOwnerFilter(bool flag)
	{
		_isOwner = flag;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		// IsAdminWatch is const-false in headless (Phase 4) — the guard collapses to
		// `card.IsPlayer == _isOwner`. Preserved the local for readability.
		bool isAdminWatch = false;
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.IsPlayer == _isOwner || isAdminWatch)
			{
				list.Add(card);
			}
		}
		return list;
	}
}
