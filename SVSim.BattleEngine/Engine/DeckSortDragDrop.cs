using UnityEngine;
using Wizard;

public class DeckSortDragDrop : UIDragDropItem
{
	protected enum DropType
	{
		None,
		Sort,
		Add
	}

	public DeckListMenuUI DeckListMenuClass;

	protected Vector3 _defaultPosition;

	protected Transform _defaultParentTransform;

	protected UIPanel _uiPanel;

	protected Camera _deckListCamera;

	protected string _sortExecTargetName;

	protected bool _isHighSpeedCheck;

	protected Vector3 _oldMousePosition;

	protected bool _isDragStart;

	protected bool _isPress;

	protected bool _isSortExecEvenOnce;

	protected float _pageChangeIntervalTime;

	protected override void Awake()
	{
		IsGridRepositionUse = false;
		_deckListCamera = UIManager.GetInstance().transform.Find("UIRoot/CameraUI").GetComponent<Camera>();
		_pageChangeIntervalTime = 0f;
	}

	protected override void Update()
	{
		if (_isHighSpeedCheck)
		{
			if (IsSortDrag() && !DeckListMenuClass.IsPlayingSortAnimation)
			{
				if ((_oldMousePosition - Input.mousePosition).magnitude == 0f)
				{
					bool isDrop = true;
					if (_isPress)
					{
						isDrop = false;
					}
					CurrentTouchPositionSort(isDrop);
					_isHighSpeedCheck = false;
				}
				_oldMousePosition = Input.mousePosition;
			}
			else
			{
				_isHighSpeedCheck = false;
			}
		}
		if (_isDragStart && !DeckListMenuClass.IsPlayingSortAnimation)
		{
			_pageChangeIntervalTime += Time.deltaTime;
			PageChangeUpdate();
		}
	}

	protected override void OnPress(bool isPressed)
	{
		if (_isPress != isPressed && DeckListMenuClass.IsSortMode)
		{
			if (isPressed && !DeckListMenuClass.IsSortDragging)
			{
				if (!DeckListMenuClass.IsPlayingSortAnimation)
				{
					base.OnPress(isPressed);
					DragDropStart();
				}
			}
			else if (IsSortDrag())
			{
				base.OnPress(isPressed);
				SetDragStatus(isDrag: false);
				Object.Destroy(_uiPanel);
				CurrentTouchPositionSort(isDrop: true);
				if (_isSortExecEvenOnce)
				{

				}
				_isSortExecEvenOnce = false;
				_isDragStart = false;
			}
		}
		_isPress = isPressed;
	}

	protected override void OnDragStart()
	{
		if (!DeckListMenuClass.IsPlayingSortAnimation)
		{
			base.OnDragStart();
		}
	}

	protected void DragDropStart()
	{
		if (DeckListMenuClass.IsSortMode && !IsSortDrag())
		{
			SetDragStatus(isDrag: true);
			mTrans = base.transform;
			base.transform.parent.GetComponent<UIGrid>().Reposition();
			_defaultParentTransform = base.transform.parent;
			_defaultPosition = base.transform.localPosition;
			base.transform.parent = _deckListCamera.transform;
			base.transform.gameObject.GetComponent<BoxCollider>().enabled = false;
			_uiPanel = base.gameObject.AddComponent<UIPanel>();
			_uiPanel.depth = 50;
			base.gameObject.GetComponent<DeckUI>().UpdateUIAlpha(0.5f);
			base.transform.position = _deckListCamera.ScreenToWorldPoint(Input.mousePosition);
			_isDragStart = true;
			_isSortExecEvenOnce = false;
			_pageChangeIntervalTime = 0.75f;

		}
	}

	protected override void OnDragDropMove(Vector2 delta)
	{
		base.transform.position = _deckListCamera.ScreenToWorldPoint(Input.mousePosition);
		if (!DeckListMenuClass.IsSortMode || DeckListMenuClass.IsPlayingSortAnimation)
		{
			return;
		}
		switch (UICamera.hoveredObject.name)
		{
		case "LeftDragArea":
			return;
		case "LeftArrow":
			return;
		case "RightArrow":
			return;
		case "RightDragArea":
			return;
		}
		if (delta.magnitude < 5f)
		{
			DeckFrame inRetObject = null;
			switch (GetSortTaget(UICamera.hoveredObject, out inRetObject))
			{
			case DropType.Sort:
				DeckSort(inRetObject.Transform.gameObject, inDrop: false);
				break;
			case DropType.Add:
				AddExec(inRetObject, inDrop: false);
				break;
			}
			_isHighSpeedCheck = false;
		}
		else
		{
			_isHighSpeedCheck = true;
		}
	}

	protected void DeckSort(GameObject inDropObject, bool inDrop)
	{
		bool flag = false;
		if (inDropObject != null)
		{
			if (inDropObject.GetComponent<DeckSortDragDrop>() != null)
			{
				if (_sortExecTargetName != inDropObject.name)
				{
					flag = true;
					if (DeckListMenuClass.DeckSort(inDropObject.name, base.name, ref _defaultPosition))
					{
						_defaultParentTransform = base.gameObject.transform.parent;
						base.transform.parent = _deckListCamera.transform;
					}
					if (inDrop)
					{
						MoveReset();
					}
					else
					{
						base.transform.position = _deckListCamera.ScreenToWorldPoint(Input.mousePosition);
					}
					_sortExecTargetName = inDropObject.name;
				}
			}
			else if (inDrop)
			{
				MoveReset();
			}
		}
		else if (inDrop)
		{
			MoveReset();
		}
		_sortExecTargetName = string.Empty;
		if (flag)
		{
			_isSortExecEvenOnce = true;
		}
	}

	protected void AddExec(DeckFrame inAddObject, bool inDrop)
	{
		DeckListMenuClass.DeckSortAddLast(inAddObject);
		_defaultParentTransform = base.gameObject.transform.parent;
		_defaultPosition = base.transform.localPosition;
		if (!inDrop)
		{
			base.transform.parent = _deckListCamera.transform;
			base.transform.position = _deckListCamera.ScreenToWorldPoint(Input.mousePosition);
		}
		else
		{
			MoveReset();
		}
		_isSortExecEvenOnce = true;
	}

	protected DropType GetSortTaget(GameObject inDropObject, out DeckFrame inRetObject)
	{
		inRetObject = new DeckFrame();
		if (inDropObject.GetComponent<DeckSortDragDrop>() != null)
		{
			string text = ((!(inDropObject.transform.position.x > _deckListCamera.ScreenToWorldPoint(Input.mousePosition).x)) ? (int.Parse(inDropObject.transform.name) + 1).ToString() : inDropObject.transform.name);
			Transform transform = null;
			for (int i = 0; i < DeckListMenuClass.DeckPageList.Count; i++)
			{
				transform = DeckListMenuClass.DeckPageList[i].transform.Find(text);
				if (transform != null)
				{
					break;
				}
			}
			if (text == base.gameObject.name)
			{
				return DropType.None;
			}
			if (transform == null || transform.GetComponent<DeckSortDragDrop>() == null)
			{
				inRetObject.Transform = base.gameObject.transform;
				inRetObject.DeckId = DeckListMenuClass.GetDeckNoFromGameObject(inRetObject.Transform.gameObject);
				return DropType.Add;
			}
			inRetObject.Transform = transform.gameObject.transform;
			inRetObject.DeckId = DeckListMenuClass.GetDeckNoFromGameObject(inRetObject.Transform.gameObject);
			return DropType.Sort;
		}
		return DropType.None;
	}

	protected void CurrentTouchPositionSort(bool isDrop)
	{
		DeckFrame inRetObject = new DeckFrame();
		GameObject gameObject = null;
		BetterList<UICamera.DepthEntry> hitsList = UICamera.GetHitsList();
		for (int i = 0; i < hitsList.buffer.Length; i++)
		{
			if (hitsList.buffer[i].go != null && (bool)hitsList.buffer[i].go.GetComponent<DeckSortDragDrop>())
			{
				gameObject = hitsList[i].go;
				break;
			}
		}
		if (gameObject != null)
		{
			switch (GetSortTaget(gameObject, out inRetObject))
			{
			case DropType.Sort:
				DeckSort(inRetObject.Transform.gameObject, isDrop);
				break;
			case DropType.Add:
				AddExec(inRetObject, isDrop);
				break;
			default:
				DeckSort(null, isDrop);
				break;
			}
		}
		else
		{
			DeckSort(null, isDrop);
		}
	}

	public void SortAnimeComplete()
	{
		base.gameObject.GetComponent<BoxCollider>().enabled = true;
	}

	protected void MoveReset()
	{
		mParent = _defaultParentTransform;
		base.transform.SetParent(_defaultParentTransform);
		base.transform.localPosition = _defaultPosition;
		base.gameObject.GetComponent<DeckUI>().UpdateUIAlpha(1f);
		base.transform.gameObject.GetComponent<BoxCollider>().enabled = true;
	}

	private void PageChangeUpdate()
	{
		BetterList<UICamera.DepthEntry> hitsList = UICamera.GetHitsList();
		for (int i = 0; i < hitsList.buffer.Length; i++)
		{
			if (!(hitsList.buffer[i].go != null))
			{
				continue;
			}
			if (hitsList.buffer[i].go.name == "LeftDragArea")
			{
				if (_pageChangeIntervalTime > 0.75f)
				{
					DeckListMenuClass.PrevPage();
					_pageChangeIntervalTime = 0f;
				}
				break;
			}
			if (hitsList.buffer[i].go.name == "RightDragArea")
			{
				if (_pageChangeIntervalTime > 0.75f)
				{
					DeckListMenuClass.NextPage();
					_pageChangeIntervalTime = 0f;
				}
				break;
			}
		}
	}

	private bool IsSortDrag()
	{
		if (DeckListMenuClass.IsSortDragging && DeckListMenuClass.SortDragObject == base.gameObject)
		{
			return true;
		}
		return false;
	}

	private void SetDragStatus(bool isDrag)
	{
		if (isDrag)
		{
			DeckListMenuClass.IsSortDragging = true;
			DeckListMenuClass.SortDragObject = base.gameObject;
		}
		else
		{
			DeckListMenuClass.IsSortDragging = false;
			DeckListMenuClass.SortDragObject = null;
		}
	}
}
