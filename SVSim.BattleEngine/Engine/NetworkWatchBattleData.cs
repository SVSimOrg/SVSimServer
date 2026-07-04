using System.Collections.Generic;
using System.Linq;

public class NetworkWatchBattleData : NetworkBattleData
{
	public class FromStateData
	{
		public int Index;

		public bool IsOpponent;

		public NetworkBattleDefine.NetworkCardPlaceState FromState;

		public FromStateData(int index, bool isOpponent, NetworkBattleDefine.NetworkCardPlaceState fromState)
		{
			Index = index;
			IsOpponent = isOpponent;
			FromState = fromState;
		}
	}

	public NetworkWatchBattleData(NetworkBattleManagerBase battleMgr)
		: base(battleMgr)
	{
	}

	public override void BeforeSettingReceiveData()
	{
		if (_battleMgr.GameMgr.IsReplayBattle)
		{
			return;
		}
		List<CardDataModel> watchCardList = receiveData.watchCardList;
		new Dictionary<int, NetworkBattleDefine.NetworkCardPlaceState>();
		List<FromStateData> list = new List<FromStateData>();
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		foreach (CardDataModel card in watchCardList)
		{
			ReplaceReceivedCard replaceReceivedCard = CreateReplaceReceivedCard(card);
			NetworkBattleDefine.NetworkCardPlaceState fromState = card.fromState;
			FromStateData fromStateData = list.FirstOrDefault((FromStateData f) => f.Index == card.Index && f.IsOpponent == card.isOpponent);
			if (fromStateData != null)
			{
				fromState = fromStateData.FromState;
			}
			else
			{
				FromStateData item = new FromStateData(card.Index, card.isOpponent, card.fromState);
				list.Add(item);
			}
			if (card.CardId == 0)
			{
				if (card.isOpponent)
				{
					replaceReceivedCard.SetPrivateCardSpellboost(_battleMgr.BattleEnemy);
				}
				continue;
			}
			if (_battleMgr.GameMgr.IsAdmin || !card.isOpponent)
			{
				if (receiveData.IsFusion && card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Hand)
				{
					continue;
				}
				NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = card.ToStateList.FirstOrDefault();
				if (receiveData.dataUri == NetworkBattleDefine.NetworkBattleURI.Deal || (receiveData.IsChoice && receiveData.choiceIdList.Contains(card.CardId)) || networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Deck || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Hand && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Field) || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Field && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Hand) || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Field && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Cemetery) || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Hand && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Cemetery) || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Field && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Banish) || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Hand && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Banish) || (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Field && networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Unite) || fromState == NetworkBattleDefine.NetworkCardPlaceState.Hand)
				{
					continue;
				}
			}
			else
			{
				if (card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Field && card.ToStateList.FirstOrDefault() == NetworkBattleDefine.NetworkCardPlaceState.Cemetery)
				{
					continue;
				}
				if (!card.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Field) && !card.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Cemetery) && !card.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Banish) && !card.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Unite))
				{
					bool num = card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Deck && card.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Hand) && card.IsOpen;
					bool isFusion = receiveData.IsFusion;
					bool flag = card.fromState == NetworkBattleDefine.NetworkCardPlaceState.Hand && card.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Hand) && card.IsOpen;
					if (!num && !isFusion && !flag)
					{
						continue;
					}
				}
			}
			if (!card.isOpponent)
			{
				if (!_battleMgr.GameMgr.IsReplayBattle && !_battleMgr.IsRecovery)
				{
					list2.Add(replaceReceivedCard.ReplaceCard(_battleMgr.BattlePlayer));
				}
			}
			else
			{
				list2.Add(replaceReceivedCard.ReplaceCard(_battleMgr.BattleEnemy));
			}
		}
		if (!_battleMgr.IsRecovery && list2.Count > 0)
		{
			_battleMgr.VfxMgr.RegisterSequentialVfx(_battleMgr.LoadCardResources(list2));
		}
	}
}
