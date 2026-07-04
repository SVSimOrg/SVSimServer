using System.Linq;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class PlayerDrawCardToHandVfx : SequentialVfxPlayer
{
	public class ChangePlayerHandCardParentVfx : VfxBase
	{
		private readonly IBattleCardView m_view;

		public ChangePlayerHandCardParentVfx(IBattleCardView view)
		{
			m_view = view;
		}

		public override void Play()
		{
			base.Play();
			m_view.GameObject.SetActive(value: false);
			/* Pre-Phase-5b: PCardPlace parent-reparent dropped; view is a null-view stub headless */
			MotionUtils.SetLayerAll(m_view.GameObject, 10);
			m_view.GameObject.SetActive(value: true);
			IsEnd = true;
		}
	}

	private readonly BattleCardBase _card;

	public PlayerDrawCardToHandVfx(BattleCardBase drawCard)
	{
		_card = drawCard;
		Register(Prepare());
		Register(WaitVfx.Create(0.25f));
		Register(OnHand());
		Register(InstantVfx.Create(delegate
		{
			_card.SetOnDraw(draw: false);
		}));
	}

	private VfxBase Prepare()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			GameObject pCardPlace = _card.SelfBattlePlayer.BattleMgr.PCardPlace;
			_card.BattleCardView.GameObject.SetActive(value: false);
			_card.BattleCardView.GameObject.transform.parent = pCardPlace.transform;
			MotionUtils.SetLayerAll(_card.BattleCardView.GameObject, 10);
			_card.BattleCardView.GameObject.SetActive(value: true);
		}));
		if (_card.SelfBattlePlayer.HandCardList.Any((BattleCardBase c) => c == _card))
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				if (!_card.SelfBattlePlayer.BattleMgr.IsRecovery)
				{

				}
				_card.SelfBattlePlayer.BattleView.HandView.AddCardToView(_card.BattleCardView, 0.25f);
				_card.SelfBattlePlayer.HandControl.AttachCardView(_card.BattleCardView);
			}));
		}
		else
		{
			sequentialVfxPlayer.Register(_card.DestroyInHand(null));
		}
		return sequentialVfxPlayer;
	}

	private VfxBase OnHand()
	{
		if (_card.SelfBattlePlayer.HandCardList.Any((BattleCardBase c) => c == _card))
		{
			return ParallelVfxPlayer.Create(_card.BattleCardView.ShowHandCardInfo(), _card.CalcHandCost(), InstantVfx.Create(delegate
			{
				if (!_card.SelfBattlePlayer.BattleMgr.IsRecovery)
				{
					_card.SelfBattlePlayer.BattleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_CARD_DRAW_2, _card.BattleCardView.GameObject.transform.position)
						.GetGameObjIns()
						.transform.rotation = _card.BattleCardView.GameObject.transform.rotation;
				}
			}));
		}
		return NullVfx.GetInstance();
	}
}
