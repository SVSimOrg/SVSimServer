using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class DrumrollScrollManager : MonoBehaviour
{

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UIGrid _itemListParent;

	[SerializeField]
	private UICenterOnChild _gridCenter;

	[SerializeField]
	private UIButton _ButtonScrollPrev;

	[SerializeField]
	private UIButton _ButtonScrollNext;

	private List<GameObject> _itemList = new List<GameObject>();

	private bool _isDrag;

	private int _CurrentIndex;

	private Action<int> _SelectCallBack;

	public IEnumerator CreateDrumrollScroll_Coroutine(List<GameObject> itemObjList, int defaultIndex, Action<int> selectCallback, Action onCreateCallBack = null)
	{
		InitItemList(itemObjList);
		_CurrentIndex = defaultIndex;
		_SelectCallBack = selectCallback;
		yield return null;
		_gridCenter.onCenter = null;
		float springStrength = _gridCenter.springStrength;
		_gridCenter.springStrength = 5000f;
		_gridCenter.onFinished = delegate
		{
			_gridCenter.onFinished = null;
			TweenAlpha.Begin(base.gameObject, 0f, 1f);
		};
		_gridCenter.CenterOn(_itemList[defaultIndex].transform);
		_gridCenter.springStrength = springStrength;
		_gridCenter.onCenter = null;
		_gridCenter.onCenter = delegate(GameObject g)
		{
			_isDrag = true;
			OnChangeScroll(g);
		};
		_ButtonScrollPrev.onClick.Clear();
		_ButtonScrollPrev.onClick.Add(new EventDelegate(delegate
		{
			OnPushBtnPrev();
		}));
		_ButtonScrollNext.onClick.Clear();
		_ButtonScrollNext.onClick.Add(new EventDelegate(delegate
		{
			OnPushBtnNext();
		}));
		SetDrumrollButton();
		if (_itemList.Count <= 1)
		{
			_scrollView.enabled = false;
		}
		onCreateCallBack.Call();
	}

	private void InitItemList(List<GameObject> itemObjList)
	{
		_scrollView.currentMomentum = Vector3.zero;
		_scrollView.DisableSpring();
		_scrollView.momentumAmount = 70f;
		_itemList = itemObjList;
		for (int i = 0; i < _itemList.Count; i++)
		{
			GameObject obj = _itemList[i];
			obj.transform.parent = _itemListParent.transform;
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.down * i * _itemListParent.cellHeight;
			obj.name = i.ToString();
			obj.gameObject.SetActive(value: true);
			UIEventListener uIEventListener = UIEventListener.Get(obj);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject g)
			{
				OnChangeScroll(g);
			});
		}
		_itemListParent.repositionNow = true;
		_scrollView.ResetPosition();
	}

	private void OnChangeScroll(GameObject g)
	{
		int num = int.Parse(g.name);
		if (_CurrentIndex != num)
		{
			_CurrentIndex = num;

			if (!_isDrag)
			{
				_gridCenter.CenterOn(g.transform);
			}
			else
			{
				_isDrag = false;
			}
			SetDrumrollButton();
			_SelectCallBack.Call(_CurrentIndex);
		}
	}

	private void SetDrumrollButton()
	{
		if (_itemList.Count <= 1)
		{
			_ButtonScrollNext.gameObject.SetActive(value: false);
			_ButtonScrollPrev.gameObject.SetActive(value: false);
		}
		else if (_CurrentIndex == _itemList.Count - 1)
		{
			_ButtonScrollNext.gameObject.SetActive(value: false);
			_ButtonScrollPrev.gameObject.SetActive(value: true);
		}
		else if (_CurrentIndex == 0)
		{
			_ButtonScrollNext.gameObject.SetActive(value: true);
			_ButtonScrollPrev.gameObject.SetActive(value: false);
		}
		else
		{
			_ButtonScrollNext.gameObject.SetActive(value: true);
			_ButtonScrollPrev.gameObject.SetActive(value: true);
		}
	}

	private void OnPushBtnNext()
	{
		int currentIndex = _CurrentIndex;
		if (_CurrentIndex < _itemList.Count - 1)
		{
			currentIndex++;
			_isDrag = false;
			OnChangeScroll(_itemList[currentIndex].gameObject);
		}
	}

	private void OnPushBtnPrev()
	{
		int currentIndex = _CurrentIndex;
		if (_CurrentIndex > 0)
		{
			currentIndex--;
			_isDrag = false;
			OnChangeScroll(_itemList[currentIndex].gameObject);
		}
	}
}
