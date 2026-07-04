using System.Collections.Generic;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 1 of 3 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.NetworkPlayerMulliganCtrl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Mulligan;

public class NetworkPlayerMulliganCtrl : PlayerMulliganCtrl
{
	public NetworkPlayerMulliganCtrl(BattlePlayerBase player, MulliganInfoControl mulliganInfo, IPlayerView view)
		: base(player, mulliganInfo, view)
	{
		_playerMulliganView = new PlayerMulliganView(mulliganInfo, view);
		_mulliganView = _playerMulliganView;
		m_AbandonList = new List<BattleCardBase>();
	}

	public override VfxBase StartMulliganVfx(SkillProcessor skillProcessor)
	{
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
		}));
		_battlePlayer.CallRecordingMulliganStart(_firstDrawList);
		return sequentialVfxPlayer;
	}

	protected override IDictionary<int, BattleCardBase> _MoveNewCardToHand(IList<BattleCardBase> AbandonCards)
	{
		return NetworkMoveNewCardToHand(AbandonCards);
	}
}
