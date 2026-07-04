using System;
using System.Collections;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

[RequireComponent(typeof(CardSelectListUI_Positioning))]
public class CardSelectListUI_State_Edit : MecanimStateBase
{

	[SerializeField]
	private CardSelectListUIBase m_scene;

	[SerializeField]
	private CardSelectListUI_State_CardDrag m_stateDragMode;

	[SerializeField]
	private UIScrollView m_scrollView;

	private UIPanel _scrollViewPanel;

	private SpringPanel _springPanel;

	[SerializeField]
	private UICenterOnChild m_scrollViewCenterOnChild;

	[SerializeField]
	private CardSelectListUI_Positioning _selectionAreaCardPositioning;

	[SerializeField]
	private CardSelectListUI_Positioning _pagingAreaCardPositioning;

	[SerializeField]
	private UIPanel _maskPanel;

	private GameObject _pressObj;

	private float _pressTime;

	private Vector2 _pressPoint = Vector2.zero;

	private Coroutine _pagingCoroutine;

	private bool _requestImmediateMove;

	public bool IsClick { get; private set; }

	public override void onOpen()
	{
		base.onOpen();
		RefreshSelectionArea(isImmediate: false);
		RefreshPage(isImmediate: false);
		m_scrollView.enabled = true;
		_maskPanel.clipping = UIDrawCall.Clipping.None;
	}

	public override void onClose()
	{
		base.onClose();
		RefreshSelectionArea(isImmediate: false);
		RefreshPage(isImmediate: false);
	}

	public void RefreshSelectionArea(bool isImmediate)
	{
		RefreshBase(m_scene.SelectionAreaList, onPressSelectionAreaCard, delegate(GameObject o)
		{
			onDoubleClick(o, CardSelectListUI_State_CardDrag.Mode.InToOut);
		}, _selectionAreaCardPositioning, isMerge: false, isImmediate);
	}

	public void RefreshPage(bool isImmediate)
	{
		RefreshBase(m_scene.PagingList, onPressPagingCard, delegate(GameObject o)
		{
			onDoubleClick(o, CardSelectListUI_State_CardDrag.Mode.OutToIn);
		}, _pagingAreaCardPositioning, isMerge: false, isImmediate);
	}

	private void RefreshBase(CardBundle cardList, UIEventListener.BoolDelegate callback, UIEventListener.VoidDelegate doubleClickCallback, CardSelectListUI_Positioning positioning, bool isMerge, bool isImmediate)
	{
		int countKind = cardList.CountKind;
		GameObject[] array = new GameObject[countKind];
		for (int i = 0; i < countKind; i++)
		{
			array[i] = cardList.FindWithIndex(i).CardObj;
			if (!(array[i] != null))
			{
				continue;
			}
			BoxCollider component = array[i].GetComponent<BoxCollider>();
			if (!component)
			{
				component = array[i].AddComponent<BoxCollider>();
				UIWidget component2 = array[i].GetComponent<UIWidget>();
				if ((bool)component2)
				{
					component.size = component2.localSize;
				}
			}
			UIEventListener uIEventListener = UIEventListener.Get(array[i]);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, callback);
			UIEventListener uIEventListener2 = UIEventListener.Get(array[i]);
			uIEventListener2.onPressRight = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPressRight, callback);
			UIEventListener uIEventListener3 = UIEventListener.Get(array[i]);
			uIEventListener3.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onDoubleClick, doubleClickCallback);
		}
		if (isMerge)
		{
			positioning.Add(array);
		}
		else
		{
			positioning.Change(array);
		}
		if (isImmediate)
		{
			positioning.Immediate();
		}
	}

	public void NextPage(Action onMoved)
	{
		if (_pagingCoroutine != null)
		{
			StopCoroutine(_pagingCoroutine);
		}
		_pagingCoroutine = StartCoroutine(PagingBase(-1f, onMoved));
	}

	public void PrevPage(Action onMoved)
	{
		if (_pagingCoroutine != null)
		{
			StopCoroutine(_pagingCoroutine);
		}
		_pagingCoroutine = StartCoroutine(PagingBase(1f, onMoved));
	}

	private IEnumerator PagingBase(float direction, Action onMoved)
	{
		Vector3 offset = _pagingAreaCardPositioning.Offset;
		offset.x = 1400f * direction;
		_pagingAreaCardPositioning.Offset = offset;
		_pagingAreaCardPositioning.Speed = 0.4f;
		while (_pagingAreaCardPositioning.IsMoving)
		{
			yield return null;
		}
		offset.x = 1400f * (0f - direction);
		_pagingAreaCardPositioning.Offset = offset;
		_pagingAreaCardPositioning.Speed = 1f;
		onMoved.Call();
		yield return null;
		offset.x = 0f;
		_pagingAreaCardPositioning.Offset = offset;
		_pagingAreaCardPositioning.Speed = 0.4f;
	}

	public void Fit()
	{
		SetupScrollComponent();
		float num = Mathf.Max(0f, _scrollViewPanel.clipOffset.x);
		int a = Mathf.Max(0, m_scene.SelectionAreaList.CountKind - 10);
		float breakWidth = _selectionAreaCardPositioning.BreakWidth;
		int b = Mathf.RoundToInt(num / breakWidth);
		Fit(Mathf.Min(a, b));
		m_scrollView.customMovement = ((m_scene.SelectionAreaList.CountKind > 10) ? Vector2.right : Vector2.zero);
	}

	public void CenterOn(int idx)
	{
		SetupScrollComponent();
		idx -= 5;
		int a = Mathf.Max(0, m_scene.SelectionAreaList.CountKind - 10);
		int a2 = 0;
		Fit(Mathf.Max(a2, Mathf.Min(a, idx)));
	}

	private void Fit(int idx)
	{
		float breakWidth = _selectionAreaCardPositioning.BreakWidth;
		_springPanel.target.x = (float)(-idx) * breakWidth;
		_springPanel.enabled = true;
	}

	public void ResetScroll()
	{
		SetupScrollComponent();
		m_scrollView.ResetPosition();
		_springPanel.target.x = 0f;
		_springPanel.enabled = true;
	}

	private void SetupScrollComponent()
	{
		if (_scrollViewPanel == null)
		{
			_scrollViewPanel = m_scrollView.GetComponent<UIPanel>();
		}
		if (_springPanel == null)
		{
			_springPanel = m_scrollView.GetComponent<SpringPanel>();
		}
		m_scrollViewCenterOnChild.gameObject.SetActive(value: true);
	}

	public void RemoveSelectionArea()
	{
		_selectionAreaCardPositioning.Clear();
	}

	private void StartDragState(bool immediate)
	{
		CardObject cardObject = m_stateDragMode.TryGetCard(_pressObj);
		if (cardObject != null)
		{
			m_scene.HideDetail();
			m_scene.ChangeState(m_stateDragMode, skipCloseAnim: true);
			m_stateDragMode.ImmediateMove = immediate;
			m_stateDragMode.CreateDragCard(cardObject);
			m_scrollView.enabled = false;
			m_scrollViewCenterOnChild.gameObject.SetActive(value: true);
			IsClick = false;
		}
		_pressObj = null;
		_pressTime = 0f;
	}

	public override void onMove()
	{
		base.onMove();
		if (!_pressObj)
		{
			return;
		}
		if (_requestImmediateMove)
		{
			StartDragState(immediate: true);
			_requestImmediateMove = false;
		}
		else if (Input.GetMouseButton(0))
		{
			IsClick = true;
			_pressTime += Time.deltaTime;
			Vector2 vector = _pressPoint - (Vector2)Input.mousePosition;
			vector.Normalize();
			float num = Mathf.Abs(vector.y);
			float num2 = Vector2.Distance(_pressPoint, Input.mousePosition);
			if (num2 >= 35f && num < 0.8f)
			{
				_pressObj = null;
				_pressTime = 0f;
			}
			else if (_pressTime >= 0.5f || (num >= 0.8f && num2 >= 35f))
			{
				StartDragState(immediate: false);
			}
		}
		else
		{
			_pressObj = null;
			_pressTime = 0f;
		}
	}

	private void onPressSelectionAreaCard(GameObject obj, bool state)
	{
		if (!m_scene.IsLoading)
		{
			bool mouseButton = Input.GetMouseButton(0);
			bool flag = !mouseButton && Input.GetMouseButtonDown(1);
			if (mouseButton || flag)
			{
				initPress(obj, CardSelectListUI_State_CardDrag.Mode.InToOut);
				_requestImmediateMove = flag;
			}
		}
	}

	private void onPressPagingCard(GameObject obj, bool state)
	{
		if (!m_scene.IsLoading)
		{
			bool mouseButton = Input.GetMouseButton(0);
			bool flag = !mouseButton && Input.GetMouseButtonDown(1);
			if (mouseButton || flag)
			{
				initPress(obj, CardSelectListUI_State_CardDrag.Mode.OutToIn);
				_requestImmediateMove = flag;
			}
		}
	}

	private void initPress(GameObject obj, CardSelectListUI_State_CardDrag.Mode mode)
	{
		m_stateDragMode.EditMode = mode;
		_pressObj = obj;
		_pressPoint = Input.mousePosition;
	}

	private void onDoubleClick(GameObject obj, CardSelectListUI_State_CardDrag.Mode mode)
	{
		if (!m_scene.IsLoading)
		{
			_requestImmediateMove = true;
			initPress(obj, mode);
		}
	}
}
