using System;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 16 of 18 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.PlayerMulliganView
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace Wizard.Battle.Mulligan;

public class PlayerMulliganView : MulliganViewBase
{
	private IPlayerView m_PlayerBattleView;

	public event Func<VfxBase> OnMulliganDragSuccess;

	public PlayerMulliganView(MulliganInfoControl mlgInfoCtrl, IPlayerView view)
		: base(mlgInfoCtrl)
	{
		m_PlayerBattleView = view;
		m_MlgUI.InitMulliganInfo();
		m_MlgUI.OnTimeUp += OnTimeUp;
	}

	public void DragCardStart(BattleCardBase card)
	{
		m_PlayerBattleView.MoveCardStart(card, isEffectAndSoundOn: false);
	}

	public void DragCard(BattleCardBase card, Vector3 Pos)
	{
		m_PlayerBattleView.MoveCard(card, Pos);
	}

	public void DragCardStop(BattleCardBase card)
	{
		m_PlayerBattleView.CardMoveEffectSwitch(on: false);

		card.SetOnMove(move: false);
	}

	public override SequentialVfxPlayer MoveCardToStaticPosition(BattleCardBase card, int posIndex, bool isAbandon)
	{
		VfxBase vfx = NullVfx.GetInstance();
		if (isAbandon)
		{
			vfx = this.OnMulliganDragSuccess.GetAllFuncVfxResults();
		}
		SequentialVfxPlayer sequentialVfxPlayer = base.MoveCardToStaticPosition(card, posIndex, isAbandon);
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			DragCardStop(card);
			if (isAbandon)
			{
				m_MlgUI.SetExchangeMarkPlayer(posIndex, on: true);
			}
		}));
		sequentialVfxPlayer.Register(vfx);
		return sequentialVfxPlayer;
	}

	public void ShowCardDetail(BattleCardBase card)
	{
		bool flag = Mathf.Approximately(card.BattleCardView.Transform.localPosition.x, 0f); // Pre-Phase-5b: IsWatchBattle const-false
		m_PlayerBattleView.SetDetailScreenPosition(!flag && _IsDetailScreenRight());
		m_PlayerBattleView.ShowDetailPanel(null, null, card, DetailPanelControl.ShowRequest.MULLIGAN);
	}

	private bool _IsDetailScreenRight()
	{
		if (InputMgr.ShowDetailLeftAndRight /* Pre-Phase-5b: IsWatchBattle const-false */)
		{
			return Input.mousePosition.x < (float)Screen.width / 2f;
		}
		return false;
	}

	public void ShutDownCardDetail()
	{
		m_PlayerBattleView.HideDetailPanel();
	}

	public RaycastHit[] ConvertMousePositionToRayCastHits(Vector3 position)
	{
		return m_MlgUI.GetRaycastHitFromPosition(position);
	}

	public RaycastHit[] ConvertMousePositionToFrontUIRaycastHits(Vector3 position)
	{
		return m_MlgUI.Get2DRaycastHitFromPosition(position);
	}

	public Vector3 GetWorldPointInMulliganUICamera(Vector3 position)
	{
		return m_MlgUI.ScreenToWorldPoint3D(position);
	}

	private VfxBase OnTimeUp()
	{
		return InstantVfx.Create(ShutDownCardDetail);
	}

	protected override GameObject GetMulliganUIKeepZone()
	{
		return m_MlgUI.GetKeepZonePlayer().gameObject;
	}

	protected override GameObject GetMulliganUIAbandonZone()
	{
		return m_MlgUI.GetAbandonZonePlayer().gameObject;
	}

	public override void HideMulliganUIAbandonZone()
	{
		m_MlgUI.HideMulliganChangeUI();
	}
}
