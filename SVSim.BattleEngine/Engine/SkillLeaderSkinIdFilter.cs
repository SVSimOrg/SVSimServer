using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillLeaderSkinIdFilter : ISkillCardFilter
{
	private readonly int _id;

	private readonly bool _isEqual;

	public SkillLeaderSkinIdFilter(int id, string op)
	{
		_id = id;
		_isEqual = op == "=";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		DataMgr dataMgr = cards.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr?.GameMgr?.GetDataMgr();
		if (dataMgr == null) return list;
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.IsPlayer)
			{
				if (dataMgr.GetPlayerSkinId() == _id && _isEqual)
				{
					list.Add(readOnlyBattleCardInfo);
				}
			}
			else if (dataMgr.GetEnemySkinId() == _id && _isEqual)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
