using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;

public class DeckListMenuUI : MonoBehaviour
{
	private enum EditMode
	{
		Default,
		DeckSort,
		MultiDelete
	}

	public class PageData
	{
		public readonly DeckData[] DeckList;

		public readonly GameObject Obj;

		public PageData(DeckData[] deckArray, GameObject obj)
		{
			DeckList = deckArray;
			Obj = obj;
		}
	}

	public enum eEditState
	{
		CanEdit,
		DeleteOnly,
		Lock
	}

	[SerializeField]
	private DeckUI _deckFrameOriginal;

	[SerializeField]
	private UIGrid _deckTableOriginal;

	[SerializeField]
	private UISprite _radioIconOriginal;

	[SerializeField]
	private GameObject _deckTableRoot;

	[SerializeField]
	private UIGrid _radioIconsGrid;

	[SerializeField]
	private UILabel _pageLabel;

	[SerializeField]
	private UIButton _leftButton;

	[SerializeField]
	private UIButton _rightButton;

	[SerializeField]
	private BoxCollider _flickCollider;

	private List<UISprite> _radioIconClones;

	private List<PageData> _pageList = new List<PageData>();

	private int _currentPage;

	private List<string> _resourcePathList;

	private Coroutine _loadCoroutine;

	private bool _isChangePage;

	private List<DeckUI> _deckUIList = new List<DeckUI>();

	private Action _onMultiDeckDelete;

	private Action<DeckData> _onLongTapMultiDeckDelete;

	private Func<List<int>, BaseTask> _funcCreateDeckDeleteTask;

	private Func<List<int>, BaseTask> _funcCreateSaveDeckOrderTask;

	private bool _isOnDestroy;

	[SerializeField]
	private UIButton _deckSortStartButton;

	[SerializeField]
	private UIButton _deckSortSaveButton;

	[SerializeField]
	private UIButton _deckSortCancelButton;

	[SerializeField]
	private GameObject _deckSortButtonBase;

	[SerializeField]
	private GameObject _multiDeckDeleteMenuRoot;

	[SerializeField]
	private UIButton _multiDeckDeleteStartButton;

	[SerializeField]
	private UIButton _multiDeckDeleteCancelButton;

	[SerializeField]
	private UIButton _multiDeckDeleteDecideButton;

	[SerializeField]
	private GameObject _deckSortBlackBg;

	private EditMode _editMode;

	private DeckUI _newCreateObject;

	private bool _enableDeckSortDeckCount;

	private bool _initializeEnd;

	private List<List<DeckFrame>> _deckFrameDefaultList = new List<List<DeckFrame>>();

	private List<List<DeckFrame>> _deckFrameListTempSort = new List<List<DeckFrame>>();

	private DeckGroup _deckGroup;

	private bool _isVisibleCreateNewButton;

	private bool _enableFirstViewLastUseDeck = true;

	public eEditState EditState { get; private set; }

	public bool EnableDrag { get; set; } = true;

	public bool IsSortMode => _editMode == EditMode.DeckSort;

	private bool _isSortUse => EditState != eEditState.Lock;

	public bool IsSortDragging { get; set; }

	public bool IsPlayingSortAnimation { get; private set; }

	public GameObject SortDragObject { get; set; }

	public List<UIGrid> DeckPageList { get; private set; } = new List<UIGrid>();

	public event Action<DeckData> OnSelectDeck;

	private void Awake()
	{
		_radioIconClones = new List<UISprite>();
		DeckSortInit();
	}

	public void Initialize(DeckGroup deckGroup, eEditState editState, Action<DeckData> onSelectDeck, Action onMultiDeckDelete, Action<DeckData> onLongPressMultiDeckDelete, Func<List<int>, BaseTask> funcCreateDeckDeleteTask, Func<List<int>, BaseTask> funcCreateSaveDeckOrderTask, bool isVisibleCreateNewButton, bool enableFirstViewLastUseDeck, Action onFinish)
	{
		EditState = editState;
		OnSelectDeck += onSelectDeck;
		_onMultiDeckDelete = onMultiDeckDelete;
		_onLongTapMultiDeckDelete = onLongPressMultiDeckDelete;
		_funcCreateDeckDeleteTask = funcCreateDeckDeleteTask;
		_funcCreateSaveDeckOrderTask = funcCreateSaveDeckOrderTask;
		_isVisibleCreateNewButton = isVisibleCreateNewButton;
		_enableFirstViewLastUseDeck = enableFirstViewLastUseDeck;
		_rightButton.gameObject.SetActive(value: false);
		_leftButton.gameObject.SetActive(value: false);
		UIEventListener uIEventListener = UIEventListener.Get(_flickCollider.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPanel));
		UIEventListener uIEventListener2 = UIEventListener.Get(_rightButton.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			NextPage();
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(_leftButton.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
		{
			PrevPage();
		});
		_deckSortStartButton.onClick.Add(new EventDelegate(delegate
		{
			OnClickDeckSortStart();
		}));
		_deckSortSaveButton.onClick.Add(new EventDelegate(delegate
		{
			OnClickDeckSortSave();
		}));
		_deckSortCancelButton.onClick.Add(new EventDelegate(delegate
		{
			OnClickDeckSortCancel();
		}));
		UIEventListener uIEventListener4 = UIEventListener.Get(_multiDeckDeleteStartButton.gameObject);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnClickMultiDeckDeleteStartButton();
		});
		UIEventListener uIEventListener5 = UIEventListener.Get(_multiDeckDeleteCancelButton.gameObject);
		uIEventListener5.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener5.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnClickMultiDeckDeleteCancelButton();
		});
		UIEventListener uIEventListener6 = UIEventListener.Get(_multiDeckDeleteDecideButton.gameObject);
		uIEventListener6.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener6.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnClickMultiDeckDeleteDecideButton();
		});
		UpdateDeckList(deckGroup, delegate
		{
			_initializeEnd = true;
			onFinish.Call();
		});
	}

	public void UpdateDeckList(DeckGroup deckGroup, Action onFinish)
	{
		if (deckGroup != null)
		{
			if (_initializeEnd)
			{
				Delete();
			}
			_deckGroup = deckGroup;
			_loadCoroutine = UIManager.GetInstance().StartCoroutine(LoadResourceCoroutine(delegate
			{
				SetupEditableDeckList();
				onFinish.Call();
			}));
		}
	}

	private IEnumerator LoadResourceCoroutine(Action onFinish)
	{
		_resourcePathList = new List<string>();
		List<int> list = new List<int>();
		List<long> list2 = new List<long>();
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		for (int i = 1; i < 9; i++)
		{
			int skin_id = dataMgr.GetCharaPrmByClassId(i).skin_id;
			if (!list.Contains(skin_id))
			{
				list.Add(skin_id);
				_resourcePathList.Add(Toolbox.ResourcesManager.GetAssetTypePath(skin_id.ToString(), ResourcesManager.AssetLoadPathType.DeckListTexture));
			}
		}
		int num = 3000011;
		if (!list2.Contains(num))
		{
			list2.Add(num);
			_resourcePathList.Add(Toolbox.ResourcesManager.GetAssetTypePath(num.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture));
		}
		foreach (DeckData deckData in _deckGroup.DeckDataList)
		{
			LoadSkinSleeve(deckData, list, list2);
		}
		yield return UIManager.GetInstance().StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(_resourcePathList, null));
		_loadCoroutine = null;
		if (!_isOnDestroy)
		{
			onFinish.Call();
		}
	}

	private void LoadSkinSleeve(DeckData deck, List<int> skinIdList, List<long> sleeveIdList)
	{
		if (deck.IsNoCard())
		{
			return;
		}
		int skinId = deck.GetSkinId();
		if (!skinIdList.Contains(skinId))
		{
			skinIdList.Add(skinId);
			_resourcePathList.Add(Toolbox.ResourcesManager.GetAssetTypePath(skinId.ToString(), ResourcesManager.AssetLoadPathType.DeckListTexture));
		}
		long existingSleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(deck.GetDeckSleeveID());
		Sleeve sleeve = Data.Master.SleeveMgr.Get(existingSleeveId);
		if (!sleeveIdList.Contains(existingSleeveId))
		{
			sleeveIdList.Add(existingSleeveId);
			_resourcePathList.Add(Toolbox.ResourcesManager.GetAssetTypePath(existingSleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture));
			if (sleeve.IsPremiumSleeve)
			{
				UIManager.GetInstance().getUIBase_CardManager().AddPremireSleevePath(ref _resourcePathList, sleeve);
			}
		}
	}

	private void SetupEditableDeckList()
	{
		SetupDeckList();
		foreach (List<DeckFrame> deckFrameDefault in _deckFrameDefaultList)
		{
			foreach (DeckFrame item in deckFrameDefault)
			{
				item.Transform.gameObject.AddMissingComponent<DeckSortDragDrop>().DeckListMenuClass = this;
			}
		}
	}

	private void SetupDeckList()
	{
		int newPage = 0;
		_deckUIList.Clear();
		AddCustomDeckTable(_deckGroup.DeckDataList);
		if (_enableFirstViewLastUseDeck && _deckGroup.DeckDataList.Any((DeckData deck) => deck.IsUsable()))
		{
			DeckData firstDisplayDeck = GetFirstDisplayDeck();
			if (firstDisplayDeck != null)
			{
				for (int num = 0; num < _pageList.Count; num++)
				{
					DeckData[] deckList = _pageList[num].DeckList;
					for (int num2 = 0; num2 < deckList.Length; num2++)
					{
						if (deckList[num2] == firstDisplayDeck)
						{
							newPage = num;
							break;
						}
					}
				}
			}
		}
		ChangePage(newPage, isImmediate: true);
		SetDefaultMode();
	}

	private DeckData GetFirstDisplayDeck()
	{
		int deckId = 0;
		switch (_deckGroup.DeckFormat)
		{
		case Format.Rotation:
			deckId = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_ROTATION);
			break;
		case Format.Unlimited:
			deckId = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_UNLIMITED);
			break;
		case Format.PreRotation:
			deckId = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_PRE_ROTATION);
			break;
		case Format.Crossover:
			deckId = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_CROSSOVER);
			break;
		case Format.MyRotation:
			deckId = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_MY_ROTATION);
			break;
		case Format.Avatar:
			deckId = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_AVATAR);
			break;
		}
		DeckData deckData = _deckGroup.DeckDataList.FirstOrDefault((DeckData deck) => deck.GetDeckID() == deckId && !deck.IsNoCard());
		if (deckData != null)
		{
			return deckData;
		}
		deckData = _deckGroup.DeckDataList.FirstOrDefault((DeckData deck) => deck.IsUsable());
		if (deckData != null)
		{
			return deckData;
		}
		return _deckGroup.DeckDataList.FirstOrDefault();
	}

	private void AddCustomDeckTable(List<DeckData> deckList)
	{
		int num = 0;
		List<DeckUI.DeckViewData> list = new List<DeckUI.DeckViewData>();
		deckList.FindLastIndex((DeckData d) => !d.IsNoCard());
		int num2 = 0;
		List<DeckUI.DeckViewData> list2 = DeckUI.DeckViewData.CreateDeckViewList(deckList, _isVisibleCreateNewButton);
		for (int num3 = 0; num3 < deckList.Count; num3++)
		{
			list.Add(list2[num3]);
			if (!deckList[num3].IsNoCard())
			{
				num2++;
			}
			if (list.Count == 9 || num3 + 1 == deckList.Count)
			{
				num++;
				AddDeckTable(list);
				list.Clear();
			}
		}
		if (_isSortUse)
		{
			_enableDeckSortDeckCount = num2 >= 2;
		}
	}

	private void AddDeckTable(List<DeckUI.DeckViewData> deckViewList)
	{
		UIGrid uIGrid = UnityEngine.Object.Instantiate(_deckTableOriginal);
		uIGrid.transform.parent = _deckTableRoot.transform;
		uIGrid.transform.localScale = Vector3.one;
		int num = 0;
		uIGrid.sorting = UIGrid.Sorting.Custom;
		uIGrid.onCustomSort = SortUiGridCustom;
		DeckPageList.Add(uIGrid);
		_deckFrameListTempSort.Add(new List<DeckFrame>());
		_deckFrameDefaultList.Add(new List<DeckFrame>());
		for (int i = 0; i < _deckFrameListTempSort.Count; i++)
		{
			num += _deckFrameListTempSort[i].Count;
		}
		for (int j = 0; j < deckViewList.Count; j++)
		{
			DeckData deck = deckViewList[j].Deck;
			Transform transform = CreateDeckFrame(deckViewList[j]);
			uIGrid.AddChild(transform);
			transform.localScale = Vector3.one;
			transform.gameObject.name = (num + j).ToString();
			if (!deck.IsNoCard())
			{
				DeckFrame deckFrame = new DeckFrame();
				deckFrame.Transform = transform;
				deckFrame.DeckId = deck.GetDeckID();
				_deckFrameListTempSort[_deckFrameListTempSort.Count - 1].Add(deckFrame);
				_deckFrameDefaultList[_deckFrameListTempSort.Count - 1].Add(deckFrame);
			}
		}
		UISprite uISprite = UnityEngine.Object.Instantiate(_radioIconOriginal);
		_radioIconClones.Add(uISprite);
		_radioIconsGrid.AddChild(uISprite.transform);
		uISprite.transform.localScale = Vector3.one;
		PageData item = new PageData(deckViewList.Select((DeckUI.DeckViewData deckView) => deckView.Deck).ToArray(), uIGrid.gameObject);
		_pageList.Add(item);
	}

	public static int SortUiGridCustom(Transform a, Transform b)
	{
		if (int.TryParse(a.name, out var result) && int.TryParse(b.name, out var result2))
		{
			if (result > result2)
			{
				return 1;
			}
			if (result < result2)
			{
				return -1;
			}
		}
		return 0;
	}

	public bool DeckSort(string inTargetObjectName, string inSortObjectName, ref Vector3 sortObjectPosition)
	{
		if (inTargetObjectName == inSortObjectName + 1)
		{
			return false;
		}
		if (_deckFrameListTempSort[0].Count < 2)
		{
			return false;
		}
		if (inTargetObjectName == inSortObjectName)
		{
			return false;
		}
		int num = int.Parse(inTargetObjectName);
		int num2 = int.Parse(inSortObjectName) / 9;
		int num3 = num / 9;
		if (num2 < num3 && num % 9 == 0 && _currentPage != num2)
		{
			return false;
		}
		DeckFrame deckFrame = null;
		for (int i = 0; i < _deckFrameListTempSort.Count; i++)
		{
			deckFrame = _deckFrameListTempSort[i].FindLast((DeckFrame a) => inSortObjectName == a.Transform.name);
			if (deckFrame != null)
			{
				break;
			}
		}
		for (int num4 = 0; num4 < _deckFrameListTempSort.Count && _deckFrameListTempSort[num4].FindLast((DeckFrame a) => inTargetObjectName == a.Transform.name) == null; num4++)
		{
		}
		List<DeckFrame> list = new List<DeckFrame>();
		for (int num5 = 0; num5 < _deckFrameListTempSort.Count; num5++)
		{
			list.AddRange(_deckFrameListTempSort[num5]);
		}
		list.Remove(deckFrame);
		int index = list.FindIndex((DeckFrame a) => inTargetObjectName == a.Transform.name);
		list.Insert(index, deckFrame);
		sortObjectPosition = CreateSortedDeckPage(list, deckFrame, inIsMoveAnimation: true);
		return true;
	}

	public void DeckSortAddLast(DeckFrame inAddObject)
	{
		List<DeckFrame> list = new List<DeckFrame>();
		for (int i = 0; i < _deckFrameListTempSort.Count; i++)
		{
			list.AddRange(_deckFrameListTempSort[i]);
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].DeckId == inAddObject.DeckId)
			{
				list.RemoveAt(j);
				break;
			}
		}
		list.Add(inAddObject);
		CreateSortedDeckPage(list, inAddObject, inIsMoveAnimation: true);
	}

	private Vector3 CreateSortedDeckPage(List<DeckFrame> inSortedAllList, DeckFrame inSortObject, bool inIsMoveAnimation)
	{
		List<List<DeckFrame>> list = new List<List<DeckFrame>>();
		List<Vector3> list2 = new List<Vector3>();
		for (int i = 0; i <= inSortedAllList.Count / 9; i++)
		{
			if (i < DeckPageList.Count)
			{
				list.Add(new List<DeckFrame>());
				list[i].AddRange(inSortedAllList.GetRange(i * 9, _deckFrameListTempSort[i].Count));
			}
		}
		int num = 0;
		for (int j = 0; j < list.Count; j++)
		{
			for (int k = 0; k < list[j].Count; k++)
			{
				list[j][k].Transform.name = num.ToString();
				list[j][k].Transform.SetParent(DeckPageList[j].transform);
				num++;
				list2.Add(list[j][k].Transform.localPosition);
			}
		}
		for (int l = 0; l < DeckPageList.Count; l++)
		{
			DeckPageList[l].Reposition();
		}
		Vector3 result = Vector3.zero;
		if (inSortObject != null)
		{
			result = inSortObject.Transform.localPosition;
		}
		if (inIsMoveAnimation)
		{
			int num2 = 0;
			for (int m = 0; m < list.Count; m++)
			{
				for (int n = 0; n < list[m].Count; n++)
				{
					if (inSortObject != list[m][n] && list[m][n].Transform.localPosition != list2[num2])
					{
						if (!list[m][n].Transform.gameObject.activeInHierarchy)
						{
							list[m][n].TweenTargetPosition = list[m][n].Transform.localPosition;
							list[m][n].Transform.localPosition = list[m][n].TweenTargetPosition;
						}
						else
						{
							list[m][n].TweenTargetPosition = list[m][n].Transform.localPosition;
							UITweenPosition uITweenPosition = list[m][n].Transform.gameObject.AddMissingComponent<UITweenPosition>();
							IsPlayingSortAnimation = true;
							uITweenPosition.OnFinishCallBack = delegate(UITweenPosition fadeObject)
							{
								fadeObject.GetComponent<DeckSortDragDrop>().SortAnimeComplete();
								IsPlayingSortAnimation = false;
							};
							uITweenPosition.From = list2[num2];
							uITweenPosition.To = list[m][n].TweenTargetPosition;
							uITweenPosition.EndTime = 0.2f;
							uITweenPosition.PlayForward(resetFlag: true);
							list[m][n].Transform.gameObject.GetComponent<BoxCollider>().enabled = false;
						}
					}
					num2++;
				}
			}
		}
		_deckFrameListTempSort = list;
		return result;
	}

	public int GetDeckNoFromGameObject(GameObject inObject)
	{
		for (int i = 0; i < _deckFrameListTempSort.Count; i++)
		{
			for (int j = 0; j < _deckFrameListTempSort[i].Count; j++)
			{
				if (_deckFrameListTempSort[i][j].Transform.gameObject == inObject)
				{
					return _deckFrameListTempSort[i][j].DeckId;
				}
			}
		}
		return 0;
	}

	public bool ChangePage(int newPage, bool isImmediate = false)
	{
		int currentPage = _currentPage;
		if (_isChangePage)
		{
			return false;
		}
		int count = _radioIconClones.Count;
		bool flag = false;
		if (currentPage == newPage)
		{
			isImmediate = true;
		}
		if (!IsValidPage(newPage))
		{
			return false;
		}
		_currentPage = newPage;
		foreach (PageData page in _pageList)
		{
			page.Obj.SetActive(value: false);
		}
		_pageList[_currentPage].Obj.SetActive(value: true);
		_pageList[currentPage].Obj.SetActive(value: true);
		float num = ((_currentPage < currentPage) ? 1400f : (-1400f));
		if (flag)
		{
			num = 0f - num;
		}
		Vector3 pos = new Vector3(num, 0f, 0f);
		Vector3 localPosition = new Vector3(0f - num, 0f, 0f);
		Vector3 zero = Vector3.zero;
		_pageList[_currentPage].Obj.transform.localPosition = localPosition;
		TweenPosition.Begin(_pageList[currentPage].Obj, isImmediate ? 0f : 0.2f, pos);
		TweenPosition.Begin(_pageList[_currentPage].Obj, isImmediate ? 0f : 0.2f, zero);
		ClearLongPress();
		if (!isImmediate)
		{
			_isChangePage = true;
		}
		for (int i = 0; i < count; i++)
		{
			if (i == newPage)
			{
				_radioIconClones[i].spriteName = _radioIconClones[i].spriteName.Replace("_off", "_on");
			}
			else
			{
				_radioIconClones[i].spriteName = _radioIconClones[i].spriteName.Replace("_on", "_off");
			}
		}
		bool flag2 = _pageList.Count >= 11;
		bool flag3 = IsValidPage(newPage + 1);
		bool flag4 = IsValidPage(newPage - 1);
		bool active = (flag3 || flag4) && !flag2;
		_rightButton.gameObject.SetActive(flag3);
		_leftButton.gameObject.SetActive(flag4);
		_radioIconsGrid.gameObject.SetActive(active);
		_pageLabel.gameObject.SetActive(flag2);
		_pageLabel.text = Data.SystemText.Get("Card_0053", (newPage + 1).ToString(), _pageList.Count.ToString());
		return true;
	}

	private void Delete()
	{
		if (_loadCoroutine != null)
		{
			StopCoroutine(_loadCoroutine);
			_loadCoroutine = null;
		}
		_radioIconClones.Clear();
		_pageList.Clear();
		Transform[] componentsInChildren = _deckTableRoot.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (_deckTableRoot.transform != componentsInChildren[i])
			{
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
		componentsInChildren = _radioIconsGrid.GetComponentsInChildren<Transform>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (_radioIconsGrid.transform != componentsInChildren[j])
			{
				UnityEngine.Object.Destroy(componentsInChildren[j].gameObject);
			}
		}
		DeckPageList.Clear();
		_deckFrameDefaultList.Clear();
		_deckFrameListTempSort.Clear();
		DeleteResource();
	}

	private void DeleteResource()
	{
		if (_resourcePathList != null)
		{
			Toolbox.ResourcesManager.RemoveAssetGroup(_resourcePathList);
			_resourcePathList.Clear();
		}
	}

	private bool IsValidPage(int page)
	{
		int count = _radioIconClones.Count;
		if (count > 0 && page >= 0)
		{
			return page < count;
		}
		return false;
	}

	private Transform CreateDeckFrame(DeckUI.DeckViewData deckViewData)
	{
		DeckUI deckUI = UnityEngine.Object.Instantiate(_deckFrameOriginal);
		deckUI.Initialize(OnClickDeck, OnLongPress);
		deckUI.UpdateView(deckViewData);
		if (deckViewData.ViewType == DeckUI.eViewType.CreateNew)
		{
			_newCreateObject = deckUI;
		}
		UIEventListener uIEventListener = UIEventListener.Get(deckUI.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPanel));
		_deckUIList.Add(deckUI);
		return deckUI.transform;
	}

	public void NextPage()
	{
		if (ChangePage(_currentPage + 1))
		{

		}
	}

	public void PrevPage()
	{
		if (ChangePage(_currentPage - 1))
		{

		}
	}

	private void OnDragPanel(GameObject obj, Vector2 dir)
	{
		if (EnableDrag && !IsSortDragging)
		{
			if (dir.x >= 70f)
			{
				PrevPage();
			}
			else if (dir.x <= -70f)
			{
				NextPage();
			}
		}
	}

	private void OnClickDeck(DeckUI deckDisplay)
	{
		if (UIManager.GetInstance().IsTouchable)
		{
			switch (_editMode)
			{
			case EditMode.Default:

				this.OnSelectDeck.Call(deckDisplay.Deck);
				break;
			case EditMode.MultiDelete:
				OnClickDeckForMultiDeckDelete(deckDisplay);
				break;
			case EditMode.DeckSort:
				break;
			}
		}
	}

	private void DeckSortInit()
	{
		SetDefaultMode();
		_deckSortStartButton.gameObject.SetActive(value: false);
	}

	private void OnClickDeckSortStart()
	{

		SetSortMode();
	}

	private void OnClickDeckSortCancel()
	{
		if (!IsSortDragging && !IsPlayingSortAnimation)
		{

			SetDefaultMode();
			List<DeckFrame> list = new List<DeckFrame>();
			for (int i = 0; i < _deckFrameDefaultList.Count; i++)
			{
				list.AddRange(_deckFrameDefaultList[i]);
			}
			CreateSortedDeckPage(list, null, inIsMoveAnimation: false);
			ClearBackButtonAction();
		}
	}

	private void OnClickDeckSortSave()
	{
		if (!IsSortDragging && !IsPlayingSortAnimation)
		{

			SaveDeckOrder(delegate
			{
				SetDefaultMode();
				ClearBackButtonAction();
			});
		}
	}

	private void SaveDeckOrder(Action onSuccess)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _deckFrameListTempSort.Count; i++)
		{
			for (int j = 0; j < _deckFrameListTempSort[i].Count; j++)
			{
				list.Add(_deckFrameListTempSort[i][j].DeckId);
			}
		}
		BaseTask task = _funcCreateSaveDeckOrderTask.Call(list);
		StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			_deckFrameDefaultList.Clear();
			_deckFrameDefaultList.AddRange(_deckFrameListTempSort);
			onSuccess.Call();
		}));
	}

	private void DeckCreateEmptyChange(bool isEmpty)
	{
		if (!(_newCreateObject == null) && (_newCreateObject.ViewType == DeckUI.eViewType.CreateNew || _newCreateObject.ViewType == DeckUI.eViewType.Empty))
		{
			_newCreateObject.UpdateView(_newCreateObject.Deck, isEmpty ? DeckUI.eViewType.Empty : DeckUI.eViewType.CreateNew);
		}
	}

	public DeckData GetDeckDataSamePage(DeckData targetDeck)
	{
		foreach (PageData page in _pageList)
		{
			bool flag = false;
			DeckData[] deckList = page.DeckList;
			foreach (DeckData deckData in deckList)
			{
				if (deckData.Format == targetDeck.Format && !deckData.IsNoCard() && deckData.GetDeckID() == targetDeck.GetDeckID())
				{
					flag = true;
				}
			}
			if (!flag)
			{
				continue;
			}
			deckList = page.DeckList;
			foreach (DeckData deckData2 in deckList)
			{
				if (!deckData2.IsNoCard() && deckData2.GetDeckID() != targetDeck.GetDeckID())
				{
					return deckData2;
				}
			}
		}
		return null;
	}

	private void OnClickMultiDeckDeleteStartButton()
	{

		SaveDeckOrder(delegate
		{
			SetMultiDeleteMode();
		});
	}

	private void OnClickDeckForMultiDeckDelete(DeckUI deckDisplay)
	{

		deckDisplay.SetVisibleCheckMark(!deckDisplay.IsCheckeMark);
		deckDisplay.SetColorToGrey(deckDisplay.IsCheckeMark);
		RefreshMultiDeleteDecideButtonEnable();
	}

	private void RefreshMultiDeleteDecideButtonEnable()
	{
		bool flag = GetMultiDeleteCount() > 0;
		UIManager.SetObjectToGrey(_multiDeckDeleteDecideButton.gameObject, !flag);
	}

	private void SetDefaultMode()
	{
		_editMode = EditMode.Default;
		IsSortDragging = false;
		DeckCreateEmptyChange(isEmpty: false);
		_deckSortButtonBase.SetActive(value: false);
		_deckSortBlackBg.SetActive(value: false);
		_multiDeckDeleteMenuRoot.SetActive(value: false);
		_multiDeckDeleteStartButton.gameObject.SetActive(value: false);
		_deckSortStartButton.gameObject.SetActive(_isSortUse && _enableDeckSortDeckCount);
		SetSelectableDeckAll(isSelectable: true);
		ClearAllCheckMark();
		ClearLongPress();
	}

	private void SetSortMode()
	{
		_editMode = EditMode.DeckSort;
		IsSortDragging = false;
		_deckSortButtonBase.SetActive(value: true);
		_deckSortBlackBg.SetActive(value: true);
		_multiDeckDeleteMenuRoot.SetActive(value: false);
		_multiDeckDeleteStartButton.gameObject.SetActive(value: true);
		DeckCreateEmptyChange(isEmpty: true);
		_deckSortStartButton.gameObject.SetActive(value: false);
		SetSelectableDeckAll(isSelectable: false);
		ClearAllCheckMark();
		ClearLongPress();
	}

	private void SetMultiDeleteMode()
	{
		_editMode = EditMode.MultiDelete;
		_deckSortButtonBase.SetActive(value: false);
		_multiDeckDeleteMenuRoot.SetActive(value: true);
		_multiDeckDeleteStartButton.gameObject.SetActive(value: false);
		SetBackButtonForMultiDeleteMode();
		SetSelectableDeckAll(isSelectable: true);
		ClearAllCheckMark();
		ClearLongPress();
		RefreshMultiDeleteDecideButtonEnable();
	}

	private void SetBackButtonForMultiDeleteMode()
	{
	}

	private void ClearBackButtonAction()
	{
	}

	private void ClearAllCheckMark()
	{
		foreach (DeckUI deckUI in _deckUIList)
		{
			deckUI.SetVisibleCheckMark(isVisible: false);
			deckUI.SetColorToGrey(isGrey: false);
		}
	}

	private void SetSelectableDeckAll(bool isSelectable)
	{
		foreach (DeckUI deckUI in _deckUIList)
		{
			deckUI.SetSelectable(isSelectable);
		}
	}

	private int GetMultiDeleteCount()
	{
		int num = 0;
		foreach (DeckUI deckUI in _deckUIList)
		{
			if (deckUI.IsCheckeMark)
			{
				num++;
			}
		}
		return num;
	}

	private void ClearLongPress()
	{
		foreach (DeckUI deckUI in _deckUIList)
		{
			deckUI.ClearLongPress();
		}
	}

	private void OnClickMultiDeckDeleteCancelButton()
	{

		SetSortMode();
	}

	private void OnClickMultiDeckDeleteDecideButton()
	{

		ClearBackButtonAction();
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_001_Title"));
		dialogBase.SetText(systemText.Get("Card_0218"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.RedBtn_CancelBtn);
		dialogBase.SetButtonText(systemText.Get("Card_0104"));
		dialogBase.onPushButton1 = delegate
		{
			MultiDeckDelete();
		};
		dialogBase.OnCloseStart = delegate
		{
			SetBackButtonForMultiDeleteMode();
		};
	}

	private void MultiDeckDelete()
	{
		List<int> list = new List<int>();
		foreach (DeckUI deckUI in _deckUIList)
		{
			if (deckUI.IsCheckeMark)
			{
				list.Add(deckUI.Deck.GetDeckID());
			}
		}
		BaseTask task = _funcCreateDeckDeleteTask.Call(list);
		StartCoroutine(Toolbox.NetworkManager.Connect(task, OnSuccessDeckDelete));
	}

	private void OnLongPress(DeckUI deckDisplay)
	{
		if (_editMode == EditMode.MultiDelete)
		{
			_onLongTapMultiDeckDelete.Call(deckDisplay.Deck);
		}
	}

	private void OnSuccessDeckDelete(NetworkTask.ResultCode code)
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_002_Title"));
		dialogBase.SetText(systemText.Get("Card_0010"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		_onMultiDeckDelete.Call();
		ClearBackButtonAction();
	}
}
