using System.Collections.Generic;
using Wizard;

public class NetworkConsistency
{
	private BattleManagerBase battleMgr;

	private List<string> consistencyList;

	public NetworkConsistency(BattleManagerBase mgr)
	{
		battleMgr = mgr;
	}

	public Dictionary<string, object> GetConsistency()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int num = 1;
		foreach (string consistency in consistencyList)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.key] + num, consistency);
			num++;
		}
		return dictionary;
	}

	public void SetupConsistency()
	{
		consistencyList = new List<string>();
		for (int i = 0; i < 2; i++)
		{
			long num = 0L;
			BattlePlayerBase battlePlayerBase = ((i != 0) ? battleMgr.GetBattlePlayer(isPlayer: false) : battleMgr.GetBattlePlayer(isPlayer: true));
			int num2 = 1;
			num += battlePlayerBase.Class.Life;
			num2++;
			num += battlePlayerBase.CemeteryList.Count * num2;
			num2++;
			num += battlePlayerBase.DeckCardList.Count * num2;
			num2++;
			num += battlePlayerBase.CurrentEpCount * num2;
			num2++;
			num += battlePlayerBase.PpTotal * num2;
			if (Data.CurrentFormat == Format.Avatar)
			{
				num2++;
				num += battlePlayerBase.Bp * num2;
			}
			consistencyList.Add(num.ToString());
			long num3 = 0L;
			int num4 = 0;
			foreach (BattleCardBase handCard in battlePlayerBase.HandCardList)
			{
				num4++;
				num3 += num4 * handCard.Index;
			}
			consistencyList.Add(num3.ToString());
			long num5 = 0L;
			int num6 = 0;
			foreach (BattleCardBase inPlayCard in battlePlayerBase.InPlayCards)
			{
				num6++;
				int num7 = 1;
				num5 += inPlayCard.CardId;
				num5 += num6 * inPlayCard.Index;
				num7++;
				num5 += num6 * inPlayCard.Atk * num7;
				num7++;
				num5 += num6 * inPlayCard.Life * num7;
				ISkillApplyInformation skillApplyInformation = inPlayCard.SkillApplyInformation;
				int num8 = (inPlayCard.IsEvolution ? 1 : 0);
				num7++;
				num5 += num6 * num8 * num7;
				int num9 = (skillApplyInformation.IsGuard ? 1 : 0);
				num7++;
				num5 += num6 * num9 * num7;
				int num10 = (skillApplyInformation.CantBeFocusedSkill ? 1 : 0);
				num7++;
				num5 += num6 * num10 * num7;
				int num11 = (skillApplyInformation.IsIgnoreGuard ? 1 : 0);
				num7++;
				num5 += num6 * num11 * num7;
				int num12 = (inPlayCard.IsCantAttackClass ? 1 : 0);
				num7++;
				num5 += num6 * num12 * num7;
				int num13 = (skillApplyInformation.IsKiller ? 1 : 0);
				num7++;
				num5 += num6 * num13 * num7;
				int num14 = (skillApplyInformation.IsDrain ? 1 : 0);
				num7++;
				num5 += num6 * num14 * num7;
				num7++;
				num5 += num6 * inPlayCard.ChantCount * num7;
			}
			consistencyList.Add(num5.ToString());
		}
	}
}
