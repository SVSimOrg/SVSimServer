using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public abstract class CardSelectListUIBase : MecanimSceneBase
{
	private enum SlideType
	{
		None,
		Next,
		Prev
	}

	[SerializeField]
	protected MecanimStateBase _stateDefaultView;

	[SerializeField]
	protected CardSelectListUI_State_Edit _stateEdit;

	[SerializeField]
	protected CardSelectListUI_State_CardDrag _stateCardDrag;

	[SerializeField]
	private UIButton m_nextPageBtn;

	[SerializeField]
	private UIButton m_prevPageBtn;

	[SerializeField]
	private UISprite m_nextPageBtnGrayout;

	[SerializeField]
	private UISprite m_prevPageBtnGrayout;

	private SlideType _slideType;

	private bool _isSlideEnd = true;

	public Action<bool> OnChangeSlideEnd;

	protected CardBundleControllerBase _cardBundle;

	[SerializeField]
	protected Transform _parentSelectionObj;

	[SerializeField]
	protected Transform m_parentPagingObj;

	[SerializeField]
	protected SimpleCardDetail m_simpleDetailPrefab;

	protected SimpleCardDetail _simpleDetail;

	[SerializeField]
	private UILabel m_labelPageInfo;

	[SerializeField]
	protected UITexture m_sleeveOriginal;

	[SerializeField]
	protected GameObject m_cardInfoOriginal;

	[SerializeField]
	private FilterController _prefabFilter;

	protected FilterController _selectCardFilter;

	protected FilterController _pagingFilter;

	[SerializeField]
	protected UIInputWizard m_searchInput;

	[SerializeField]
	protected UIButton m_searchCancelButton;

	[SerializeField]
	private UILabel m_noEditCardLabel;

	[SerializeField]
	protected GameObject m_PagingDragPanel;

	[SerializeField]
	protected UILabel _redetherNum;

	[SerializeField]
	private UIScrollView _scrollView;

	private bool _isLoading;

	protected bool _isDisableTouchWhileLoading;

	protected List<string> _resourcePathList;

	protected bool _enableSelectSameKindCardNum = true;

	protected bool _isSelectableSpotCard;

	private GameObject _detailTargetObj;

	private MyRotationInfo _myRotationInfoBeforeInitialize;

	private FilterController.MyRotationFilterType _filterType = FilterController.MyRotationFilterType.CARD_POOL_SELECT_ONLY;

	private MyRotationInfo _myRotationInfo;

	protected bool IsSlideEnd
	{
		get
		{
			return _isSlideEnd;
		}
		set
		{
			if (value != _isSlideEnd)
			{
				_isSlideEnd = value;
				OnChangeSlideEnd.Call(value);
			}
		}
	}

	public FilteringCardBundle SelectionAreaList => _cardBundle.SelectionAreaList;

	public CardBundle PagingList => _cardBundle.PagingList;

	public bool IsSetup { get; protected set; }

	public Format Format { get; protected set; }

	public bool IsLoading
	{
		get
		{
			return _isLoading;
		}
		protected set
		{
			if (_isLoading = value && !IsSetup)
			{
				UIManager.GetInstance().createInSceneCenterLoading(notBlack: true, !_isDisableTouchWhileLoading);
			}
			else
			{
				UIManager.GetInstance().closeInSceneCenterLoading();
			}
		}
	}

	protected bool IsShowCardDetailCraftPanel { get; set; } = true;

	public IFormatBehavior FormatBehavior { get; private set; }

	protected FilterController.MyRotationFilterType MyRotationFilterTypeCardPool
	{
		get
		{
			return _filterType;
		}
		set
		{
			_filterType = value;
			_stateCardDrag.SetMyRotationInfo(_myRotationInfo, MyRotationFilterTypeCardPool);
		}
	}

	public abstract bool IsEnableSwipeAutoSameBasicCardAdd();

	protected void InitializeBase(Format format, ConventionDeckList conventionDeckList)
	{
		FormatBehavior = FormatBehaviorManager.Create(format, conventionDeckList);
	}

	protected void SetMyRotationData(MyRotationInfo myRotationInfo)
	{
		_myRotationInfo = myRotationInfo;
		_stateCardDrag.SetMyRotationInfo(myRotationInfo, MyRotationFilterTypeCardPool);
		if (_selectCardFilter != null && _pagingFilter != null && _cardBundle != null)
		{
			_selectCardFilter.SetMyRotationData(myRotationInfo, FilterController.MyRotationFilterType.DECK, MyRotationFilterTypeCardPool == FilterController.MyRotationFilterType.CARD_POOL_ALL_PACK);
			_pagingFilter.SetMyRotationData(myRotationInfo, MyRotationFilterTypeCardPool, isAllBackVisible: false);
			_isDisableTouchWhileLoading = true;
			FetchPagingCard();
			_cardBundle.UpdateMyRotationInfo(myRotationInfo, MyRotationFilterTypeCardPool);
			_myRotationInfoBeforeInitialize = null;
		}
		else
		{
			_myRotationInfoBeforeInitialize = myRotationInfo;
		}
	}

	public override void onFirstStart()
	{
		base.IsShowFooterMenu = false;
		UIEventListener.Get(m_PagingDragPanel).onDrag = OnDragPagingCard;
		UIEventListener.Get(m_PagingDragPanel).onDragOver = OnDragOverPagingCard;
		UIEventListener.Get(m_PagingDragPanel).onScroll = OnScrollPagingCard;
		UIEventListener.Get(m_nextPageBtn.gameObject).onPress = delegate(GameObject g, bool b)
		{
			if (b)
			{
				NextPage();
			}
		};
		UIEventListener.Get(m_prevPageBtn.gameObject).onPress = delegate(GameObject g, bool b)
		{
			if (b)
			{
				PrevPage();
			}
		};
		UIEventListener uIEventListener = UIEventListener.Get(m_nextPageBtn.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, (UIEventListener.VectorDelegate)delegate
		{
			if (IsSlideEnd)
			{
				m_nextPageBtn.state = UIButtonColor.State.Normal;
			}
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(m_prevPageBtn.gameObject);
		uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, (UIEventListener.VectorDelegate)delegate
		{
			if (IsSlideEnd)
			{
				m_prevPageBtn.state = UIButtonColor.State.Normal;
			}
		});
		_simpleDetail = UnityEngine.Object.Instantiate(m_simpleDetailPrefab);
		_simpleDetail.transform.parent = base.transform;
		_simpleDetail.transform.localPosition = Vector3.zero;
		_simpleDetail.transform.localScale = Vector3.one;
		_simpleDetail.OnClickCloseButton += HideDetail;
		_simpleDetail.OnClickCreateButton += OnCreate;
		_simpleDetail.OnClickLiquefyButton += OnLiquefy;
		_cardBundle.OnClickSelectionAreaCard += OnClickSelectionAreaCard;
		_cardBundle.OnClickPagingCard += OnClickPagingCard;
		_cardBundle.OnDragPagingCard += OnDragPagingCard;
		_cardBundle.OnScrollPagingCard += OnScrollPagingCard;
		_cardBundle.OnDragOverPagingCard += OnDragOverPagingCard;
		_cardBundle.OnCreatePagingSleeve += OnCreatePagingSleeve;
		_cardBundle.OnCreatePagingCard += OnCreatePagingCard;
		Action onInputSearchText = delegate
		{
			if (!_isDisableTouchWhileLoading)
			{
				SearchApplyOwn(m_searchInput.value);

			}
		};
		m_searchInput.onSubmit.Add(new EventDelegate(delegate
		{
			if (onInputSearchText != null)
			{
				onInputSearchText();
			}
		}));
		m_searchInput.onDeselect.Add(new EventDelegate(delegate
		{
			onInputSearchText();
		}));
		base.onFirstStart();
	}

	protected override void onOpen()
	{
		base.onOpen();
		UIManager.GetInstance().ShowFooterMenu(isShow: false);
		_selectCardFilter = UnityEngine.Object.Instantiate(_prefabFilter);
		_pagingFilter = UnityEngine.Object.Instantiate(_prefabFilter);
		_selectCardFilter.Initialize(FormatBehavior);
		_pagingFilter.Initialize(FormatBehavior);
		if (_myRotationInfoBeforeInitialize != null)
		{
			_selectCardFilter.SetMyRotationData(_myRotationInfoBeforeInitialize, FilterController.MyRotationFilterType.DECK, isAllBackVisible: false);
			_pagingFilter.SetMyRotationData(_myRotationInfoBeforeInitialize, MyRotationFilterTypeCardPool, isAllBackVisible: false);
			_cardBundle.UpdateMyRotationInfo(_myRotationInfoBeforeInitialize, MyRotationFilterTypeCardPool);
		}
		_selectCardFilter.Hide();
		_pagingFilter.Hide();
		RegistFilterEvent();
		/* Pre-Phase-5b: GameObjMgr.GetUIContainer().SetActive(false) dropped; headless has no UIContainer. */
		_stateEdit.ResetScroll();
		m_searchInput.value = "";
		m_searchCancelButton.gameObject.SetActive(value: false);
	}

	private void RegistFilterEvent()
	{
		_selectCardFilter.OnValidate += OnValidateSelectionAreaFilter;
		_pagingFilter.OnValidate += OnValidatePagingFilter;
	}

	private void ClearFilterEvent()
	{
		_selectCardFilter.OnValidate -= OnValidateSelectionAreaFilter;
		_pagingFilter.OnValidate -= OnValidatePagingFilter;
	}

	protected List<string> GetEffectAssetPathList()
	{
		return new List<string>
		{
			Toolbox.ResourcesManager.GetAssetTypePath("cmn_frame_card_1", ResourcesManager.AssetLoadPathType.Effect2D),
			Toolbox.ResourcesManager.GetAssetTypePath("cmn_frame_card_3", ResourcesManager.AssetLoadPathType.Effect2D),
			Toolbox.ResourcesManager.GetAssetTypePath("cmn_frame_card_2", ResourcesManager.AssetLoadPathType.Effect2D)
		};
	}

	protected void SetupEffect(Action callback)
	{
		List<GameObject> effectObjList = new List<GameObject>
		{
			Toolbox.ResourcesManager.LoadObject<GameObject>(Toolbox.ResourcesManager.GetAssetTypePath("cmn_frame_card_3", ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true)),
			Toolbox.ResourcesManager.LoadObject<GameObject>(Toolbox.ResourcesManager.GetAssetTypePath("cmn_frame_card_2", ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true)),
			Toolbox.ResourcesManager.LoadObject<GameObject>(Toolbox.ResourcesManager.GetAssetTypePath("cmn_frame_card_1", ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true))
		};
		/* Pre-Phase-5b: SetUIParticleShader dropped */ if (callback != null) callback();
	}

	protected void SetupState()
	{
		_stateEdit.RefreshSelectionArea(isImmediate: true);
		_stateEdit.RefreshPage(isImmediate: true);
	}

	protected void SetupSimpleDetail()
	{
		if (FormatBehavior.IsConventionMode)
		{
			_simpleDetail.ActiveCraftPanel(isActive: false);
		}
	}

	protected override void onClose()
	{
		_cardBundle.DisableNewFlagDisplayedCards();
		_cardBundle.Destroy();
		_stateCardDrag.DestroyDragCard();
		UnityEngine.Object.Destroy(_selectCardFilter.gameObject);
		UnityEngine.Object.Destroy(_pagingFilter.gameObject);
		Toolbox.ResourcesManager.RemoveAssetGroup(Toolbox.ResourcesManager.Card2DAssetPathList);
		Toolbox.ResourcesManager.RemoveAssetGroup(_resourcePathList);
		Toolbox.ResourcesManager.CardListAssetPathList.Clear();
		_stateEdit.RefreshSelectionArea(isImmediate: true);
		_stateEdit.RefreshPage(isImmediate: true);
		MyPageMenu.SetEnableReloadCard();
		UIDrawCall.ReleaseInactive();
		base.onClose();
	}

	public override void onMove()
	{
		if (_cardBundle != null)
		{
			_cardBundle.Tick();
			base.onMove();
		}
	}

	protected virtual void OnFinishFadeIn()
	{
		ChangeState(_stateDefaultView, skipCloseAnim: true);
		ClearFilterEvent();
		_selectCardFilter.Reset();
		_pagingFilter.Reset();
		RegistFilterEvent();
		IsSetup = false;
	}

	public bool LoadPagingCard(int page)
	{
		IsLoading = true;
		HideDetail();
		return _cardBundle.LoadPagingCard(page, isDestroyImmediate: true);
	}

	public void FetchPagingCard()
	{
		IsLoading = true;
		HideDetail();
		_cardBundle.FetchPagingCard();
	}

	public void NextPage()
	{
		if (!IsLoading && m_state != _stateCardDrag && !m_searchInput.isSelected && _cardBundle.CurrentPage + 1 <= _cardBundle.MaxPage)
		{
			if (m_searchInput.ClearIMEOnlyOSX())
			{
				SearchApplyOwn(m_searchInput.value);
				return;
			}
			IsSlideEnd = false;
			_slideType = SlideType.Next;
			IsLoading = true;
			PagingBase(_stateEdit.NextPage, _cardBundle.CurrentPage + 1, NextPage);
		}
		else if (IsLoading)
		{
			if (_slideType == SlideType.Prev)
			{
				IsSlideEnd = true;
				m_nextPageBtn.state = UIButtonColor.State.Normal;
			}
		}
		else if (m_searchInput.isSelected)
		{
			if (m_searchInput.ClearIMEOnlyOSX())
			{
				SearchApplyOwn(m_searchInput.value);
			}
			m_searchInput.ClearFocusOnlyOSX();
		}
	}

	public void PrevPage()
	{
		if (!IsLoading && m_state != _stateCardDrag && !m_searchInput.isSelected && _cardBundle.CurrentPage - 1 >= 0)
		{
			if (m_searchInput.ClearIMEOnlyOSX())
			{
				SearchApplyOwn(m_searchInput.value);
				return;
			}
			IsSlideEnd = false;
			_slideType = SlideType.Prev;
			IsLoading = true;
			PagingBase(_stateEdit.PrevPage, _cardBundle.CurrentPage - 1, PrevPage);
		}
		else if (IsLoading)
		{
			if (_slideType == SlideType.Next)
			{
				IsSlideEnd = true;
				m_prevPageBtn.state = UIButtonColor.State.Normal;
			}
		}
		else if (m_searchInput.isSelected)
		{
			if (m_searchInput.ClearIMEOnlyOSX())
			{
				SearchApplyOwn(m_searchInput.value);
			}
			m_searchInput.ClearFocusOnlyOSX();
		}
	}

	private void PagingBase(Action<Action> anim, int page, Action next)
	{

		anim.Call(delegate
		{
			if (LoadPagingCard(page))
			{
				Action replay = null;
				replay = delegate
				{
					_cardBundle.OnCreatePagingCard -= replay;
					if (Input.GetMouseButton(0) && !IsSlideEnd && (m_nextPageBtn.state == UIButtonColor.State.Pressed || m_prevPageBtn.state == UIButtonColor.State.Pressed))
					{
						next();
					}
					else
					{
						_slideType = SlideType.None;
						IsSlideEnd = true;
					}
				};
				_cardBundle.OnCreatePagingCard += replay;
			}
		});
	}

	public virtual int InsertToSelectionArea(CardObject card)
	{
		int num = _cardBundle.InsertToSelectionArea(card);
		CardObject cardObject = SelectionAreaList.FindWithIndex(num);
		if (cardObject != null && SelectionAreaList.CountKind > 10)
		{
			int num2 = Mathf.Clamp(num, 5, SelectionAreaList.CountKind - 5);
			if (cardObject.TotalCardNum == 1 && num > 0)
			{
				num2++;
			}
			_stateEdit.CenterOn(num2);
		}
		if (_scrollView != null)
		{
			_scrollView.customMovement = ((SelectionAreaList.CountKind > 10) ? Vector2.right : Vector2.zero);
		}
		return num;
	}

	public virtual int RemoveFromSelectionArea(CardObject card)
	{
		int result = _cardBundle.RemoveFromSelectionArea(card);
		_stateEdit.Fit();
		if (_scrollView != null)
		{
			_scrollView.customMovement = ((SelectionAreaList.CountKind > 10) ? Vector2.right : Vector2.zero);
		}
		return result;
	}

	private bool IsMaxCardNumInSelectionArea(int cardId)
	{
		if (!_enableSelectSameKindCardNum)
		{
			return false;
		}
		int num = GetSameKindNumMaxInFormat(cardId);
		if (_myRotationInfo != null)
		{
			num = _myRotationInfo.GetSameCardCount(CardMaster.GetInstance(FormatBehavior.CardMasterId).GetCardParameterFromId(cardId).BaseCardId);
		}
		if (_cardBundle.CountCardNumInSelectionArea(cardId, isStrictSameCard: false) >= num)
		{
			return true;
		}
		return false;
	}

	protected virtual int GetSameKindNumMaxInFormat(int cardId)
	{
		return FormatBehavior.DeckSameKindCardNumMax;
	}

	public bool IsExistCardCardPool(int cardId)
	{
		if (IsMaxCardNumInSelectionArea(cardId))
		{
			return false;
		}
		int num = SelectionAreaList.CardList.Where((CardObject c) => c.CardId == cardId).Sum((CardObject c) => c.TotalCardNum);
		return FormatBehavior.GetPossessionCardNum(cardId, _isSelectableSpotCard) > num;
	}

	public bool IsAddableByBaseCardId(int cardId, out int addCardId)
	{
		if (IsMaxCardNumInSelectionArea(cardId))
		{
			addCardId = 0;
			return false;
		}
		CardMaster instance = CardMaster.GetInstance(FormatBehavior.CardMasterId);
		List<int> list = new List<int>();
		foreach (int item in instance.GetSameCardListByBaseCardId(instance.GetCardParameterFromId(cardId).BaseCardId))
		{
			if (FormatBehavior.SortedDeckUsableCardList.Contains(item))
			{
				list.Add(item);
			}
		}
		IOrderedEnumerable<int> orderedEnumerable = list.OrderBy((int id) => -id);
		CardParameter cardParameterFromId = instance.GetCardParameterFromId(cardId);
		foreach (int item2 in list)
		{
			if (instance.GetCardParameterFromId(item2).NormalCardId == cardParameterFromId.NormalCardId && IsExistCardCardPool(item2))
			{
				addCardId = item2;
				return true;
			}
		}
		foreach (int item3 in orderedEnumerable)
		{
			if (instance.GetCardParameterFromId(item3).IsFoil == cardParameterFromId.IsFoil && IsExistCardCardPool(item3))
			{
				addCardId = item3;
				return true;
			}
		}
		foreach (int item4 in orderedEnumerable)
		{
			if (IsExistCardCardPool(item4))
			{
				addCardId = item4;
				return true;
			}
		}
		if (_cardBundle.CanUseNonPossessionCard)
		{
			if (instance.GetCardParameterFromId(cardId).CanCraft)
			{
				addCardId = cardId;
				return true;
			}
			foreach (int item5 in list)
			{
				if (instance.GetCardParameterFromId(item5).CanCraft)
				{
					addCardId = item5;
					return true;
				}
			}
		}
		addCardId = 0;
		return false;
	}

	public bool IsRemainingAddableCardToSelectionArea(int cardId)
	{
		if (IsMaxCardNumInSelectionArea(cardId))
		{
			return false;
		}
		CardParameter cardParameterFromId = CardMaster.GetInstance(FormatBehavior.CardMasterId).GetCardParameterFromId(cardId);
		if (_cardBundle.CanUseNonPossessionCard && DeckCardEditUI.IsSelectableNonPossessionCard(cardParameterFromId))
		{
			return true;
		}
		int num = SelectionAreaList.CardList.Where((CardObject c) => c.CardId == cardId).Sum((CardObject c) => c.TotalCardNum);
		return FormatBehavior.GetPossessionCardNum(cardId, _isSelectableSpotCard) > num;
	}

	private void ShowDetail(GameObject obj, int id, bool isSelectionArea)
	{
		if (!IsLoading && !UIManager.GetInstance().isOpenDialog())
		{
			SelectCard(obj, isSelectionArea);
			_simpleDetail.ChangeDetail(id, FormatBehavior.CardMasterId, _myRotationInfo);
			_simpleDetail.ActiveCraftPanel(!isSelectionArea && IsShowCardDetailCraftPanel);
		}
	}

	public void HideDetail()
	{
		SelectCard(null, isSelectionArea: false);
		_simpleDetail.HideDetail();
	}

	private void SelectCard(GameObject obj, bool isSelectionArea)
	{
		int setype = 0;
		CardObject cardObject = SelectionAreaList.FindWithObject(_detailTargetObj);
		if (cardObject == null)
		{
			cardObject = PagingList.FindWithObject(_detailTargetObj);
		}
		if (cardObject != null && cardObject.IsVisibleCursorEffect)
		{
			cardObject.ChangeSelectingState(isSelect: false);
			setype = 0;
		}
		CardObject cardObject2 = (isSelectionArea ? SelectionAreaList.FindWithObject(obj) : PagingList.FindWithObject(obj));
		if (cardObject2 != null)
		{
			cardObject2.ChangeSelectingState(isSelect: true);
			setype = 0;
		}
		_detailTargetObj = obj;

	}

	protected virtual void AccordCardInfo()
	{
		_cardBundle.AccordCardInfo();
		_stateEdit.RefreshSelectionArea(isImmediate: false);
	}

	private void SearchApplyOwn(string word)
	{
		SetSearchKeyword(word);
		_isDisableTouchWhileLoading = true;
		FetchPagingCard();
	}

	private void SetSearchKeyword(string word)
	{
		if (!(_cardBundle.FilterParameter.Word == word))
		{
			m_searchInput.RemoveFocus();
			m_searchCancelButton.gameObject.SetActive(word.Length > 0);
			UIBase_CardManager.FilterParameter filterParameter = _cardBundle.FilterParameter;
			string word2 = (m_searchInput.value = word);
			filterParameter.Word = word2;
			_cardBundle.FilterParameter = filterParameter;
		}
	}

	protected virtual void OnValidateSelectionAreaFilter()
	{
		_stateCardDrag.OnChangeSelectionAreaFilter();
		UIBase_CardManager.FilterParameter filterParameter = _selectCardFilter.GetFilterParameter(new UIBase_CardManager.FilterParameter());
		SelectionAreaList.ApplyFilter(filterParameter);
		_stateEdit.RefreshSelectionArea(isImmediate: false);
		_stateEdit.Fit();
	}

	protected virtual void OnValidatePagingFilter()
	{
		if (!_isDisableTouchWhileLoading)
		{
			SetSearchKeyword(m_searchInput.value);
			_cardBundle.FilterParameter = _pagingFilter.GetFilterParameter(_cardBundle.FilterParameter);
			_isDisableTouchWhileLoading = true;
			FetchPagingCard();
		}
	}

	protected virtual void OnCreatePagingSleeve()
	{
		m_labelPageInfo.text = Data.SystemText.Get("Card_0053", (_cardBundle.CurrentPage + 1).ToString(), (_cardBundle.MaxPage + 1).ToString());
		bool flag = _cardBundle.CurrentPage + 1 <= _cardBundle.MaxPage;
		bool flag2 = _cardBundle.CurrentPage - 1 >= 0;
		m_nextPageBtn.gameObject.SetActive(flag);
		m_prevPageBtn.gameObject.SetActive(flag2);
		m_nextPageBtnGrayout.gameObject.SetActive(!flag);
		m_prevPageBtnGrayout.gameObject.SetActive(!flag2);
		_stateEdit.RefreshPage(isImmediate: false);
	}

	private void OnCreatePagingCard()
	{
		IsLoading = false;
		_isDisableTouchWhileLoading = false;
		_stateEdit.RefreshSelectionArea(isImmediate: true);
		_stateEdit.RefreshPage(isImmediate: true);
		m_noEditCardLabel.gameObject.SetActive(_cardBundle.PagingList.CountKind <= 0);
	}

	private void OnCreate()
	{
		_cardBundle.OnCreateCard(_simpleDetail.CardID);
		AccordCardInfo();
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		if (dataMgr.GetPossessionCardNum(_simpleDetail.CardID, _isSelectableSpotCard) == 1)
		{
			if (!_cardBundle.IsCraftMode)
			{
				_cardBundle.FetchPagingCard();
			}
			dataMgr.SetIsNewCard(_simpleDetail.CardID, isNew: false);
		}
	}

	protected virtual void OnLiquefy(int cardId)
	{
		AccordCardInfo();
	}

	private void OnClickSelectionAreaCard(GameObject obj)
	{
		OnClickCard(obj, isSelectionArea: true, isIdCheck: true);
	}

	private void OnClickPagingCard(GameObject obj)
	{
		OnClickCard(obj, isSelectionArea: false, isIdCheck: true);
	}

	private void OnClickCard(GameObject obj, bool isSelectionArea, bool isIdCheck)
	{
		if (!_stateEdit.IsClick && !(m_state == _stateDefaultView))
		{
			return;
		}
		CharIdx component = obj.GetComponent<CharIdx>();
		if (component != null)
		{
			if (isIdCheck && _simpleDetail.IsVisible && _simpleDetail.CardID == component.GetCardId())
			{
				HideDetail();
			}
			else
			{
				ShowDetail(obj, component.GetCardId(), isSelectionArea);
			}
		}
	}

	private void OnDragPagingCard(GameObject obj, Vector2 dir)
	{
		if (Mathf.Abs(dir.x) >= 70f)
		{
			RequestChangePage(dir.x);
		}
	}

	private void OnScrollPagingCard(GameObject o, float factor)
	{
		RequestChangePage(factor);
	}

	private void RequestChangePage(float dir)
	{
		if (!(m_state == _stateEdit) || _simpleDetail.IsVisible || IsLoading)
		{
			return;
		}
		if (dir > 0f)
		{
			if (_cardBundle.CurrentPage > 0)
			{
				PrevPage();
			}
		}
		else if (_cardBundle.CurrentPage < _cardBundle.MaxPage)
		{
			NextPage();
		}
	}

	private void OnDragOverPagingCard(GameObject obj)
	{
		if (_simpleDetail.IsVisible && !_simpleDetail.IsDetailPanelDragging)
		{
			OnClickCard(obj, isSelectionArea: false, isIdCheck: false);
		}
	}

	public override bool IsGetOutOfScene(Action backupExec)
	{
		if (IsLoading)
		{
			return false;
		}
		if (_selectCardFilter.IsShow)
		{
			_selectCardFilter.Hide();
			return false;
		}
		if (_pagingFilter.IsShow)
		{
			_pagingFilter.Hide();
			return false;
		}
		if (_simpleDetail.IsVisible)
		{
			HideDetail();
			return false;
		}
		if (m_state == _stateCardDrag)
		{
			return false;
		}
		return true;
	}
}
