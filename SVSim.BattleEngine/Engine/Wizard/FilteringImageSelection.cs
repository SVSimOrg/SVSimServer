using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class FilteringImageSelection : MonoBehaviour
{
	private enum PreloadType
	{
		Both,
		Prev,
		Next
	}

	private class SeriesData
	{
		public int _id;

		public string _name;

		public SeriesData(int id, string name)
		{
			_id = id;
			_name = name;
		}
	}

	public class ItemData
	{
		private enum LoadState
		{
			Unloaded,
			Loading,
			Loaded,
			Canceling
		}

		public readonly string _key;

		public readonly int _series;

		public readonly bool _isSelectable;

		public readonly List<string> _loadTexturePaths;

		public readonly string _fetchTexturePath;

		public readonly bool _isDisplaySprite;

		public readonly string _name;

		public readonly string[] _texts;

		public readonly Func<bool> _isNewFunc;

		public readonly Action _onDisplayAction;

		private LoadState _loadState;

		public int _loadGeneration;

		public bool IsNew { get; private set; }

		public bool IsUnuseTexture { get; private set; }

		public Texture Texture { get; private set; }

		public Action<UITexture, ItemData> TextureSettingCustomize { get; private set; }

		public long? CustomTextureItemId { get; private set; }

		public bool IsFavorite { get; set; }

		public ItemData(string key, int series, bool isSelectable, string loadTexturePath, string fetchTexturePath, bool isDisplaySprite, string name, string[] texts, Func<bool> isNewFunc, Action onDisplayAction, Action<UITexture, ItemData> textureSetting, long? textureSettingItemId, bool isFavorite)
		{
			_key = key;
			_series = series;
			_isSelectable = isSelectable;
			_loadTexturePaths = ((loadTexturePath != null) ? new List<string> { loadTexturePath } : null);
			_fetchTexturePath = fetchTexturePath;
			_isDisplaySprite = isDisplaySprite;
			_name = name;
			_texts = texts;
			_isNewFunc = isNewFunc;
			_onDisplayAction = onDisplayAction;
			IsFavorite = isFavorite;
			UpdateIsNewItem();
			IsUnuseTexture = _loadTexturePaths == null && _fetchTexturePath == null;
			Texture = null;
			TextureSettingCustomize = textureSetting;
			CustomTextureItemId = textureSettingItemId;
		}

		public ItemData(string key, int series, bool isSelectable, List<string> loadTexturePaths, string fetchTexturePath, bool isDisplaySprite, string name, string[] texts, Func<bool> isNewFunc, Action onDisplayAction, Action<UITexture, ItemData> textureSetting, long? textureSettingItemId, bool isFavorite)
		{
			_key = key;
			_series = series;
			_isSelectable = isSelectable;
			_loadTexturePaths = loadTexturePaths;
			_fetchTexturePath = fetchTexturePath;
			_isDisplaySprite = isDisplaySprite;
			_name = name;
			_texts = texts;
			_isNewFunc = isNewFunc;
			_onDisplayAction = onDisplayAction;
			TextureSettingCustomize = textureSetting;
			CustomTextureItemId = textureSettingItemId;
			IsFavorite = isFavorite;
			UpdateIsNewItem();
			IsUnuseTexture = _loadTexturePaths == null && _fetchTexturePath == null;
			Texture = null;
		}

		public bool IsMatchKey(string key)
		{
			return _key == key;
		}

		public void UpdateIsNewItem()
		{
			if (_isNewFunc != null)
			{
				IsNew = _isNewFunc();
			}
		}

		public void OnStartLoad()
		{
			_loadState = LoadState.Loading;
		}

		public void OnEndLoad()
		{
			LoadState loadState = _loadState;
			_loadState = LoadState.Loaded;
			if (loadState == LoadState.Canceling)
			{
				Unload();
			}
			else if (_fetchTexturePath != null)
			{
				Texture = Toolbox.ResourcesManager.LoadObject(_fetchTexturePath) as Texture;
			}
		}

		public void Unload()
		{
			switch (_loadState)
			{
			case LoadState.Loaded:
				_loadState = LoadState.Unloaded;
				Texture = null;
				Toolbox.ResourcesManager.RemoveAssetGroup(_loadTexturePaths);
				break;
			case LoadState.Loading:
				_loadState = LoadState.Canceling;
				break;
			}
		}

		public bool IsLoaded()
		{
			if (_loadState != LoadState.Loaded)
			{
				return IsUnuseTexture;
			}
			return true;
		}

		public bool IsUnloaded()
		{
			return _loadState == LoadState.Unloaded;
		}
	}

	[Header("Objects")]
	[SerializeField]
	protected UITexture _selectedItemTexture;

	[SerializeField]
	protected UILabel _selectedItemNameLabel;

	[SerializeField]
	private UIButton _seriesButton;

	[SerializeField]
	private UILabel _selectedSeriesLabel;

	[SerializeField]
	private UIButton _nextPageButton;

	[SerializeField]
	private UIButton _prevPageButton;

	[SerializeField]
	private UILabel _pageHeadlineLabel;

	[SerializeField]
	private UILabel _pageNumberLabel;

	[SerializeField]
	private FilteringImageSelectionItem _itemTemplate;

	[SerializeField]
	private GameObject _itemParent;

	[SerializeField]
	private GameObject _dragArea;

	[SerializeField]
	private GameObject _noItemText;

	[SerializeField]
	private UIButton _modeFavorite;

	[SerializeField]
	private UIButton _toggleFavorite;

	[SerializeField]
	private UISprite _spriteCollider;

	[Header("Settings")]
	[SerializeField]
	private float _pageAppearMoveDistance = 700f;

	[SerializeField]
	private float _pageDisappearMoveDistance = 700f;

	[SerializeField]
	private float _pageAppearTime = 0.4f;

	[SerializeField]
	private float _pageDisappearTime = 0.15f;

	[SerializeField]
	private int _loadMax = 80;

	[SerializeField]
	private int _itemNumPerPage = 10;

	private List<FilteringImageSelectionItem> _itemList;

	private List<ItemData> _dataList;

	private List<ItemData> _nextDataList = new List<ItemData>();

	private ItemData _selectedItemData;

	private int _pageIndexMax;

	private int _pageIndex;

	private bool _isLoading;

	private int _currentLoadGeneration;

	private bool _canUpdateItems;

	private bool _isPaging;

	private Vector3 _pageCenterPosition = Vector3.zero;

	private TweenPosition _pageMoveTweenPosition;

	private TweenAlpha _pageMoveTweenAlpha;

	private List<SeriesData> _seriesList;

	private int _selectedSeriesIndex = -1;

	private int _selectedSeriesDrumIndex;

	protected UIManager _uiManager;


	private bool _isFavoriteMode;

	private UIButton _dialogCloseButtonCopy;

	private List<ItemData> _favoriteList;

	private bool IsLoading
	{
		get
		{
			return _isLoading;
		}
		set
		{
			if (_isLoading = value)
			{
				_uiManager.createInSceneCenterLoading(notBlack: true);
			}
			else
			{
				_uiManager.closeInSceneCenterLoading();
			}
		}
	}

	public UIButton DialogCloseButton { get; set; }

	protected virtual FavoriteTask.Kind TaskKind { get; }

	protected virtual string SelectionButtonTextId { get; }

	public virtual void Initialize(int itemMax, int seriesMax)
	{
		_uiManager = UIManager.GetInstance();
		_itemList = new List<FilteringImageSelectionItem>(_itemNumPerPage);
		_dataList = new List<ItemData>(_itemNumPerPage);
		for (int i = 0; i < _itemNumPerPage; i++)
		{
			FilteringImageSelectionItem component = NGUITools.AddChild(_itemParent.gameObject, _itemTemplate.gameObject).GetComponent<FilteringImageSelectionItem>();
			_itemList.Add(component);
		}
		_itemTemplate.gameObject.SetActive(value: false);
		_pageMoveTweenPosition = _itemParent.GetComponent<TweenPosition>();
		_pageMoveTweenAlpha = _itemParent.GetComponent<TweenAlpha>();
		_pageCenterPosition = _itemParent.transform.localPosition;
		_seriesList = new List<SeriesData>(seriesMax);
		_pageHeadlineLabel.text = Data.SystemText.Get("Profile_0016");
		SetObjectAsFlicker(_dragArea);
		EventDelegate.Add(_seriesButton.onClick, CreateSeriesSelectionDialog);
		_selectedSeriesLabel.text = Data.SystemText.Get("Card_0186");
		EventDelegate.Add(_nextPageButton.onClick, OnNextPage);
		EventDelegate.Add(_prevPageButton.onClick, OnPreviousPage);
		_isFavoriteMode = false;
		SetMode(_isFavoriteMode);
		if (_modeFavorite != null)
		{
			EventDelegate.Add(_modeFavorite.onClick, delegate
			{
				SetMode(!_isFavoriteMode);
				if (_isFavoriteMode)
				{

					if (_dialogCloseButtonCopy == null)
					{
						_dialogCloseButtonCopy = UnityEngine.Object.Instantiate(DialogCloseButton);
						_dialogCloseButtonCopy.GetComponent<UISprite>().depth = 33;
						_dialogCloseButtonCopy.gameObject.transform.parent = _modeFavorite.transform;
						_dialogCloseButtonCopy.transform.position = DialogCloseButton.transform.position;
						_dialogCloseButtonCopy.transform.localScale = DialogCloseButton.transform.localScale;
					}
				}
				else
				{

				}
			});
		}
		if (!(_toggleFavorite != null))
		{
			return;
		}
		EventDelegate.Add(_toggleFavorite.onClick, delegate
		{
			if (_selectedItemData.IsFavorite)
			{
				_toggleFavorite.normalSprite = "btn_favorite_off";
				_selectedItemData.IsFavorite = false;

			}
			else
			{
				_toggleFavorite.normalSprite = "btn_favorite_on";
				_selectedItemData.IsFavorite = true;

			}
			FilteringImageSelectionItem filteringImageSelectionItem = _itemList.FirstOrDefault((FilteringImageSelectionItem x) => x.Data == _selectedItemData);
			if (filteringImageSelectionItem != null)
			{
				filteringImageSelectionItem.SetFavorite(_selectedItemData.IsFavorite);
			}
		});
	}

	private void SetMode(bool isFavorite)
	{
		_isFavoriteMode = isFavorite;
		if (_spriteCollider != null)
		{
			_spriteCollider.gameObject.SetActive(_isFavoriteMode);
		}
		UILabel componentInChildren = _modeFavorite.GetComponentInChildren<UILabel>();
		if (isFavorite)
		{
			componentInChildren.text = Data.SystemText.Get(SelectionButtonTextId);
		}
		else
		{
			componentInChildren.text = Data.SystemText.Get("Profile_0045");
		}
	}

	public void AddSeries(int seriesId, string seriesName)
	{
		if (_seriesList != null)
		{
			SeriesData item = new SeriesData(seriesId, seriesName);
			_seriesList.Add(item);
		}
	}

	public void AddItem(string key, int series, bool isSelectable, List<string> loadTexturePaths, string fetchTexturePath, bool isDisplaySprite, string name, string[] texts, Func<bool> isNewItemFunc, Action onDisplayAction, Action<UITexture, ItemData> textureSetting = null, long? textureSettingItemId = null, bool isFavorite = false)
	{
		if (_dataList != null)
		{
			ItemData item = new ItemData(key, series, isSelectable, loadTexturePaths, fetchTexturePath, isDisplaySprite, name, texts, isNewItemFunc, onDisplayAction, textureSetting, textureSettingItemId, isFavorite);
			_dataList.Add(item);
		}
	}

	public void SelectItemWithKey(string key)
	{
		if (_dataList != null)
		{
			int num = _dataList.FindIndex((ItemData data) => data.IsMatchKey(key));
			if (num >= 0)
			{
				SetSelectedDisplay(_selectedItemData = _dataList[num]);
			}
		}
	}

	public void SetTargetPage(ItemData targetData)
	{
		int pageIndex = 0;
		if (targetData != null)
		{
			int num = GetFilteringDataList().FindIndex((ItemData data) => data.IsMatchKey(targetData._key));
			if (num >= 0)
			{
				pageIndex = num / _itemNumPerPage;
			}
		}
		_pageIndex = pageIndex;
		SetPage(_pageIndex, PreloadType.Both, delegate
		{
			_canUpdateItems = true;
			_uiManager.StartCoroutine(UpdateItems());
			IsLoading = false;
		});
	}

	public string GetSelectedItemKey()
	{
		if (_selectedItemData == null)
		{
			return string.Empty;
		}
		return _selectedItemData._key;
	}

	public void Open()
	{
		UpdateIsNewItem();
		SetSeries(-1);
		SetTargetPage(_selectedItemData);
		SetMode(isFavorite: false);
	}

	private void OnNextPage()
	{
		Paging(isNext: true);
	}

	private void OnPreviousPage()
	{
		Paging(isNext: false);
	}

	private void Paging(bool isNext)
	{
		if (_isPaging || IsLoading || IsOnlyOnePage())
		{
			return;
		}
		_pageIndex = GetLoopPageIndex(isNext ? (_pageIndex + 1) : (_pageIndex - 1), _pageIndexMax);

		_canUpdateItems = false;
		_isPaging = true;
		StartPageMoveTween(isAppear: false, isNext, delegate
		{
			_uiManager.StartCoroutine(UpdateItems(delegate
			{
				StartPageMoveTween(isAppear: true, isNext, delegate
				{
					_isPaging = false;
				});
			}));
		});
		PreloadType preloadType = ((!isNext) ? PreloadType.Prev : PreloadType.Next);
		SetPage(_pageIndex, preloadType, delegate
		{
			_canUpdateItems = true;
			IsLoading = false;
		});
	}

	private void StartPageMoveTween(bool isAppear, bool isNext, Action onFinish = null)
	{
		if (isAppear)
		{
			Vector3 pageCenterPosition = _pageCenterPosition;
			pageCenterPosition.x += (isNext ? _pageAppearMoveDistance : (0f - _pageAppearMoveDistance));
			_pageMoveTweenPosition.from = pageCenterPosition;
			_pageMoveTweenPosition.to = _pageCenterPosition;
			_pageMoveTweenPosition.duration = _pageAppearTime;
			_pageMoveTweenAlpha.from = 0f;
			_pageMoveTweenAlpha.to = 1f;
		}
		else
		{
			Vector3 pageCenterPosition2 = _pageCenterPosition;
			pageCenterPosition2.x += (isNext ? (0f - _pageDisappearMoveDistance) : _pageDisappearMoveDistance);
			_pageMoveTweenPosition.from = _pageCenterPosition;
			_pageMoveTweenPosition.to = pageCenterPosition2;
			_pageMoveTweenPosition.duration = _pageDisappearTime;
			_pageMoveTweenAlpha.from = 1f;
			_pageMoveTweenAlpha.to = 0f;
		}
		if (onFinish != null)
		{
			_pageMoveTweenPosition.SetOnFinished(new EventDelegate(delegate
			{
				onFinish();
			}));
		}
		_pageMoveTweenPosition.ResetToBeginning();
		_pageMoveTweenPosition.PlayForward();
		_pageMoveTweenAlpha.ResetToBeginning();
		_pageMoveTweenAlpha.PlayForward();
	}

	private void SetPage(int pageIndex, PreloadType preloadType, Action onFinish = null)
	{
		List<ItemData> filteringDataList = GetFilteringDataList();
		_pageIndexMax = (filteringDataList.Count - 1) / _itemNumPerPage;
		_noItemText.SetActive(filteringDataList.Count == 0);
		bool active = !IsOnlyOnePage();
		_nextPageButton.gameObject.SetActive(active);
		_prevPageButton.gameObject.SetActive(active);
		_pageIndex = Mathf.Min(Mathf.Max(pageIndex, 0), _pageIndexMax);
		UpdatePageNumber();
		_nextDataList = filteringDataList.Skip(pageIndex * _itemNumPerPage).Take(_itemNumPerPage).ToList();
		int i = 0;
		for (int count = _nextDataList.Count; i < count; i++)
		{
			_nextDataList[i]._onDisplayAction.Call();
		}
		List<ItemData> list = new List<ItemData>((preloadType == PreloadType.Both) ? (_itemNumPerPage * 2) : _itemNumPerPage);
		if (preloadType == PreloadType.Prev || preloadType == PreloadType.Both)
		{
			int loopPageIndex = GetLoopPageIndex(_pageIndex - 1, _pageIndexMax);
			if (loopPageIndex != _pageIndex)
			{
				list.AddRange(filteringDataList.Skip(loopPageIndex * _itemNumPerPage).Take(_itemNumPerPage));
			}
		}
		if (preloadType == PreloadType.Next || preloadType == PreloadType.Both)
		{
			int loopPageIndex2 = GetLoopPageIndex(_pageIndex + 1, _pageIndexMax);
			if (loopPageIndex2 != _pageIndex)
			{
				list.AddRange(filteringDataList.Skip(loopPageIndex2 * _itemNumPerPage).Take(_itemNumPerPage));
			}
		}
		if (!IsAllTextureLoaded(_nextDataList))
		{
			IsLoading = true;
		}
		LoadTexture(_nextDataList, list, delegate
		{
			onFinish.Call();
		});
		int num = _dataList.Count((ItemData data) => !data.IsUnloaded()) - _loadMax;
		if (num > 0)
		{
			UnloadTexture(num);
		}
	}

	private void LoadTexture(List<ItemData> nextDataList, List<ItemData> exceptPreloadDataList, Action onFinishNext = null)
	{
		if (_currentLoadGeneration == 1024)
		{
			ResetLoadGeneration();
		}
		_currentLoadGeneration++;
		foreach (ItemData nextData in nextDataList)
		{
			nextData._loadGeneration = _currentLoadGeneration;
		}
		foreach (ItemData exceptPreloadData in exceptPreloadDataList)
		{
			exceptPreloadData._loadGeneration = _currentLoadGeneration;
		}
		if (_selectedItemData != null)
		{
			_selectedItemData._loadGeneration = _currentLoadGeneration;
		}
		List<ItemData> loadDataList = nextDataList.Where((ItemData data) => !data.IsLoaded()).ToList();
		int count = loadDataList.Count;
		if (count > 0)
		{
			foreach (ItemData item in loadDataList)
			{
				item.OnStartLoad();
			}
			List<string> rogueAssetList = CollectLoadPaths(loadDataList);
			_uiManager.StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(rogueAssetList, delegate
			{
				foreach (ItemData item2 in loadDataList)
				{
					item2.OnEndLoad();
					if (item2 == _selectedItemData)
					{
						SetSelectedDisplay(item2);
					}
				}
				onFinishNext.Call();
			}));
		}
		List<ItemData> preloadDataList = (from data in exceptPreloadDataList.Except(nextDataList)
			where !data.IsLoaded()
			select data).ToList();
		if (preloadDataList.Count > 0)
		{
			foreach (ItemData item3 in preloadDataList)
			{
				item3.OnStartLoad();
			}
			List<string> rogueAssetList2 = CollectLoadPaths(preloadDataList);
			_uiManager.StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(rogueAssetList2, delegate
			{
				foreach (ItemData item4 in preloadDataList)
				{
					item4.OnEndLoad();
				}
			}));
		}
		if (count == 0)
		{
			onFinishNext.Call();
		}
	}

	private List<string> CollectLoadPaths(List<ItemData> dataList)
	{
		List<string> list = new List<string>(dataList.Sum((ItemData data) => (data._loadTexturePaths != null) ? data._loadTexturePaths.Count : 0));
		foreach (ItemData data in dataList)
		{
			list.AddRange(data._loadTexturePaths);
		}
		return list;
	}

	private void UnloadTexture(int unloadNum)
	{
		foreach (ItemData item in (from data in _dataList
			where !data.IsUnloaded()
			orderby data._loadGeneration
			select data).Take(unloadNum))
		{
			item.Unload();
		}
	}

	private IEnumerator UpdateItems(Action onFinish = null)
	{
		while (!_canUpdateItems)
		{
			yield return null;
		}
		for (int i = 0; i < _itemNumPerPage; i++)
		{
			FilteringImageSelectionItem item = _itemList[i];
			GameObject gameObject = item.gameObject;
			if (i >= _nextDataList.Count)
			{
				gameObject.SetActive(value: false);
				item.SetVisible(isVisible: false);
				continue;
			}
			ItemData data = _nextDataList[i];
			item.SetItemData(data);
			item.SetTexture(data);
			item.SetActiveSelectMark(data == _selectedItemData);
			item.SetActiveNewMark(data.IsNew);
			item.SetFavorite(data.IsFavorite);
			gameObject.SetActive(value: true);
			item.SetVisible(isVisible: true);
			SetObjectAsFlicker(gameObject);
			UIButton component = gameObject.GetComponent<UIButton>();
			component.name = data._key;
			component.onClick.Clear();
			component.onClick.Add(new EventDelegate(delegate
			{
				int setype = 0;
				if (data._isSelectable)
				{
					OnClickSelectableItem(data);
					if (_isFavoriteMode)
					{
						data.IsFavorite = !data.IsFavorite;
						item.SetFavorite(data.IsFavorite);
						if (!data.IsFavorite)
						{
							setype = 0;
						}
					}
					UpdateToggleFavoriteButton(data.IsFavorite);
				}
				else
				{
					OnClickNonSelectableItem(data);
				}

			}));
		}
		if (onFinish != null)
		{
			yield return null;
			onFinish();
		}
	}

	protected virtual void OnClickSelectableItem(ItemData data)
	{
		_selectedItemData = data;
		SetSelectedDisplay(data);
		UpdateSelectMark();
	}

	protected virtual void OnClickNonSelectableItem(ItemData data)
	{
	}

	private void UpdateSelectMark()
	{
		int i = 0;
		for (int count = _itemList.Count; i < count; i++)
		{
			FilteringImageSelectionItem filteringImageSelectionItem = _itemList[i];
			filteringImageSelectionItem.SetActiveSelectMark(filteringImageSelectionItem.Data == _selectedItemData);
		}
	}

	private void UpdateIsNewItem()
	{
		int i = 0;
		for (int count = _dataList.Count; i < count; i++)
		{
			_dataList[i].UpdateIsNewItem();
		}
	}

	protected virtual void SetSelectedDisplay(ItemData data)
	{
		if (_selectedItemTexture != null)
		{
			_selectedItemTexture.mainTexture = data.Texture;
			_selectedItemTexture.enabled = true;
		}
		if (_selectedItemNameLabel != null)
		{
			_selectedItemNameLabel.SetWrapText(data._name);
		}
		UpdateToggleFavoriteButton(data.IsFavorite);
	}

	private void CreateSeriesSelectionDialog()
	{

		List<string> list = new List<string>(_seriesList.Count + 1);
		list.Add(Data.SystemText.Get("Card_0186"));
		int i = 0;
		for (int count = _seriesList.Count; i < count; i++)
		{
			list.Add(_seriesList[i]._name);
		}
		list.Add(Data.SystemText.Get("Card_0180"));
		_selectedSeriesDrumIndex = _selectedSeriesIndex + 1;
		DialogBase dialogBase = DrumrollDialog.Create(list, _selectedSeriesDrumIndex, delegate(int index)
		{
			_selectedSeriesDrumIndex = index;
		});
		dialogBase.SetPanelDepth(500);
		dialogBase.SetPanelSortingOrder(1);
		UIPanel component = dialogBase.InsideObject.GetComponent<UIPanel>();
		component.depth = 501;
		component.sortingOrder = 2;
		UIPanel[] componentsInChildren = dialogBase.InsideObject.GetComponentsInChildren<UIPanel>();
		for (int num = 0; num < componentsInChildren.Length; num++)
		{
			componentsInChildren[num].depth += 502;
			componentsInChildren[num].sortingOrder = 2;
		}
		dialogBase.SetTitleLabel(Data.SystemText.Get("Card_0185"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.DecisionBtn);
		dialogBase.onPushButton1 = delegate
		{
			int num2 = _selectedSeriesDrumIndex - 1;
			if (num2 != _selectedSeriesIndex)
			{
				SetSeries(num2);
				SetTargetPage(null);
			}
		};
	}

	private void SetSeries(int seriesIndex)
	{
		_selectedSeriesIndex = seriesIndex;
		if (seriesIndex == -1)
		{
			_selectedSeriesLabel.text = Data.SystemText.Get("Card_0186");
		}
		else if (seriesIndex < _seriesList.Count)
		{
			_selectedSeriesLabel.text = _seriesList[seriesIndex]._name;
		}
		else
		{
			_selectedSeriesLabel.text = Data.SystemText.Get("Card_0180");
		}
		if (_selectedSeriesIndex >= _seriesList.Count)
		{
			_favoriteList = _dataList.Where((ItemData x) => x.IsFavorite).ToList();
		}
	}

	private List<ItemData> GetFilteringDataList()
	{
		if (_selectedSeriesIndex == -1)
		{
			return _dataList;
		}
		if (_selectedSeriesIndex < _seriesList.Count)
		{
			int selectedSeries = _seriesList[_selectedSeriesIndex]._id;
			return _dataList.Where((ItemData x) => x._series == selectedSeries).ToList();
		}
		return _favoriteList;
	}

	private int GetLoopPageIndex(int pageIndex, int pageIndexMax)
	{
		if (pageIndex > pageIndexMax)
		{
			return 0;
		}
		if (pageIndex < 0)
		{
			return pageIndexMax;
		}
		return pageIndex;
	}

	private void UpdatePageNumber()
	{
		_pageNumberLabel.text = string.Format(Data.SystemText.Get("Profile_0040"), _pageIndex + 1, _pageIndexMax + 1);
	}

	private void SetObjectAsFlicker(GameObject targetObj)
	{
		UIEventListener.Get(targetObj).onDrag = delegate(GameObject obj, Vector2 delta)
		{
			if (delta.x >= 70f)
			{
				OnPreviousPage();
			}
			else if (delta.x <= -70f)
			{
				OnNextPage();
			}
		};
		UIEventListener.Get(targetObj).onScroll = delegate(GameObject obj, float delta)
		{
			if (delta > 0f)
			{
				OnPreviousPage();
			}
			else
			{
				OnNextPage();
			}
		};
	}

	private void ResetLoadGeneration()
	{
		int i = 0;
		for (int count = _dataList.Count; i < count; i++)
		{
			ItemData itemData = _dataList[i];
			if (!itemData.IsUnloaded())
			{
				itemData._loadGeneration = ((itemData._loadGeneration > 0) ? (itemData._loadGeneration - 1024) : (-1024));
			}
		}
		_currentLoadGeneration = 0;
	}

	private bool IsOnlyOnePage()
	{
		return _pageIndexMax == 0;
	}

	private bool IsAllTextureLoaded(List<ItemData> dataList)
	{
		return dataList.Any((ItemData data) => data.IsLoaded());
	}

	protected virtual void UpdateFavoriteFlag(IEnumerable<long> added, IEnumerable<long> removed)
	{
	}

	protected virtual IEnumerable<long> GetFavorites()
	{
		return Enumerable.Empty<long>();
	}

	protected void UpdateToggleFavoriteButton(bool b)
	{
		if (_toggleFavorite != null)
		{
			if (b)
			{
				_toggleFavorite.normalSprite = "btn_favorite_on";
			}
			else
			{
				_toggleFavorite.normalSprite = "btn_favorite_off";
			}
		}
	}
}
