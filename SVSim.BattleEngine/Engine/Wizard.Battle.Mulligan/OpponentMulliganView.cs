using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Mulligan;

public class OpponentMulliganView : MulliganViewBase
{
	private bool _isUseExchangeMark;

	public OpponentMulliganView(MulliganInfoControl mulliganInfo, bool isUseExchangeMark)
		: base(mulliganInfo)
	{
		_isUseExchangeMark = isUseExchangeMark;
	}

	public override SequentialVfxPlayer MoveCardToStaticPosition(BattleCardBase card, int posIndex, bool isAbandon)
	{
		SequentialVfxPlayer sequentialVfxPlayer = base.MoveCardToStaticPosition(card, posIndex, isAbandon);
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			card.SetOnMove(move: false);
			if (_isUseExchangeMark && isAbandon)
			{
				m_MlgUI.SetExchangeMarkOpponent(posIndex, on: true);
			}
		}));
		return sequentialVfxPlayer;
	}

	protected override GameObject GetMulliganUIKeepZone()
	{
		return m_MlgUI.GetKeepZoneOpponent().gameObject;
	}

	protected override GameObject GetMulliganUIAbandonZone()
	{
		return m_MlgUI.GetAbandonZoneOpponent().gameObject;
	}

	public override void HideMulliganUIAbandonZone()
	{
		m_MlgUI.HideMulliganOpponentChangeUI();
	}
}
