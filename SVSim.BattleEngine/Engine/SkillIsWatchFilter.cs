using System.Collections.Generic;
using Wizard.Battle;

public class SkillIsWatchFilter : ISkillCardFilter
{
	private bool _isWatchOk;

	public SkillIsWatchFilter(bool flag)
	{
		// IsWatchBattle and IsReplayBattle are both const-false in headless (Phase 4).
		// `(false && !false) == flag` collapses to `false == flag`, i.e. `!flag`.
		_isWatchOk = !flag;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		if (!_isWatchOk)
		{
			yield break;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			yield return card;
		}
	}
}
