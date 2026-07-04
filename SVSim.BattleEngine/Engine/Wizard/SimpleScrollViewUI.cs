using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public class SimpleScrollViewUI : MonoBehaviour
{
	public enum VerticalMovement
	{
		Top,
		Center,
		Bottom
	}

	[SerializeField]
	private int SCROLL_OBJECT_NUM = 6;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UIWrapContent _wrapContent;

	[SerializeField]
	private UIScrollBarWrapContent _scrollBarWrapContent;

	[SerializeField]
	private WrapContentsScrollBarSize _wrapContentScrollBarSize;

	[SerializeField]
	private GameObject _plateOriginal;

	[SerializeField]
	private List<GameObject> _plateOriginals;

	private List<GameObject> _plateList = new List<GameObject>();

	private int _contentsNum;

	private Action<int, GameObject> _initializePlate;

	public List<GameObject> ActivePlateList => _plateList.Where((GameObject p) => p.activeInHierarchy).ToList();

	public void CreateScrollView(int contentsNum, Action<int, GameObject> InitializePlate)
	{
		_contentsNum = contentsNum;
		_initializePlate = InitializePlate;
		InitPlateList();
		SetScrollView();
	}

	public void CreateScrollView(List<int> indexes, Action<int, GameObject> InitializePlate)
	{
		_contentsNum = indexes.Count();
		_initializePlate = InitializePlate;
		InitPlateList();
		SetScrollView(indexes);
	}

	private void InitPlateList()
	{
		for (int i = 0; i < _plateList.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(_plateList[i].gameObject);
		}
		_plateList.Clear();
		if (_plateOriginal != null)
		{
			_plateOriginal.gameObject.SetActive(value: false);
		}
		else
		{
			if (_plateOriginals == null)
			{
				return;
			}
			foreach (GameObject plateOriginal in _plateOriginals)
			{
				plateOriginal.SetActive(value: false);
			}
		}
	}

	private void SetScrollView(List<int> indexes = null)
	{
		_scrollView.gameObject.SetActive(value: true);
		_scrollBarWrapContent.gameObject.SetActive(value: true);
		_wrapContent.gameObject.SetActive(value: false);
		_wrapContent.cullContent = false;
		_wrapContent.EnableNoLimit = false;
		int num = Mathf.Min(_contentsNum, SCROLL_OBJECT_NUM);
		for (int i = 0; i < num; i++)
		{
			GameObject prefab = _plateOriginal;
			if (indexes != null && _plateOriginals != null)
			{
				prefab = _plateOriginals[indexes[i]];
			}
			GameObject gameObject = NGUITools.AddChild(_wrapContent.gameObject, prefab);
			gameObject.SetActive(value: false);
			_plateList.Add(gameObject);
		}
		_wrapContent.minIndex = -(_contentsNum - 1);
		_wrapContent.maxIndex = 0;
		_wrapContent.onInitializeItem = OnInitializeItem;
		_wrapContent.gameObject.SetActive(value: true);
		_wrapContent.enabled = true;
		ResetScrollView();
	}

	private void ResetScrollView()
	{
		_wrapContentScrollBarSize.ContentUpdate();
		_wrapContent.SortBasedOnScrollMovement();
		_scrollView.ResetPosition();
		_wrapContent.WrapContent();
	}

	private void OnInitializeItem(GameObject obj, int wrapIndex, int realIndex)
	{
		int num = -realIndex;
		if (num >= _contentsNum || num < 0)
		{
			obj.SetActive(value: false);
			return;
		}
		obj.SetActive(value: true);
		_initializePlate(num, obj);
	}

	public void MovePlateByIndex(int index, VerticalMovement move, bool allowMargin = false)
	{
		if (index < 0 || _contentsNum <= index || _scrollView.movement != UIScrollView.Movement.Vertical)
		{
			return;
		}
		UIPanel component = _scrollView.GetComponent<UIPanel>();
		float viewPanelHeight = component.height - 2f * component.clipSoftness.y;
		float num = _wrapContent.itemSize;
		if (!allowMargin)
		{
			if (HasMarginOnTopAfterMove(index, move, viewPanelHeight, num))
			{
				index = 0;
				move = VerticalMovement.Top;
			}
			else if (HasMarginOnButtonAfterMove(index, move, viewPanelHeight, num))
			{
				index = _contentsNum - 1;
				move = VerticalMovement.Bottom;
			}
		}
		float num2 = CalcVerticalMovementOffset(move, viewPanelHeight, num);
		float y = num * (float)index - num2;
		Vector3 relative = new Vector3(0f, y, 0f);
		_scrollView.ResetPosition();
		_scrollView.MoveRelative(relative);
		_wrapContent.WrapContent();
	}

	private bool HasMarginOnTopAfterMove(int index, VerticalMovement move, float viewPanelHeight, float plateSize)
	{
		return move switch
		{
			VerticalMovement.Top => false, 
			VerticalMovement.Center => plateSize * (float)index + plateSize / 2f < viewPanelHeight / 2f, 
			VerticalMovement.Bottom => plateSize * (float)(index + 1) < viewPanelHeight, 
			_ => false, 
		};
	}

	private bool HasMarginOnButtonAfterMove(int index, VerticalMovement move, float viewPanelHeight, float plateSize)
	{
		return move switch
		{
			VerticalMovement.Top => plateSize * (float)(_contentsNum - index) < viewPanelHeight, 
			VerticalMovement.Center => plateSize * (float)(_contentsNum - (index + 1)) + plateSize / 2f < viewPanelHeight / 2f, 
			VerticalMovement.Bottom => false, 
			_ => false, 
		};
	}

	private float CalcVerticalMovementOffset(VerticalMovement move, float viewPanelHeight, float plateSize)
	{
		return move switch
		{
			VerticalMovement.Top => 0f, 
			VerticalMovement.Center => (viewPanelHeight - plateSize) / 2f, 
			VerticalMovement.Bottom => viewPanelHeight - plateSize, 
			_ => 0f, 
		};
	}
}
