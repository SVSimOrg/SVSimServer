using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Mulligan;

public class NetworkOpponentMulliganCtrl : OpponentMulliganCtrl
{
	public NetworkOpponentMulliganCtrl(BattlePlayerBase player, MulliganInfoControl mulliganInfo, bool isUseExchangeMark)
		: base(player, mulliganInfo, isUseExchangeMark)
	{
		_mulliganView = new OpponentMulliganView(mulliganInfo, isUseExchangeMark);
		_isHideCard = true;
	}

	public override VfxBase StartMulliganVfx(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(_battlePlayer.BattleMgr.LoadCardResources(_firstDrawList));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		_battlePlayer.CallRecordingMulliganStart(_firstDrawList);
		return sequentialVfxPlayer;
	}

	protected override IDictionary<int, BattleCardBase> _MoveNewCardToHand(IList<BattleCardBase> AbandonCards)
	{
		return NetworkMoveNewCardToHand(AbandonCards);
	}
}
