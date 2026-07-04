using System;
using System.Collections.Generic;
using Cute;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 6 of 9 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.PlayerMulliganCtrl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace Wizard.Battle.Mulligan;

public class PlayerMulliganCtrl : MulliganCtrl
{
	protected PlayerMulliganView _playerMulliganView;

	protected IList<BattleCardBase> m_AbandonList;

	public Action OnMulliganLaunchComplete;

	protected bool _isSetOnCard;

	public IList<BattleCardBase> AbandonList => m_AbandonList;

	public PlayerMulliganCtrl(BattlePlayerBase player, MulliganInfoControl mulliganInfo, IPlayerView view)
		: base(player)
	{
		_playerMulliganView = new PlayerMulliganView(mulliganInfo, view);
		_mulliganView = _playerMulliganView;
		m_AbandonList = new List<BattleCardBase>();
	}

	public override VfxBase StartMulliganVfx(SkillProcessor skillProcessor)
	{
		BattlePlayerBase battlePlayer = GetBattlePlayer();
		List<int> indexList = _LotMulliganCardIndex(battlePlayer.DeckCardList.Count);
		_CreateMulliganCardList(indexList);
		DrawFirstMulliganCard();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(_playerMulliganView.SortFirstDrawsToKeepZone(_firstDrawList));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			for (int i = 0; i < _firstDrawList.Count; i++)
			{
				_firstDrawList[i].SetOnDraw(draw: false);
			}
			_isSetOnCard = true;
			OnMulliganLaunchComplete.Call();
		}));
		return sequentialVfxPlayer;
	}

	public override VfxBase SubmitMulliganVfx(IList<BattleCardBase> abandonCards)
	{
		_mulliganChangedNum = abandonCards.Count;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (abandonCards.Count > 0)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(_MulliganCardChange(abandonCards));
			parallelVfxPlayer.Register(sequentialVfxPlayer);
		}
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		return parallelVfxPlayer;
	}

	protected override VfxBase _MulliganSwap(IDictionary<int, BattleCardBase> newList, IList<BattleCardBase> oldList)
	{
		return _CardSwapAndMoveToStaticPositionVfx(newList, oldList, isCardHolderPlayer: true, isHideMulliganTitle: true);
	}

	public PlayerMulliganView GetPlayerMulliganView()
	{
		return _playerMulliganView;
	}

	public void RegisterAbandonCard(BattleCardBase card)
	{
		if (_firstDrawList.Contains(card) && !m_AbandonList.Contains(card))
		{

			m_AbandonList.Add(card);
		}
	}

	public void TakeOutAbandonCard(BattleCardBase card)
	{
		if (m_AbandonList.Contains(card))
		{

			m_AbandonList.Remove(card);
		}
	}
}
