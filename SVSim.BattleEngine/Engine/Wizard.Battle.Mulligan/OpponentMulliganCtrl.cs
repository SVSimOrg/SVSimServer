using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 2 of 4 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.OpponentMulliganCtrl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Mulligan;

public class OpponentMulliganCtrl : MulliganCtrl
{
	protected List<int> opponentIndexList = new List<int>();

	protected bool _isHideCard = true;

	public OpponentMulliganCtrl(BattlePlayerBase player, MulliganInfoControl mulliganInfo, bool isUseExchangeMark)
		: base(player)
	{
		_mulliganView = new OpponentMulliganView(mulliganInfo, isUseExchangeMark);
		_isHideCard = true;
	}

	public override VfxBase StartMulliganVfx(SkillProcessor skillProcessor)
	{
		BattlePlayerBase battlePlayer = GetBattlePlayer();
		opponentIndexList = _LotMulliganCardIndex(battlePlayer.DeckCardList.Count);
		_CreateMulliganCardList(opponentIndexList);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(battlePlayer.BattleMgr.LoadCardResources(_firstDrawList));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		return sequentialVfxPlayer;
	}

	public override VfxBase SubmitMulliganVfx(IList<BattleCardBase> abandonCards)
	{
		_mulliganChangedNum = abandonCards.Count;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (abandonCards.Count > 0)
		{
			parallelVfxPlayer.Register(_MulliganCardChange(abandonCards));
		}
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		parallelVfxPlayer.Register(_mulliganView.UpdateOpponentMulliganStatusLabel(abandonCards.Count));
		return parallelVfxPlayer;
	}

	protected override VfxBase _MulliganSwap(IDictionary<int, BattleCardBase> newList, IList<BattleCardBase> oldList)
	{
		return NullVfx.GetInstance();
	}
}
