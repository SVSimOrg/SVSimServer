using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.Phase;

public static class EmitHandUtility
{

	public static void SendSelectObject(BattleManagerBase battleMgr, BattleCardBase selectedCard)
	{
		if (true /* Pre-Phase-5b: no room battle type headless; always fall through */)
		{
			return;
		}
		NetworkStandardBattleMgr networkStandardBattleMgr = ConvertToStandardNetworkBattleMgr(battleMgr);
		if (networkStandardBattleMgr != null && battleMgr.GetCurrentPhase() is MainPhase)
		{
			if (selectedCard != null && selectedCard.IsInHand)
			{
				networkStandardBattleMgr.NetworkSender.SendSelectObject(selectedCard);
			}
			else
			{
				networkStandardBattleMgr.NetworkSender.SendSelectObject(null);
			}
		}
	}

	public static void SendTurnEndReady(BattleManagerBase battleMgr, bool isShortenedTurn)
	{
		ConvertToStandardNetworkBattleMgr(battleMgr)?.NetworkSender.SendTurnEndReady(isShortenedTurn);
	}

	public static void SendSlideObject(BattleManagerBase battleMgr, NetworkBattleSender.SLIDE_OBJECT_TYPE slideObjectType, BattleCardBase selectedCard = null, BattleCardBase attackingCard = null)
	{
		ConvertToStandardNetworkBattleMgr(battleMgr)?.NetworkSender.SendSlideObject(slideObjectType, selectedCard, attackingCard);
	}

	private static NetworkStandardBattleMgr ConvertToStandardNetworkBattleMgr(BattleManagerBase battleMgr)
	{
		if (battleMgr?.InstanceNetworkAgent == null)
		{
			return null;
		}
		return battleMgr as NetworkStandardBattleMgr;
	}
}
