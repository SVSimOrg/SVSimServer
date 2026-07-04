using System.Collections.Generic;
using UnityEngine;
using Wizard;
using Wizard.Battle.View;

public abstract class HandControl
{
	public enum ArrangeType
	{
		Fan,
		Flat
	}

	public enum HandState
	{
		Unfocus,
		Focus
	}

	public enum HandVisible
	{
		Invisible	}

	protected readonly GameObject _gameObject;

	protected HandState _handState;

	protected HandVisible _handVisible;

	protected Vector3[] _cardPos;

	protected Vector3[] _cardRot;

	protected Vector3[] _cardScale;

	protected readonly BattleCamera _battleCamera;

	private HandTRSCalculatorBase _TRSCalculator;

	public Transform Transform { get; private set; }

	public bool IsHandStateLocked { get; private set; }

	public abstract Vector3 BaseHandPos { get; }

	public HandControl(GameObject gameObject, BattleCamera battleCamera)
	{
		_gameObject = gameObject;
		Transform = _gameObject.transform;
		Transform.localPosition = BaseHandPos;
		_handState = HandState.Unfocus;
		_handVisible = HandVisible.Invisible;
		_battleCamera = battleCamera;
		_cardPos = new Vector3[9];
		_cardRot = new Vector3[9];
		_cardScale = new Vector3[9];
		_TRSCalculator = CreateHandCardTRSCalculator(PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.FIXEDUSE_COST_INFO) ? ArrangeType.Flat : ArrangeType.Fan);
	}

	public void AttachCardView(IBattleCardView cardView)
	{
		if (!(cardView is NullBattleCardView))
		{
			cardView.Transform.parent = Transform;
			cardView.GameObject.SetActive(value: false);
			cardView.GameObject.SetActive(value: true);
		}
	}

	protected abstract HandTRSCalculatorBase CreateHandCardTRSCalculator(ArrangeType type);

	public abstract void RearrangeHand(float time, List<IBattleCardView> battleCardViewList, bool isNewReplayMoveTurn = false);

	public void SetHandState(HandState state)
	{
		if (!IsHandStateLocked)
		{
			_handState = state;
		}
	}

	public bool IsHandStateFocus()
	{
		return _handState == HandState.Focus;
	}

	public void SetHandPosition()
	{
		Transform.localPosition = BaseHandPos;
	}
}
