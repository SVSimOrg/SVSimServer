using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterIsChaosFilter : ISkillCardFilter
{
	private bool _isChaos;

	public SkillParameterIsChaosFilter(string value)
	{
		_isChaos = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		// Pull NetworkUserInfoData lazily via the first card's mgr chain instead of caching
		// it at ctor time (the ctor runs during skill parsing when no mgr is attached).
		NetworkUserInfoData userInfo = cards.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr?.GameMgr?.GetNetworkUserInfoData();
		if (userInfo == null) return new List<IReadOnlyBattleCardInfo>();
		int chaosId = userInfo.GetSelfChaosId();
		if ((_isChaos && chaosId != -1) || (!_isChaos && chaosId == -1))
		{
			return cards;
		}
		return new List<IReadOnlyBattleCardInfo>();
	}
}
