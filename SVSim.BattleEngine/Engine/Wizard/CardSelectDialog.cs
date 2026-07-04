using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class CardSelectDialog : MonoBehaviour
{
	[SerializeField]
	private UILabel _instructionLabel;

	[SerializeField]
	private UIGrid _objectsGrid;

	[SerializeField]
	private CardSelectDialogObject _objectOriginal;

	[SerializeField]
	private GameObject _pageChangeButtonsRoot;

	[SerializeField]
	private UIButton _previousPageButton;

	[SerializeField]
	private UIButton _nextPageButton;

	[SerializeField]
	private UIEventListener _dragCollider;

	[SerializeField]
	private GameObject _paginationDotsRoot;

	[SerializeField]
	private PaginationDots _paginationDotsPrefab;

	[SerializeField]
	private GameObject _cardDetailRoot;

	private int _activePageNo = 1;

	private int _pageNum;

	private List<string> _unloadAssetList = new List<string>();

	private CardSelectDialogObject[] _dialogObjectList;

	private List<UIBase_CardManager.CardObjData> _cardObjectList;

	private PaginationDots _paginationDots;

	private CardDetailUI _cardDetailDialog;

	private bool _isLoopPage;

	private bool _isDrag;

	public bool IsReady { get; private set; }

	public void Init(List<int> cardList, Action<int> selectCallback, string instructionText)
	{
		_instructionLabel.text = instructionText;
		StartCoroutine(InitCoroutine(cardList, selectCallback));
	}

	private IEnumerator InitCoroutine(List<int> cardList, Action<int> selectCallback)
	{
		_pageChangeButtonsRoot.SetActive(value: false);
		yield return StartCoroutine(CreateCardObjectCoroutine(cardList));
		CreateDialogObject(selectCallback);
		CreateCardDetailDialog();
		_pageNum = (cardList.Count - 1) / 3 + 1;
		bool flag = _pageNum > 1;
		_pageChangeButtonsRoot.SetActive(flag);
		_dragCollider.gameObject.SetActive(flag);
		_paginationDotsRoot.SetActive(flag);
		if (flag)
		{
			UIEventListener.Get(_previousPageButton.gameObject).onClick = delegate
			{
				PreviousPage();
			};
			UIEventListener.Get(_nextPageButton.gameObject).onClick = delegate
			{
				NextPage();
			};
			_dragCollider.onDragStart = OnDragStart;
			_dragCollider.onDrag = OnDrag;
			_paginationDots = NGUITools.AddChild(_paginationDotsRoot, _paginationDotsPrefab.gameObject).GetComponent<PaginationDots>();
			_paginationDots.Init(_pageNum, 16, 26, 1);
		}
		SetActivePage(1);
		IsReady = true;
	}

	private IEnumerator CreateCardObjectCoroutine(List<int> cardList)
	{
		bool isCreated = false;
		UIManager.GetInstance().CardLoadSelect(null, cardList, base.gameObject.layer, is2D: true, delegate
		{
			List<UIBase_CardManager.CardObjData> cardList2DObjs = UIManager.GetInstance().getCardList2DObjs();
			_cardObjectList = new List<UIBase_CardManager.CardObjData>(cardList2DObjs);
			cardList2DObjs.Clear();
			List<string> cardListAssetPathList = Toolbox.ResourcesManager.CardListAssetPathList;
			_unloadAssetList.AddRange(new List<string>(cardListAssetPathList));
			cardListAssetPathList.Clear();
			isCreated = true;
		});
		while (!isCreated)
		{
			yield return null;
		}
	}

	private void CreateDialogObject(Action<int> selectCallback)
	{
		int count = _cardObjectList.Count;
		_dialogObjectList = new CardSelectDialogObject[count];
		for (int i = 0; i < count; i++)
		{
			CardSelectDialogObject cardSelectDialogObject = (_dialogObjectList[i] = NGUITools.AddChild(_objectsGrid.gameObject, _objectOriginal.gameObject).GetComponent<CardSelectDialogObject>());
			UIBase_CardManager.CardObjData cardObjData = _cardObjectList[i];
			UIEventListener.Get(cardSelectDialogObject.DecisionButton.gameObject).onClick = delegate
			{
				selectCallback(cardObjData.ids);
			};
			GameObject cardObject = cardObjData.CardObj;
			CardListTemplate component = cardObject.GetComponent<CardListTemplate>();
			component.SetParentAndResetPos(cardSelectDialogObject.CardObjectRoot.transform);
			component.transform.localScale = Vector3.one;
			component.HideNum();
			UIEventListener uIEventListener = component.AddColliderToFrame();
			uIEventListener.onClick = delegate
			{
				_cardDetailDialog.OnPushCardDetailOn(cardObject);
			};
			uIEventListener.onDragStart = OnDragStart;
			uIEventListener.onDrag = OnDrag;
			DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
			cardSelectDialogObject.PossessionNumLabel.text = string.Format(Data.SystemText.Get("Sealed_RewardCardSelect_0005"), dataMgr.GetPossessionCardNum(cardObjData.ids, isIncludingSpotCard: false).ToString(), dataMgr.SpotCardData.ExistsSpotCard(cardObjData.ids) ? $"[fcd24a]+{dataMgr.SpotCardData.GetSpotCardNum(cardObjData.ids).ToString()}[-]" : string.Empty);
		}
		_objectsGrid.Reposition();
	}

	private void CreateCardDetailDialog()
	{
		_cardDetailDialog = DialogCreator.CreateCardDetailDialog(_cardDetailRoot, "Detail");
		_cardDetailDialog.gameObject.SetActive(value: false);
	}

	private void SetActivePage(int pageNo)
	{
		_activePageNo = pageNo;
		_paginationDots?.SetActivePageNumber(pageNo);
		if (!_isLoopPage)
		{
			_previousPageButton.gameObject.SetActive(pageNo != 1);
			_nextPageButton.gameObject.SetActive(pageNo != _pageNum);
		}
		for (int i = 0; i < _dialogObjectList.Length; i++)
		{
			GameObject gameObject = _dialogObjectList[i].gameObject;
			bool flag = i / 3 + 1 == pageNo;
			if (flag)
			{
				gameObject.SetActive(value: false);
			}
			gameObject.SetActive(flag);
		}
		Vector3 localPosition = _objectsGrid.transform.localPosition;
		if (pageNo == 1)
		{
			_objectsGrid.transform.localPosition = new Vector3(0f, localPosition.y, localPosition.z);
			_objectsGrid.pivot = UIWidget.Pivot.Center;
		}
		else
		{
			_objectsGrid.transform.localPosition = new Vector3(0f - _objectsGrid.cellWidth, localPosition.y, localPosition.z);
			_objectsGrid.pivot = UIWidget.Pivot.Left;
		}
		_objectsGrid.Reposition();
	}

	private void PreviousPage()
	{
		bool flag = _activePageNo == 1;
		if (!flag || _isLoopPage)
		{

			SetActivePage(flag ? _pageNum : (_activePageNo - 1));
		}
	}

	private void NextPage()
	{
		bool flag = _activePageNo == _pageNum;
		if (!flag || _isLoopPage)
		{

			SetActivePage(flag ? 1 : (_activePageNo + 1));
		}
	}

	private void OnDragStart(GameObject g)
	{
		_isDrag = true;
	}

	private void OnDrag(GameObject g, Vector2 vec)
	{
		if (_isDrag)
		{
			if (vec.x >= 70f)
			{
				_isDrag = false;
				PreviousPage();
			}
			else if (vec.x <= -70f)
			{
				_isDrag = false;
				NextPage();
			}
		}
	}
}
