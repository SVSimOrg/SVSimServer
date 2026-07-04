using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class HandViewBase
{
	protected readonly HandControl _handControl;

	protected readonly List<IBattleCardView> _battleCardViewList;

	public HandViewBase()
	{
	}

	public HandViewBase(GameObject handGameObject, BattleCamera battleCamera)
	{
		_handControl = CreateHandControl(handGameObject, battleCamera);
		_battleCardViewList = new List<IBattleCardView>();
	}

	protected abstract void RearrangeHand(float rearrangeTime, bool isNewReplayMoveTurn = false);

	protected abstract HandControl CreateHandControl(GameObject handGameObject, BattleCamera battleCamera);

	public virtual void AddCardToView(IBattleCardView cardViewToAdd, float deckRearrangeTime, bool isNewReplayMoveTurn = false)
	{
		if (!(cardViewToAdd is NullBattleCardView))
		{
			// Pre-Phase-5b: IsRecovery guard zeroed deckRearrangeTime; see RemoveCardFromView.
			AddCardToViewWithoutRearrange(cardViewToAdd);
			RearrangeHand(deckRearrangeTime, isNewReplayMoveTurn);
		}
	}

	public virtual void AddCardToViewWithoutRearrange(IBattleCardView cardViewToAdd)
	{
		if (!_battleCardViewList.Contains(cardViewToAdd))
		{
			_battleCardViewList.Add(cardViewToAdd);
		}
	}

	public virtual void RemoveCardFromView(IBattleCardView cardViewToRemove, float deckRearrangeTime)
	{
		// Headless has no view manager; the deckRearrangeTime tweak is a Unity3D iTween
		// timing knob that never fires here. Preserving the pre-cull default (no zero-out)
		// is a safe no-op because the enclosing methods' rearrange calls hit no-op views.
		// Pre-Phase-5b: `if (BattleManagerBase.GetIns().IsRecovery) deckRearrangeTime = 0f;`
		if (_battleCardViewList.Contains(cardViewToRemove))
		{
			RemoveCardFromViewWithoutRearrange(cardViewToRemove);
			RearrangeHand(deckRearrangeTime);
		}
	}

	public virtual void RemoveCardFromViewWithoutRearrange(IBattleCardView cardViewToRemove)
	{
		if (_battleCardViewList.Remove(cardViewToRemove))
		{
			cardViewToRemove.isHiddenFromHandView = false;
		}
	}

	public virtual VfxBase ShuffleHand()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase HandUnfocus()
	{
		_handControl.SetHandState(HandControl.HandState.Unfocus);
		return ParallelVfxPlayer.Create(InstantVfx.Create(delegate
		{
			RearrangeHand(0.2f);
		}), WaitVfx.Create(0.2f));
	}

	public virtual VfxBase HandFocus()
	{
		_handControl.SetHandState(HandControl.HandState.Focus);
		return ParallelVfxPlayer.Create(InstantVfx.Create(delegate
		{
			RearrangeHand(0.2f);
		}), WaitVfx.Create(0.2f));
	}

	public virtual VfxBase FocusRearrangeHandHand()
	{
		if (_handControl.IsHandStateFocus())
		{
			return HandFocus();
		}
		return HandUnfocus();
	}

	public void ReplaceCardInView(IBattleCardView originalView, IBattleCardView newView)
	{
		ReplaceCardInViewWithoutRearrange(originalView, newView);
	}

	public void ReplaceCardInViewWithoutRearrange(IBattleCardView originalView, IBattleCardView newView)
	{
		_battleCardViewList.Insert(_battleCardViewList.IndexOf(originalView), newView);
		_battleCardViewList.RemoveAt(_battleCardViewList.IndexOf(originalView));
	}

	public virtual VfxBase AsyncTouchCard(GameObject card)
	{
		return NullVfx.GetInstance();
	}
}
