using System.Collections.Generic;
using System.Linq;
using Wizard;

public class NetworkBattleData
{
	protected NetworkBattleManagerBase _battleMgr;

	public bool isEnemyFirstTurn;

	protected NetworkBattleReceiver.ReceiveData receiveData;

	public bool isReceiveTurnEndAction;

	public bool isOppoMulliganEnd;

	public bool isPlayerMulliganEnd;

	public bool isEchoWait;

	public NetworkBattleDefine.NetworkBattleURI nowReceiveUri;

	public NetworkBattleData(NetworkBattleManagerBase battleMgr)
	{
		_battleMgr = battleMgr;
	}

	public void SetReceiveData(NetworkBattleReceiver.ReceiveData data)
	{
		receiveData = data;
	}

	public NetworkBattleReceiver.ReceiveData GetReceiveData()
	{
		return receiveData;
	}

	public virtual void BeforeSettingReceiveData()
	{
		ReplaceReceivedCards(receiveData.knownCardList);
		List<CardDataModel> cardList = receiveData.unapprovedList.Where((CardDataModel uCard) => uCard.fromState != NetworkBattleDefine.NetworkCardPlaceState.Deck || !uCard.ToStateList.Any((NetworkBattleDefine.NetworkCardPlaceState s) => s == NetworkBattleDefine.NetworkCardPlaceState.Banish) || !receiveData.knownCardList.Any((CardDataModel kCard) => kCard.Index == uCard.Index && kCard.IsOpen)).ToList();
		ReplaceReceivedCards(cardList);
		SetEnemyFirstTurn();
	}

	protected void SetEnemyFirstTurn()
	{
		if (receiveData.dataUri == NetworkBattleDefine.NetworkBattleURI.Ready && _battleMgr.InstanceNetworkAgent.GetIsFirstPlayer() == 1)
		{
			isEnemyFirstTurn = true;
		}
	}

	private void ReplaceReceivedCards(List<CardDataModel> cardList)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (CardDataModel card in cardList)
		{
			ReplaceReceivedCard replaceReceivedCard = CreateReplaceReceivedCard(card);
			if (card.CardId == 0)
			{
				if (card.isOpponent)
				{
					replaceReceivedCard.SetPrivateCardSpellboost(_battleMgr.BattleEnemy);
				}
			}
			else
			{
				list.Add(replaceReceivedCard.ReplaceCard(_battleMgr.BattleEnemy));
			}
		}
		if (!_battleMgr.IsRecovery && list.Count > 0)
		{
			_battleMgr.VfxMgr.RegisterSequentialVfx(_battleMgr.LoadCardResources(list));
		}
	}

	protected virtual ReplaceReceivedCard CreateReplaceReceivedCard(CardDataModel cardData)
	{
		return new ReplaceReceivedCard(_battleMgr, cardData);
	}

	public void AfterSettingReceiveData()
	{
		nowReceiveUri = receiveData.dataUri;
		if (receiveData.dataUri == NetworkBattleDefine.NetworkBattleURI.TurnStart)
		{
			isEnemyFirstTurn = false;
		}
	}

	public CardDataModel GetPlayCard()
	{
		return receiveData.knownCardList.FirstOrDefault((CardDataModel c) => c.Index == receiveData.playCardIndex);
	}

	public int GetPlayCardIndex()
	{
		return receiveData.playCardIndex;
	}
}
