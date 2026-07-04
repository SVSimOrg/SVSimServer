using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class PlayQueueViewBase
{

	private Action _computeCorners;

	private List<IBattleCardView> queuedCards = new List<IBattleCardView>();

	public Vector3 worldTopCornerPosition { get; private set; }

	public Vector3 worldBottomCornerPosition { get; private set; }

	protected abstract BattlePlayerBase BattlePlayerBase { get; }

	protected abstract float RotationAmount { get; }

	protected abstract Vector3 ScreenTopCornerPosition { get; }

	protected abstract Vector3 ScreenBottomCornerPosition { get; }

	public PlayQueueViewBase()
	{
	}

	public PlayQueueViewBase(BattleCamera battleCamera)
	{
		PlayQueueViewBase playQueueViewBase = this;
		Camera cutInCamera = battleCamera.m_CutInCamera.GetComponent<Camera>();
		_computeCorners = delegate
		{
			float aspect = cutInCamera.aspect;
			playQueueViewBase.worldTopCornerPosition = cutInCamera.ScreenToWorldPoint(playQueueViewBase.ScreenTopCornerPosition);
			playQueueViewBase.worldTopCornerPosition = playQueueViewBase.BattlePlayerBase.HandControl.Transform.InverseTransformPoint(playQueueViewBase.worldTopCornerPosition);
			playQueueViewBase.worldTopCornerPosition += playQueueViewBase.GetScreenTopCornerOffset(aspect);
			playQueueViewBase.worldTopCornerPosition = playQueueViewBase.BattlePlayerBase.HandControl.Transform.TransformPoint(playQueueViewBase.worldTopCornerPosition);
			playQueueViewBase.worldBottomCornerPosition = cutInCamera.ScreenToWorldPoint(playQueueViewBase.ScreenBottomCornerPosition);
			playQueueViewBase.worldBottomCornerPosition = playQueueViewBase.BattlePlayerBase.HandControl.Transform.InverseTransformPoint(playQueueViewBase.worldBottomCornerPosition);
			playQueueViewBase.worldBottomCornerPosition += playQueueViewBase.GetScreenBottomCornerOffset(aspect);
			playQueueViewBase.worldBottomCornerPosition = playQueueViewBase.BattlePlayerBase.HandControl.Transform.TransformPoint(playQueueViewBase.worldBottomCornerPosition);
		};
		_computeCorners();
	}

	public int QueueCount()
	{
		return queuedCards.Count;
	}

	public virtual void RemoveCardFromView(IBattleCardView cardViewToRemove, bool keepLayer = false)
	{
		cardViewToRemove.ResetPlayQueueFlags();
		if (queuedCards.Count > 0 && queuedCards[0] == cardViewToRemove)
		{
			queuedCards.RemoveAt(0);
		}
	}

	public virtual void ReplaceCard(IBattleCardView cardViewToAdd, IBattleCardView cardViewToRemove)
	{
		cardViewToAdd._hasCardEnteredPlayQueue = true;
		cardViewToAdd._waitUntilCardIsInQueueCoroutine = BattleCoroutine.GetInstance().StartCoroutine(WaitUntilCardIsInQueue(cardViewToAdd));
		cardViewToRemove.ResetPlayQueueFlags();
		if (queuedCards.Count > 0 && queuedCards[0] == cardViewToRemove)
		{
			queuedCards.RemoveAt(0);
			queuedCards.Insert(0, cardViewToAdd);
		}
	}

	public bool IsCardInQueue(IBattleCardView battleCardView)
	{
		return queuedCards.Contains(battleCardView);
	}

	public void UpdatePlayQueuePositions(float deltaTime)
	{
		if (_computeCorners != null)
		{
			_computeCorners();
		}
		Vector3 delta = worldBottomCornerPosition - worldTopCornerPosition;
		for (int i = 0; i < queuedCards.Count; i++)
		{
			float t = (float)i / (float)queuedCards.Count;
			if (queuedCards[i].GameObject.activeSelf)
			{
				Vector3 b = CalculatePositionInQueue(t, delta, worldTopCornerPosition);
				queuedCards[i].Transform.position = Vector3.Lerp(queuedCards[i].Transform.position, b, MotionUtils.CalculateFrameRateIndependantDampingConstant(0.01f, 5f));
			}
		}
	}

	private Vector3 CalculatePositionInQueue(float t, Vector3 delta, Vector3 queueTopPosition)
	{
		return new Vector3(delta.x * MotionUtils.GetEase(t, MotionUtils.EaseType.easeOutQuad), delta.y * MotionUtils.GetEase(t, MotionUtils.EaseType.linear), delta.z * MotionUtils.GetEase(t, MotionUtils.EaseType.linear)) + queueTopPosition;
	}

	private IEnumerator WaitUntilCardIsInQueue(IBattleCardView playedCardView)
	{
		yield return new WaitForSeconds(0.5f);
		playedCardView._isCardQueuedToBePlayed = true;
	}

	public abstract VfxBase AddCardToViewVfx(IBattleCardView playedCardView, bool forceCardIntoPlayQueue, bool isSelectTarget, bool isChoice, bool isChoiceBrave = false);

	public abstract VfxBase InstantAddCardToViewVfx(IBattleCardView playedCardView, bool forceCardIntoPlayQueue, bool isChoice);

	protected abstract Vector3 GetScreenTopCornerOffset(float aspectRatio);

	protected abstract Vector3 GetScreenBottomCornerOffset(float aspectRatio);
}
