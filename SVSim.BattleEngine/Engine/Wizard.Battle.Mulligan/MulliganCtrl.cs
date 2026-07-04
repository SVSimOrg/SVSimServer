using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 7 of 19 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.MulliganCtrl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace Wizard.Battle.Mulligan;

public abstract class MulliganCtrl
{

	protected BattlePlayerBase _battlePlayer;

	protected MulliganViewBase _mulliganView;

	protected List<BattleCardBase> _firstDrawList = new List<BattleCardBase>(3);

	protected List<BattleCardBase> _stockList = new List<BattleCardBase>(3);

	protected List<int> _mulliganAfterCardIndexList;

	protected int _mulliganChangedNum = -1;

	public List<int> DealIdxList = new List<int>();

	public MulliganCtrl(BattlePlayerBase player)
	{
		_battlePlayer = player;
	}

	public abstract VfxBase StartMulliganVfx(SkillProcessor skillProcessor);

	public abstract VfxBase SubmitMulliganVfx(IList<BattleCardBase> abandonCards);

	protected VfxBase _MulliganCardChange(IList<BattleCardBase> AbandonCards)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (AbandonCards.Count > 0)
		{
			IDictionary<int, BattleCardBase> newList = _MoveNewCardToHand(AbandonCards);
			_ReturnAbandonToDeck(AbandonCards);
			sequentialVfxPlayer.Register(_MulliganSwap(newList, AbandonCards));
		}
		return sequentialVfxPlayer;
	}

	protected void _ReturnAbandonToDeck(IList<BattleCardBase> AbandonCards)
	{
		List<BattleCardBase> list = AbandonCards.Where((BattleCardBase c) => c != null).ToList();
		for (int num = 0; num < list.Count(); num++)
		{
			GetBattlePlayer().AddToDeck(list[num]);
		}
	}

	protected virtual IDictionary<int, BattleCardBase> _MoveNewCardToHand(IList<BattleCardBase> AbandonCards)
	{
		List<BattleCardBase> list = _stockList.Take(AbandonCards.Count).ToList();
		_stockList.RemoveRange(0, AbandonCards.Count);
		BattlePlayerBase player = GetBattlePlayer();
		for (int i = 0; i < AbandonCards.Count; i++)
		{
			player.DeckCardList.Remove(list[i]);
			int index = player.HandCardList.IndexOf(AbandonCards[i]);
			player.HandCardList[index] = list[i];
		}
		return list.ToDictionary((BattleCardBase card) => player.HandCardList.IndexOf(card));
	}

	protected IDictionary<int, BattleCardBase> NetworkMoveNewCardToHand(IList<BattleCardBase> AbandonCards)
	{
		// Phase-5 chunk 48: restored card lookup — chunk 35's stub broke live receive Deal path.
		var battleMgr = _battlePlayer.BattleMgr;
		BattlePlayerBase player = GetBattlePlayer();
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < _mulliganAfterCardIndexList.Count; i++)
		{
			int num = _mulliganAfterCardIndexList[i];
			if (!DealIdxList.Contains(num))
			{
				list.Add(battleMgr.GetBattleCardIdx(player.DeckCardList, num));
			}
		}
		if (AbandonCards.Count != list.Count)
		{
			string text = "";
			for (int j = 0; j < AbandonCards.Count; j++)
			{
				if (j > 0)
				{
					text += ",";
				}
				text += AbandonCards[j].Index;
			}
			string text2 = "";
			for (int k = 0; k < list.Count; k++)
			{
				if (k > 0)
				{
					text2 += ",";
				}
				text2 += list[k].Index;
			}
			throw new Exception($"Card swap failed：AbandonCards【{text}】/DrawCards【{text2}】");
		}
		SortedList<int, BattleCardBase> sortedList = new SortedList<int, BattleCardBase>();
		for (int l = 0; l < AbandonCards.Count; l++)
		{
			int key = DealIdxList.IndexOf(AbandonCards[l].Index);
			sortedList.Add(key, AbandonCards[l]);
		}
		IList<int> keys = sortedList.Keys;
		for (int m = 0; m < keys.Count; m++)
		{
			int key2 = keys[m];
			player.DeckCardList.Remove(list[m]);
			int index = player.HandCardList.IndexOf(sortedList[key2]);
			player.HandCardList[index] = list[m];
		}
		return list.ToDictionary((BattleCardBase card) => player.HandCardList.IndexOf(card));
	}

	protected abstract VfxBase _MulliganSwap(IDictionary<int, BattleCardBase> newList, IList<BattleCardBase> oldList);

	protected VfxBase _CardSwapAndMoveToStaticPositionVfx(IDictionary<int, BattleCardBase> newList, IList<BattleCardBase> oldList, bool isCardHolderPlayer, bool isHideMulliganTitle)
	{
		List<int> list = newList.Keys.ToList();
		List<BattleCardBase> list2 = newList.Values.ToList();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			_mulliganView.HideMulliganUIAbandonZone();
			if (isHideMulliganTitle)
			{
				_mulliganView.HideMulliganTitle();
			}
		}));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		int count = newList.Count;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		for (int num = 0; num < count; num++)
		{
			BattleCardBase card = list2[num];
			int posIndex = list[num];
			parallelVfxPlayer.Register(_mulliganView.MoveCardToStaticPosition(card, posIndex, isAbandon: false));
		}
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{

		}));
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		return sequentialVfxPlayer;
	}

	public void GetAbandonCardList(BattleManagerBase battleMgr, ref List<BattleCardBase> retCardList, ref List<int> retPosList)
	{
		IEnumerable<int> source = DealIdxList.Where((int index) => !_mulliganAfterCardIndexList.Contains(index));
		retPosList = source.Select((int index) => DealIdxList.IndexOf(index)).ToList();
		retCardList = source.Select((int index) => battleMgr.GetBattleCardIdx(GetBattlePlayer().AllCards.ToList(), index)).ToList();
	}

	public VfxBase DrawFirstMulliganCard()
	{
		SkillProcessor skillProcessor = new SkillProcessor();
		return GetBattlePlayer().DrawCards(_firstDrawList, skillProcessor, isOpen: false, isMulligan: true).Vfx;
	}

	protected List<int> _LotMulliganCardIndex(int maxNum)
	{
		List<int> list = new List<int>(6);
		List<int> list2 = Enumerable.Range(1, maxNum).ToList();
		if (_battlePlayer.BattleMgr.InstanceIsRandomDraw /* Pre-Phase-5b: IsNetworkBattle assumed true */)
		{
			for (int i = 0; i < 6; i++)
			{
				int index = 0; // Pre-Phase-5b: no mgr in scope; deterministic fallback
				list.Add(list2[index]);
				list2.Remove(list2[index]);
			}
		}
		else
		{
			for (int j = 0; j < 6; j++)
			{
				list.Add(list2[j]);
			}
		}
		return list;
	}

	protected void _CreateMulliganCardList(List<int> indexList)
	{
		// Phase-5 chunk 48 (2026-07-03): restored the real card lookup — chunk 35's null stub
		// broke the live receive-driven Deal path (BattlePlayerBase.DrawCard NRE'd on null
		// entries). mgr is reachable via _battlePlayer.BattleMgr (chunk 42 seam).
		var battleMgr = _battlePlayer.BattleMgr;
		List<BattleCardBase> deckCardList = GetBattlePlayer().DeckCardList;
		for (int i = 0; i < 3; i++)
		{
			BattleCardBase battleCardIdx = battleMgr.GetBattleCardIdx(deckCardList, indexList[i]);
			BattleCardBase battleCardIdx2 = battleMgr.GetBattleCardIdx(deckCardList, indexList[i + 3]);
			_firstDrawList.Add(battleCardIdx);
			_stockList.Add(battleCardIdx2);
		}
	}

	public void CreateMulliganDealList(List<int> indexList)
	{
		// Phase-5 chunk 48 (2026-07-03): restored the real card lookup — see _CreateMulliganCardList.
		var battleMgr = _battlePlayer.BattleMgr;
		List<BattleCardBase> deckCardList = GetBattlePlayer().DeckCardList;
		for (int i = 0; i < 3; i++)
		{
			BattleCardBase battleCardIdx = battleMgr.GetBattleCardIdx(deckCardList, indexList[i]);
			_firstDrawList.Add(battleCardIdx);
		}
	}

	public List<BattleCardBase> GetFirstDrawList()
	{
		return _firstDrawList;
	}

	public List<BattleCardBase> GetStockList()
	{
		return _stockList;
	}

	public List<int> GetMulliganAfterCardIndexList()
	{
		return _mulliganAfterCardIndexList;
	}

	public void SetMulliganAfterCardIndexList(List<int> indexList)
	{
		_mulliganAfterCardIndexList = indexList;
	}

	public int GetChangedNum()
	{
		return _mulliganChangedNum;
	}

	public BattlePlayerBase GetBattlePlayer()
	{
		return _battlePlayer;
	}

	public MulliganInfoControl GetMulliganInfo()
	{
		return _mulliganView.MulliganInfo;
	}
}
