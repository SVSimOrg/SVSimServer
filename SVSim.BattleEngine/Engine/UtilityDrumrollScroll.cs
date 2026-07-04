using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

public class UtilityDrumrollScroll : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UIGrid _gridItemList;

	[SerializeField]
	private UICenterOnChild _centerOnChild;

	[SerializeField]
	private GameObject _itemOriginal;

	private List<GameObject> _itemList = new List<GameObject>();

	private bool _isDrag;

	private Action<int> _selectCallBack;

	[SerializeField]
	private UIButton _upButton;

	[SerializeField]
	private UIButton _downButton;

	[SerializeField]
	private UIPanel _panel;

	public int CurrentIndex { get; private set; }

	public IEnumerator CreateDrumrollScroll_Coroutine(List<string> textList, int defaultIndex, Action<int> selectCallback, Action callBack = null)
	{
		if (_panel != null)
		{
			_panel.alpha = 0f;
		}
		_InitList();
		CurrentIndex = defaultIndex;
		_selectCallBack = selectCallback;
		for (int i = 0; i < textList.Count; i++)
		{
			_AddList(i, textList[i]);
			UIEventListener uIEventListener = UIEventListener.Get(_itemList[i]);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject g)
			{
				_OnChangeScroll(g);
			});
		}
		yield return null;
		_centerOnChild.onCenter = null;
		float springStrength = _centerOnChild.springStrength;
		_centerOnChild.springStrength = 100f;
		_centerOnChild.CenterOn(_itemList[defaultIndex].transform);
		_centerOnChild.springStrength = springStrength;
		_centerOnChild.onCenter = delegate(GameObject g)
		{
			_isDrag = true;
			_OnChangeScroll(g);
		};
		if (_itemList.Count <= 1)
		{
			_scrollView.enabled = false;
		}
		if (_upButton != null)
		{
			_upButton.onClick.Add(new EventDelegate(delegate
			{
				OnClickUpButton();
			}));
		}
		if (_downButton != null)
		{
			_downButton.onClick.Add(new EventDelegate(delegate
			{
				OnClickDownButton();
			}));
		}
		callBack.Call();
		yield return null;
		if (_panel != null)
		{
			_panel.alpha = 1f;
		}
	}

	private void OnClickUpButton()
	{
		if (CurrentIndex > 0)
		{
			_centerOnChild.CenterOn(_itemList[CurrentIndex - 1].transform);
		}
	}

	private void OnClickDownButton()
	{
		if (CurrentIndex < _itemList.Count - 1)
		{
			_centerOnChild.CenterOn(_itemList[CurrentIndex + 1].transform);
		}
	}

	private void _InitList()
	{
		if (_itemList.Count > 0)
		{
			foreach (GameObject item in _itemList)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		_itemList.Clear();
		_itemOriginal.gameObject.SetActive(value: false);
	}

	private void _AddList(int index, string text, bool isNew = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_itemOriginal.gameObject);
		gameObject.transform.parent = _gridItemList.transform;
		gameObject.transform.localScale = _itemOriginal.transform.localScale;
		gameObject.name = index.ToString();
		gameObject.gameObject.SetActive(value: true);
		UtilityDrumrollItem utilityDrumrollItem = gameObject.GetComponent<UtilityDrumrollItem>();
		if (utilityDrumrollItem == null)
		{
			utilityDrumrollItem = gameObject.AddComponent<UtilityDrumrollItem>();
		}
		utilityDrumrollItem.Init(_scrollView.GetComponent<UIPanel>(), index, text);
		if (utilityDrumrollItem.CustomizeItem != null)
		{
			utilityDrumrollItem.CustomizeItem.OnInitialize(index);
		}
		_itemList.Add(gameObject);
		_gridItemList.repositionNow = true;
		_scrollView.ResetPosition();
	}

	private void _OnChangeScroll(GameObject g)
	{
		int index = g.GetComponent<UtilityDrumrollItem>()._index;
		if (CurrentIndex != index)
		{
			CurrentIndex = index;

			if (!_isDrag)
			{
				_centerOnChild.CenterOn(g.transform);
			}
			else
			{
				_isDrag = false;
			}
			_selectCallBack.Call(CurrentIndex);
		}
	}
}
