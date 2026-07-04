using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Dialog.Setting;

namespace Wizard.DeckCardEdit;

public class DeckCardEditUI : CardSelectListUIBase
{
	private class CopyDeckFormation
	{
		private enum CardType
		{
			Original,
			OriginalNormal,
			OriginalPremium,
			BaseNormal,
			BasePremium,
			SameBaseNormal,
			SameBasePremium,
			PrizeNormal,
			PrizePremium
		}

		private List<int> _sourceDeckCardList;

		private List<int> _afterDeckCardList;

		private Dictionary<int, int> _userOwnCardData;

		private IFormatBehavior _formatBehavior;

		public CopyDeckFormation(List<int> sourceDeckCardList, IFormatBehavior formatBehavior)
		{
			_sourceDeckCardList = sourceDeckCardList;
			_afterDeckCardList = new List<int>(40);
			_userOwnCardData = formatBehavior.ClonePossessionCardDictionary(isIncludingSpotCard: true);
			_formatBehavior = formatBehavior;
		}

		public List<int> EditNormal()
		{
			for (int i = 0; i < _sourceDeckCardList.Count; i++)
			{
				int num = _sourceDeckCardList[i];
				if (!ConvertCardTypeAndAddCardToDeck(CardType.Original, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalPremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.PrizeNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.PrizePremium, num) && CanUseNonPossessionCard)
				{
					AddNewestCraftableCardToDeck(num);
				}
			}
			return _afterDeckCardList;
		}

		public List<int> EditPremium()
		{
			for (int i = 0; i < _sourceDeckCardList.Count; i++)
			{
				int num = _sourceDeckCardList[i];
				if (!ConvertCardTypeAndAddCardToDeck(CardType.OriginalPremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.PrizePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.PrizeNormal, num) && CanUseNonPossessionCard)
				{
					AddNewestCraftableCardToDeck(num);
				}
			}
			return _afterDeckCardList;
		}

		public List<int> EditPrize()
		{
			for (int i = 0; i < _sourceDeckCardList.Count; i++)
			{
				int num = _sourceDeckCardList[i];
				if (!ConvertCardTypeAndAddCardToDeck(CardType.PrizeNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.PrizePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalPremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBasePremium, num) && CanUseNonPossessionCard)
				{
					AddNewestCraftableCardToDeck(num);
				}
			}
			return _afterDeckCardList;
		}

		public List<int> EditPremiumAndPrize()
		{
			for (int i = 0; i < _sourceDeckCardList.Count; i++)
			{
				int num = _sourceDeckCardList[i];
				if (!ConvertCardTypeAndAddCardToDeck(CardType.PrizePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.PrizeNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalPremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBasePremium, num) && !ConvertCardTypeAndAddCardToDeck(CardType.OriginalNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.SameBaseNormal, num) && !ConvertCardTypeAndAddCardToDeck(CardType.BaseNormal, num) && CanUseNonPossessionCard)
				{
					AddNewestCraftableCardToDeck(num);
				}
			}
			return _afterDeckCardList;
		}

		private bool ConvertCardTypeAndAddCardToDeck(CardType cardType, int originalCardId)
		{
			CardMaster _cardMaster = CardMaster.GetInstance(_formatBehavior.CardMasterId);
			CardParameter cardParameterFromId = _cardMaster.GetCardParameterFromId(originalCardId);
			return cardType switch
			{
				CardType.Original => AddCardToDeck(cardParameterFromId.CardId), 
				CardType.OriginalNormal => AddCardToDeck(cardParameterFromId.NormalCardId), 
				CardType.OriginalPremium => AddCardToDeck(cardParameterFromId.FoilCardId), 
				CardType.BaseNormal => AddCardToDeck(_cardMaster.GetCardParameterFromId(cardParameterFromId.BaseCardId).NormalCardId), 
				CardType.BasePremium => AddCardToDeck(_cardMaster.GetCardParameterFromId(cardParameterFromId.BaseCardId).FoilCardId), 
				CardType.SameBaseNormal => (from id in GetSameBaseIdListInUserCard(cardParameterFromId.BaseCardId).Where(delegate(int id)
					{
						CardParameter cardParameterFromId2 = _cardMaster.GetCardParameterFromId(id);
						return !cardParameterFromId2.IsFoil && !cardParameterFromId2.IsPrizeCard;
					})
					orderby id descending
					select id).ToList().Find(AddCardToDeck) > 0, 
				CardType.SameBasePremium => (from id in GetSameBaseIdListInUserCard(cardParameterFromId.BaseCardId).Where(delegate(int id)
					{
						CardParameter cardParameterFromId2 = _cardMaster.GetCardParameterFromId(id);
						return cardParameterFromId2.IsFoil && !cardParameterFromId2.IsPrizeCard;
					})
					orderby id descending
					select id).ToList().Find(AddCardToDeck) > 0, 
				CardType.PrizeNormal => (from id in GetSameBaseIdListInUserCard(cardParameterFromId.BaseCardId).Where(delegate(int id)
					{
						CardParameter cardParameterFromId2 = _cardMaster.GetCardParameterFromId(id);
						return cardParameterFromId2.IsReprintedCard && !cardParameterFromId2.IsFoil;
					})
					orderby id descending
					select id).ToList().Find(AddCardToDeck) > 0, 
				CardType.PrizePremium => (from id in GetSameBaseIdListInUserCard(cardParameterFromId.BaseCardId).Where(delegate(int id)
					{
						CardParameter cardParameterFromId2 = _cardMaster.GetCardParameterFromId(id);
						return cardParameterFromId2.IsReprintedCard && cardParameterFromId2.IsFoil;
					})
					orderby id descending
					select id).ToList().Find(AddCardToDeck) > 0, 
				_ => false, 
			};
		}

		private List<int> GetSameBaseIdListInUserCard(int baseCardId)
		{
			return _userOwnCardData.Keys.ToList().FindAll(delegate(int id)
			{
				CardParameter cardParameterFromId = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(id);
				return cardParameterFromId.BaseCardId == baseCardId && cardParameterFromId.NormalCardId != baseCardId;
			});
		}

		private bool AddCardToDeck(int cardId)
		{
			if (IsHaveCard(cardId))
			{
				_afterDeckCardList.Add(cardId);
				_userOwnCardData[cardId]--;
				return true;
			}
			return false;
		}

		private void AddNewestCraftableCardToDeck(int cardId)
		{
			int newestCraftableCardId = GetNewestCraftableCardId(CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(cardId).BaseCardId, _formatBehavior);
			_afterDeckCardList.Add(newestCraftableCardId);
			if (_userOwnCardData.ContainsKey(newestCraftableCardId))
			{
				_userOwnCardData[newestCraftableCardId]--;
			}
		}

		private bool IsHaveCard(int cardId)
		{
			int value = 0;
			_userOwnCardData.TryGetValue(cardId, out value);
			return value > 0;
		}
	}

	public static string CurrentDeckName = null;

	private static Format EditDeckFormat = Format.Max;

	private static int CurrentDeckId = 1;

	public static DeckData CurrentDeckData = null;

	public static ClassSet ClassSet = null;

	public static MyRotationInfo MyRotationInfo = null;

	private static DeckData CopySrcDeckData = null;

	private static bool IsCopySkinAndSleeve = false;

	private static bool IsCopySubClass = false;

	private static bool IsCreatedByBuilder = false;

	private static bool IsCreateAuto = false;

	private static ConventionDeckList _conventionDeckList = null;

	private static bool CanUseNonPossessionCard = false;

	private DeckGroupListData _deckGroupListData;

	private readonly Vector3 MY_ROTATION_FORMAT_ICON_POSITION = new Vector3(45f, -47f, 0f);

	private readonly Vector3 MY_ROTATION_CARD_NUMBER_ICON_POSITION = new Vector3(68f, -62f, -1f);

	private readonly Vector3 MY_ROTATION_CARD_NUMBER_POSITION = new Vector3(24f, 0f, 0f);

	private readonly Vector3 MY_ROTATION_CARD_NUMBER_MAX_POSITION = new Vector3(58f, 0f, 0f);

	[SerializeField]
	private DeckBuildShortageCardView _prefabShortageCardView;

	[SerializeField]
	private UISprite _formatIcon;

	[SerializeField]
	private CostCurveUI m_costCurve;

	[SerializeField]
	private UICardList m_deckViewerPrefab;

	private UICardList _deckViewer;

	[SerializeField]
	private CardDetailUI m_cardDetailPrefab;

	private CardDetailUI _cardDetail;

	[SerializeField]
	private UILabel m_labelDeckName;

	[SerializeField]
	private UILabel m_labelClassName;

	[SerializeField]
	private UILabel m_labelDeckCardNum;

	private int _deckCardNum;

	[SerializeField]
	private UILabel _labelDeckCardMaxNum;

	[SerializeField]
	private UILabel m_labelCharCardNum;

	[SerializeField]
	private UILabel m_labelSpellCardNum;

	[SerializeField]
	private UILabel m_labelFieldCardNum;

	[SerializeField]
	private UIButton m_craftOnBtn;

	[SerializeField]
	private UIButton m_craftOffBtn;

	[SerializeField]
	private UIButton m_saveButton;

	[SerializeField]
	private UIButton m_autoCreateButton;

	[SerializeField]
	private UIButton m_deckViewerButton;

	[SerializeField]
	private RewardBase _rewardBase;

	[SerializeField]
	private UIButton _craftShortageCardButton;

	[SerializeField]
	private UIButton _swapClassButton;

	[SerializeField]
	private GameObject _useSubClassRoot;

	[SerializeField]
	private UILabel _useSubClassDeckCardNumLabel;

	[SerializeField]
	private UILabel _useSubClassDeckCardNumMaxLabel;

	[SerializeField]
	private UISprite _useSubClassMainClassIconSprite;

	[SerializeField]
	private UILabel _useSubClassMainClassCardNumLabel;

	[SerializeField]
	private UILabel _useSubClassMainClassCardNumMinLabel;

	[SerializeField]
	private UISprite _useSubClassSubClassIconSprite;

	[SerializeField]
	private UILabel _useSubClassSubClassCardNumLabel;

	[SerializeField]
	private UILabel _useSubClassSubClassCardNumMinLabel;

	[SerializeField]
	private UISprite _useSubClassFormatIconSprite;

	[SerializeField]
	private UIButton _myRotationChangeButton;

	[SerializeField]
	private GameObject _myRotationRoot;

	[SerializeField]
	private GameObject _myRotationRoot2;

	[SerializeField]
	private UIGrid _myRotationAbilityGrid;

	[SerializeField]
	private GameObject _myRotationIconOriginal;

	[SerializeField]
	private UISprite _myRotationClassIcon;

	[SerializeField]
	private ItemToggle _myRotationAllPackVisibleToggle;

	private bool _myRotationAllPackVisible;

	[SerializeField]
	private GameObject _cardNumberIcon;

	[SerializeField]
	private UISprite _classNameUnderLine;

	[SerializeField]
	private UILabel _myRotationPackShortName;

	[SerializeField]
	private BoxCollider _allPackVisibleCollider;

	private MyRotationInfo _myRotationInfo;

	private bool _hasChanged;

	private bool _fadeInFinish;

	private IFormatBehavior _formatBehavior;

	private List<int> _shortageIdList = new List<int>();

	private bool _isEnableSpotCard;

	private bool _enableFitOnChangeDeckCardNumChange = true;

	private List<string> _otherDeckNames = new List<string>();

	private bool _needsClearSkin;

	private bool _isDefaultDeckName;

	private ConventionInfo ConventionInfo
	{
		get
		{
			if (_conventionDeckList != null)
			{
				return _conventionDeckList.Conventioninfo;
			}
			return null;
		}
	}

	private CardBundleController _deckCardBundle
	{
		get
		{
			return (CardBundleController)_cardBundle;
		}
		set
		{
			_cardBundle = value;
		}
	}

	private int DeckCardNum
	{
		get
		{
			return _deckCardNum;
		}
		set
		{
			_deckCardNum = value;
			UIManager.SetObjectToGrey(m_saveButton.gameObject, _deckCardNum <= 0);
			_deckCardBundle.SelectionAreaList.CountEachType(out var charNum, out var spellNum, out var fieldNum);
			m_labelCharCardNum.text = charNum.ToString();
			m_labelSpellCardNum.text = spellNum.ToString();
			m_labelFieldCardNum.text = fieldNum.ToString();
			if (_enableFitOnChangeDeckCardNumChange)
			{
				_stateEdit.Fit();
			}
			this.OnChangeDeckCardNum.Call(value);
		}
	}

	public bool IsBattleRetry { get; set; }

	public event Action<int> OnChangeDeckCardNum;

	private DeckData GetDeck(Format format, int deckId)
	{
		if (_conventionDeckList != null)
		{
			return _conventionDeckList.DeckList[deckId];
		}
		return _deckGroupListData.GetDeckByAttribute(DeckAttributeType.CustomDeck, deckId, format);
	}

	public static void SetDeckEditParameter(DeckData deck, ConventionDeckList conventionDeckList, bool canUseNonPossessionCard = true)
	{
		CurrentDeckName = deck.GetDeckName();
		CurrentDeckId = deck.GetDeckID();
		CurrentDeckData = deck;
		EditDeckFormat = deck.Format;
		ClassSet = new ClassSet(deck.GetDeckClassID(), deck.GetDeckSubClassID());
		CopySrcDeckData = null;
		IsCopySkinAndSleeve = false;
		IsCopySubClass = false;
		IsCreatedByBuilder = false;
		IsCreateAuto = false;
		_conventionDeckList = conventionDeckList;
		CanUseNonPossessionCard = conventionDeckList == null && canUseNonPossessionCard;
		MyRotationInfo = Data.MyRotationAllInfo.Get(deck.MyRotationId);
		UIManager.GetInstance().OverrideSceneParam(UIManager.ViewScene.DeckList, new DeckListUIParam(deck.Format, conventionDeckList?.Conventioninfo));
	}

	public override bool IsEnableSwipeAutoSameBasicCardAdd()
	{
		return true;
	}

	private static MyRotationInfo GetDefaultMyRotationData()
	{
		return Data.MyRotationAllInfo.MyRotationInfoList[Data.MyRotationAllInfo.MyRotationInfoList.Count - 1];
	}

	public override void onFirstStart()
	{
		if (!DeckListUI.IsSpecialFormatPeriodError(EditDeckFormat))
		{
			if (EditDeckFormat == Format.MyRotation && MyRotationInfo == null)
			{
				MyRotationInfo = GetDefaultMyRotationData();
			}
			if (MyRotationInfo != null)
			{
				_myRotationInfo = MyRotationInfo;
				MyRotationInfo = null;
				SetMyRotationData(_myRotationInfo);
			}
			_deckViewer = UnityEngine.Object.Instantiate(m_deckViewerPrefab);
			_deckViewer.transform.parent = base.transform;
			_deckViewer.transform.localPosition = Vector3.zero;
			_deckViewer.transform.localScale = Vector3.one;
			_deckViewer.SetCamera(null);
			_deckViewer.SetShareButtonUse(isUse: true);
			_deckViewer.gameObject.SetActive(value: false);
			TopBar topBar = UIManager.GetInstance().CreateTopBar(base.gameObject, null, UIManager.ViewScene.DeckList, MoneyDraw: false);
			InitFormatDependency();
			m_costCurve.Initialize(base.FormatBehavior.CardMasterId);
			Transform obj = topBar.BackButton.transform;
			Vector3 localPosition = obj.localPosition;
			localPosition.y = -40.7f;
			obj.localPosition = localPosition;
			base.Format = EditDeckFormat;
			UIEventListener.Get(m_autoCreateButton.gameObject).onClick = OnClickAutoDeckBtn;
			UIEventListener.Get(m_saveButton.gameObject).onClick = OnClickSaveBtn;
			UIEventListener.Get(m_craftOnBtn.gameObject).onClick = delegate
			{
				ChangeCraftMode(isCraft: true);
			};
			UIEventListener.Get(m_craftOffBtn.gameObject).onClick = delegate
			{
				ChangeCraftMode(isCraft: false);
			};
			UIEventListener.Get(m_deckViewerButton.gameObject).onClick = delegate
			{
				ShowDeckViewer();
			};
			UIEventListener.Get(_craftShortageCardButton.gameObject).onClick = OnClickCraftShortageCardButton;
			UIEventListener.Get(_swapClassButton.gameObject).onClick = OnClickSwapClassButton;
			UIEventListener.Get(_myRotationChangeButton.gameObject).onClick = OnClickMyRotationChangeButton;
			_craftShortageCardButton.gameObject.SetActive(CanUseNonPossessionCard);
			_swapClassButton.gameObject.SetActive(base.FormatBehavior.UseSubClass);
			_isSelectableSpotCard = true;
			_isEnableSpotCard = !base.FormatBehavior.IsConventionMode;
			_deckCardBundle = new CardBundleController(_parentSelectionObj, m_parentPagingObj, m_sleeveOriginal, m_cardInfoOriginal, _formatBehavior, _isEnableSpotCard, _isEnableSpotCard, _isEnableSpotCard, CanUseNonPossessionCard);
			_deckCardBundle.OnCreateDeckSleeve += OnCreateDeckSleeve;
			_deckCardBundle.OnCreateDeckCard += OnCreateDeckCard;
			_deckCardBundle.OnCreateAutoDeck += OnCreateAutoDeck;
			_deckCardBundle.OnInsertDeckCard += OnInsertDeckCard;
			_deckCardBundle.OnRemoveDeckCard += OnRemoveDeckCard;
			_enableSelectSameKindCardNum = true;
			base.onFirstStart();
			DeckListUtility.SaveLastSelectDeck(CurrentDeckId, isDefaultDeck: false, isTrialDeck: false, EditDeckFormat);
			if (ConventionInfo == null)
			{
				UIManager.GetInstance().UpdateFooterMenuTexture(UIManager.ViewScene.DeckCardEdit);
			}
			CardMaster.CardMasterId cardMasterId = base.FormatBehavior.CardMasterId;
			List<int> cardPool = UIManager.GetInstance().getUIBase_CardManager().SortIDList(CardMaster.GetInstance(cardMasterId).GetAllCardIds(), cardMasterId);
			FilterController.MyRotationFilterType myRotationFilterType = FilterController.MyRotationFilterType.None;
			if (_myRotationInfo != null)
			{
				myRotationFilterType = base.MyRotationFilterTypeCardPool;
			}
			_selectCardFilter.InitializeFilterForDeckEdit(cardPool, ClassSet, EditDeckFormat, _myRotationInfo, myRotationFilterType);
			_pagingFilter.InitializeFilterForDeckEdit(cardPool, ClassSet, EditDeckFormat, _myRotationInfo, myRotationFilterType);
			_stateCardDrag.AddCardForSameCardSwipe = AddCardForSwipe;
			_myRotationRoot.SetActive(EditDeckFormat == Format.MyRotation);
			_myRotationRoot2.SetActive(EditDeckFormat == Format.MyRotation);
			_myRotationIconOriginal.SetActive(value: false);
			if (EditDeckFormat == Format.MyRotation)
			{
				InitializeMyRotation();
				UpdateMyRotationAbilityIcon();
				SetMyRotationRayout();
			}
			StartCoroutine(Setup_Coroutine());
		}
	}

	private void SetMyRotationRayout()
	{
		_formatIcon.transform.localPosition = MY_ROTATION_FORMAT_ICON_POSITION;
		_cardNumberIcon.transform.localPosition = MY_ROTATION_CARD_NUMBER_ICON_POSITION;
		m_labelClassName.width = 107;
		m_labelDeckCardNum.transform.localPosition = MY_ROTATION_CARD_NUMBER_POSITION;
		_labelDeckCardMaxNum.transform.localPosition = MY_ROTATION_CARD_NUMBER_MAX_POSITION;
		_classNameUnderLine.width = 184;
	}

	private void InitializeMyRotation()
	{
		_myRotationClassIcon.spriteName = ClassCharaPrm.GetIconSpriteName(ClassSet.MainClass);
		m_labelClassName.enabled = false;
		_myRotationPackShortName.text = _myRotationInfo.LastPackText;
	}

	private void AddCardForSwipe(int cardNo)
	{
		_enableFitOnChangeDeckCardNumChange = false;
		List<int> list = new List<int>();
		list.Add(cardNo);
		_deckCardBundle.CreateDeckAddCard(list);
	}

	private IEnumerator Setup_Coroutine()
	{
		base.IsSetup = true;
		yield return null;
		_hasChanged = false;
		OnChangeSlideEnd = delegate(bool slideEnd)
		{
			_allPackVisibleCollider.enabled = slideEnd;
		};
		_myRotationAllPackVisibleToggle.AddChangeCallback(delegate
		{
			OnChangeMyRotationAllPackVisible();
		});
		_myRotationAllPackVisibleToggle.SetValidator(ChangeMyRotationAllPackVisibleValidator);
		SetupDeckProperty(out var rawSkinId, out var isRandomLeaderSkin, out var leaderSkinIdList, out var sleeveId, out var isNotAvailableCard);
		if (EditDeckFormat != Format.Sealed)
		{
			DeckInfoTask task = new DeckInfoTask();
			task.SetParameter(Format.All);
			UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
			{
				_deckGroupListData = task.DeckGroupListData;
				List<DeckData> source = ((_conventionDeckList != null) ? new List<DeckData>(_conventionDeckList.DeckIdList.Select((int deckId) => _conventionDeckList.DeckList[deckId])) : _deckGroupListData.GetDeckListByFormat(base.Format));
				_otherDeckNames = (from deck in source
					where deck != null
					select deck.GetDeckName() into name
					where name != CurrentDeckName
					select name).ToList();
			}));
			while (!task.IsResultSuccess)
			{
				yield return null;
			}
		}
		SetupConventionView();
		m_craftOnBtn.normalSprite = "pilltab_02_right_off";
		m_craftOffBtn.normalSprite = "pilltab_02_left_on";
		_selectCardFilter.FormatState = EditDeckFormat;
		_selectCardFilter.IsAbleFormatFilter = false;
		_pagingFilter.FormatState = EditDeckFormat;
		UIBase_CardManager.FilterParameter param = new UIBase_CardManager.FilterParameter
		{
			FixedClassSet = ClassSet
		};
		_deckCardBundle.FilterParameter = _pagingFilter.GetFilterParameter(param);
		string text = CurrentDeckName;
		if (CurrentDeckName == null && CurrentDeckData.IsNoCard())
		{
			text = DeckUtil.CreateDefaultDeckName(ClassSet, base.FormatBehavior.UseSubClass, _otherDeckNames, EditDeckFormat, _myRotationInfo);
			_isDefaultDeckName = true;
		}
		_deckCardBundle.Setup(text, CurrentDeckData, ClassSet, rawSkinId, isRandomLeaderSkin, leaderSkinIdList, sleeveId, CopySrcDeckData, EditDeckFormat, _conventionDeckList, _deckGroupListData);
		while (!_deckCardBundle.IsReady)
		{
			yield return null;
		}
		string sleevePath = sleeveId.ToString();
		Sleeve sleeveData = Data.Master.SleeveMgr.Get(sleeveId);
		_resourcePathList = GetResourcePathList(sleevePath, sleeveData);
		yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(_resourcePathList, null));
		m_sleeveOriginal.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(sleevePath, ResourcesManager.AssetLoadPathType.SleeveTexture, isfetch: true));
		if (sleeveData.IsPremiumSleeve)
		{
			UIManager.GetInstance().getUIBase_CardManager().SetSleeveTexture(m_sleeveOriginal, sleeveData.sleeve_id);
		}
		if (base.Format == Format.MyRotation)
		{
			SetMyRotationData(_myRotationInfo);
		}
		bool isEffectSetupEnd = false;
		SetupEffect(delegate
		{
			isEffectSetupEnd = true;
		});
		while (!isEffectSetupEnd)
		{
			yield return null;
		}
		SetupDeckViewerAndText();
		if (_formatBehavior.IsShowFirstTipsAtDeckEdit)
		{
			UIManager.GetInstance().CheckFirstTips(FirstTips.TipsType.Deck);
		}
		CountDeckCardNum();
		SetupState();
		SetupSimpleDetail();
		HideDetail();
		HideDeckViewer();
		if (IsCreateAuto)
		{
			CreateAutoDeck(forceClear: false);
			while (IsCreateAuto)
			{
				yield return null;
			}
		}
		if (IsCreatedByBuilder && _shortageIdList.Count > 0)
		{
			ShowShortageCardViewer(delegate
			{
				UIManager.GetInstance().OnReadyViewScene(isFadein: true, OnFinishFadeIn);
			});
		}
		else
		{
			UIManager.GetInstance().OnReadyViewScene(isFadein: true, OnFinishFadeIn);
		}
		while (!_fadeInFinish)
		{
			yield return null;
		}
		if (_formatBehavior.IsShowFirstTipsAtDeckEdit)
		{
			while (FirstTips.IsFirstTipsOpen(FirstTips.TipsType.Deck))
			{
				yield return null;
			}
		}
		if (isNotAvailableCard)
		{
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetText(Data.SystemText.Get("Card_0188"));
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		}
	}

	private void SetupDeckViewerAndText()
	{
		m_labelDeckName.text = _deckCardBundle.DeckName;
		m_labelClassName.text = ((int)ClassSet.MainClass).ToString(); // Pre-Phase-5b: no clan-name lookup
		_deckViewer.RemoveData();
		_deckViewer.SetDeckName(_deckCardBundle.DeckName);
		_deckViewer.SetClassSet(ClassSet);
		_deckViewer.SetFormat(EditDeckFormat, _conventionDeckList);
		_redetherNum.text = PlayerStaticData.UserRedEtherCount.ToString();
	}

	private List<string> GetResourcePathList(string sleevePath, Sleeve sleeveData)
	{
		_resourcePathList = new List<string>();
		_resourcePathList.Add(Toolbox.ResourcesManager.GetAssetTypePath(sleevePath, ResourcesManager.AssetLoadPathType.SleeveTexture));
		if (sleeveData.IsPremiumSleeve)
		{
			UIManager.GetInstance().getUIBase_CardManager().AddPremireSleevePath(ref _resourcePathList, sleeveData);
		}
		_resourcePathList.AddRange(GetEffectAssetPathList());
		return _resourcePathList;
	}

	private void SetupDeckProperty(out int rawSkinId, out bool isRandomLeaderSkin, out List<int> leaderSkinIdList, out long sleeveId, out bool isNotAvailableCard)
	{
		if (CopySrcDeckData != null)
		{
			_hasChanged = true;
			ClassSet = new ClassSet(CopySrcDeckData.GetDeckClassID(), CopySrcDeckData.GetDeckSubClassID());
			if (EditDeckFormat == Format.Crossover && !IsCopySubClass)
			{
				CopySrcDeckData.ExtractMainClassAndNeutralCards();
			}
			List<int> cardIdList = CopySrcDeckData.GetCardIdList();
			if (ConventionInfo != null)
			{
				CopySrcDeckData.SetCardIdList(AccordDeck(cardIdList));
			}
			else if (IsCreatedByBuilder)
			{
				bool isFoilPreferred = Data.Load.data._userConfig.IsFoilPreferred;
				bool isPrizePreferred = Data.Load.data._userConfig.IsPrizePreferred;
				if (isFoilPreferred && isPrizePreferred)
				{
					CopySrcDeckData.SetCardIdList(AccordDeckPremiumAndPrize(cardIdList));
				}
				else if (isPrizePreferred)
				{
					CopySrcDeckData.SetCardIdList(AccordDeckPrize(cardIdList));
				}
				else if (isFoilPreferred)
				{
					CopySrcDeckData.SetCardIdList(AccordDeckPremium(cardIdList));
				}
				else
				{
					CopySrcDeckData.SetCardIdList(AccordDeck(cardIdList));
				}
			}
			isNotAvailableCard = IsNotAvailableCardInIds(cardIdList, EditDeckFormat, base.FormatBehavior, _myRotationInfo);
			rawSkinId = (IsCopySkinAndSleeve ? CopySrcDeckData.GetRawSkinId() : 0);
			isRandomLeaderSkin = IsCopySkinAndSleeve && CopySrcDeckData.IsSkinRandom;
			bool flag = IsCopySkinAndSleeve && CopySrcDeckData.SelectRandomSkinIdList != null && CopySrcDeckData.SelectRandomSkinIdList.Count > 0;
			leaderSkinIdList = (flag ? CopySrcDeckData.SelectRandomSkinIdList : new List<int> { 0 });
			sleeveId = (IsCopySkinAndSleeve ? Toolbox.ResourcesManager.GetExistingSleeveId(CopySrcDeckData.GetDeckSleeveID()) : 3000011);
		}
		else
		{
			isNotAvailableCard = false;
			rawSkinId = CurrentDeckData.GetRawSkinId();
			isRandomLeaderSkin = CurrentDeckData.IsSkinRandom;
			bool flag2 = CurrentDeckData.SelectRandomSkinIdList != null && CurrentDeckData.SelectRandomSkinIdList.Count > 0;
			leaderSkinIdList = (flag2 ? CurrentDeckData.SelectRandomSkinIdList : new List<int> { 0 });
			sleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(CurrentDeckData.GetDeckSleeveID());
		}
	}

	private void SetupConventionView()
	{
		if (ConventionInfo != null)
		{
			m_craftOnBtn.gameObject.SetActive(value: false);
			m_craftOffBtn.gameObject.SetActive(value: false);
			_redetherNum.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void InitFormatDependency()
	{
		InitializeBase(EditDeckFormat, _conventionDeckList);
		_formatBehavior = base.FormatBehavior;
		bool isCraftableCardAtDeckEdit = _formatBehavior.IsCraftableCardAtDeckEdit;
		UIManager.GetInstance().setBackScene(base.gameObject, _formatBehavior.DeckEditBackScene);
		_formatIcon.spriteName = _formatBehavior.SmallIconSpriteName;
		m_labelDeckName.gameObject.SetActive(_formatBehavior.IsShowDeckName);
		if (!_formatBehavior.IsShowDeckName)
		{
			UIUtil.AddPositionY(m_labelClassName.transform, 21f);
			UIUtil.AddPositionY(_formatIcon.transform, 21f);
			UIUtil.AddPositionY(m_labelDeckCardNum.transform.parent, 21f);
			UIUtil.AddPositionY(m_labelCharCardNum.transform.parent.parent, 18f);
		}
		m_autoCreateButton.gameObject.SetActive(_formatBehavior.IsShowAutoDeckCreateButtonAtDeckEdit);
		m_craftOnBtn.gameObject.SetActive(isCraftableCardAtDeckEdit);
		m_craftOffBtn.gameObject.SetActive(isCraftableCardAtDeckEdit);
		_redetherNum.transform.parent.gameObject.SetActive(isCraftableCardAtDeckEdit);
		base.IsShowCardDetailCraftPanel = isCraftableCardAtDeckEdit;
		_cardDetail = UnityEngine.Object.Instantiate(m_cardDetailPrefab);
		_cardDetail.transform.parent = base.transform;
		_cardDetail.transform.localPosition = Vector3.zero;
		_cardDetail.transform.localScale = Vector3.one;
		_cardDetail.gameObject.SetActive(value: false);
		_cardDetail.Initialize(_cardDetail.gameObject.layer, base.FormatBehavior.CardMasterId);
		_cardDetail.IsShowFlavorTextButton = true;
		_cardDetail.IsShowVoiceButton = true;
		_cardDetail.IsShowEvolutionButton = true;
		_deckViewer.Init(_deckViewer.gameObject, _cardDetail, "", HideDeckViewer, "Detail", in_DetailCameraUse: false, null, _formatBehavior.DeckCardNumMax);
		_deckViewer.SubmitDeckType = _formatBehavior.DeckCodeType;
		bool flag = _formatBehavior.DeckCardNumMin == _formatBehavior.DeckCardNumMax;
		_labelDeckCardMaxNum.gameObject.SetActive(flag);
		if (flag)
		{
			_labelDeckCardMaxNum.text = "/" + _formatBehavior.DeckCardNumMax;
		}
		InitUseSubClassDisplay();
	}

	protected override void OnFinishFadeIn()
	{
		_fadeInFinish = true;
		base.OnFinishFadeIn();
		IsCreatedByBuilder = false;
	}

	public static bool IsNotAvailableCardInIds(List<int> cardIds, Format format, IFormatBehavior formatBehavior, MyRotationInfo myRotationInfo)
	{
		CardMaster master = CardMaster.GetInstance(formatBehavior.CardMasterId);
		List<int> source = cardIds.ConvertAll((int id) => master.GetCardParameterFromId(id).BaseCardId);
		List<int> list = source.Distinct().ToList();
		for (int num = 0; num < list.Count; num++)
		{
			CardParameter param = master.GetCardParameterFromId(list[num]);
			ClassType classType = ClassUtil.GetClassType(param, format, ClassSet);
			if (!param.IsAvailableFormat(format, classType, myRotationInfo))
			{
				return true;
			}
			if (source.Count((int entry) => entry == param.BaseCardId) > param.GetSameKindNumMaxInFormat(format, formatBehavior, classType, myRotationInfo))
			{
				return true;
			}
		}
		return false;
	}

	public void ChangeCraftMode(bool isCraft)
	{
		if (!base.IsLoading && base.CurrentState == _stateEdit)
		{
			base.IsLoading = true;
			m_craftOnBtn.normalSprite = (isCraft ? "pilltab_02_right_on" : "pilltab_02_right_off");
			m_craftOffBtn.normalSprite = (isCraft ? "pilltab_02_left_off" : "pilltab_02_left_on");

			_deckCardBundle.ChangeCraftMode(isCraft);
		}
	}

	public void CreateAutoDeck(bool forceClear)
	{
		_isDisableTouchWhileLoading = true;
		base.IsLoading = true;
		m_costCurve.Refresh();
		DeckCardNum = 0;
		_hasChanged = true;
		int tournamentId = 0;
		if (ConventionInfo != null && base.FormatBehavior.IsConventionMode)
		{
			tournamentId = int.Parse(ConventionInfo.Id);
		}
		_deckCardBundle.CreateAutoDeck(forceClear, tournamentId, _myRotationInfo);
	}

	private void RecreateDeckAddCard(List<int> addIdList)
	{
		_isDisableTouchWhileLoading = true;
		base.IsLoading = true;
		m_costCurve.Refresh();
		DeckCardNum = 0;
		if (CanUseNonPossessionCard)
		{
			addIdList = new List<int>();
		}
		_hasChanged = true;
		_deckCardBundle.CreateDeckAddCard(addIdList);
	}

	private void RecreateDeckRemoveCard(int removeId)
	{
		_isDisableTouchWhileLoading = true;
		base.IsLoading = true;
		m_costCurve.Refresh();
		DeckCardNum = 0;
		if (CanUseNonPossessionCard)
		{
			removeId = -1;
		}
		_hasChanged = true;
		_deckCardBundle.CreateDeckRemoveCard(removeId);
	}

	private void SaveDeck()
	{
		Action<CardBundleController> deckSaveFunc = _formatBehavior.DeckSaveFunc;
		if (deckSaveFunc != null)
		{
			deckSaveFunc(_deckCardBundle);
			return;
		}
		Action<bool> saveCompleteDialogAction = delegate(bool isUsable)
		{
			Action changeSceneToDeckList = delegate
			{
				DeckListUI.ChangeSceneToDeckList(EditDeckFormat, null, ConventionInfo);
			};
			Action<Action> closeAction = delegate(Action changeSceneAction)
			{
				DeckUpdateTask deckUpdateTask = null;
				bool flag = false;
				if (ConventionInfo == null)
				{
					deckUpdateTask = null; // Pre-Phase-5b: no DeckUpdateTask headless
					flag = deckUpdateTask.AchievedInfo._rewards.Count > 0;
				}
				if (flag)
				{
					UIManager.GetInstance().createInSceneCenterLoading();
					DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
					dialogBase.SetSize(DialogBase.Size.M);
					dialogBase.SetTitleLabel(Data.SystemText.Get("Story_0029"));
					dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
					dialogBase.OnClose = changeSceneAction;
					RewardBase component = NGUITools.AddChild(dialogBase.gameObject, _rewardBase.gameObject).GetComponent<RewardBase>();
					for (int i = 0; i < deckUpdateTask.AchievedInfo._rewards.Count; i++)
					{
						component.AddReward(deckUpdateTask.AchievedInfo._rewards[i]);
					}
					component.EndCreate();
				}
				else
				{
					changeSceneAction();
				}
			};
			DialogBase saveCompleteDialog = UIManager.GetInstance().CreateDialogClose();
			saveCompleteDialog.OnClose = delegate
			{
				closeAction(changeSceneToDeckList);
			};
			if (IsBattleRetry && isUsable)
			{
				SystemText systemText = Data.SystemText;
				saveCompleteDialog.SetTitleLabel(Data.SystemText.Get("Card_0246"));
				saveCompleteDialog.SetText(GetSaveAndRetryDialogText());
				saveCompleteDialog.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
				saveCompleteDialog.SetButtonText(systemText.Get("Card_0247"), systemText.Get("Card_0248"));
				saveCompleteDialog.onPushButton1 = delegate
				{
					saveCompleteDialog.OnClose = delegate
					{
						closeAction(ChangeSceneToBattle);
					};
				};
			}
			else
			{
				saveCompleteDialog.SetTitleLabel(Data.SystemText.Get("Dia_DeckEdit_007_Title"));
				saveCompleteDialog.SetText(Data.SystemText.Get("Card_0019"));
				saveCompleteDialog.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			}
		};
		_deckCardBundle.SaveDeck(null, saveCompleteDialogAction, _needsClearSkin);
	}

	private void ChangeSceneToBattle()
	{
		DataMgr dataManager = null; // Pre-Phase-5b: headless has no DataMgr
		DeckInfoTask task = new DeckInfoTask();
		task.SetParameter(Format.All);
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			DeckData deckByAttribute = task.DeckGroupListData.GetDeckByAttribute(DeckAttributeType.CustomDeck, _deckCardBundle.DeckId, base.Format);
			switch (dataManager.m_BattleType)
			{
			case DataMgr.BattleType.FreeBattle:
			case DataMgr.BattleType.RankBattle:
				FreeAndRankMatchDeckSelectConfirmDialog.DecideDeck(deckByAttribute, isBattleEnd: false, notBlack: true, notCollider: true);
				break;
			case DataMgr.BattleType.Quest:
				QuestDeckSelectConfirmDialog.DecideDeck(deckByAttribute, isBattleAgain: false);
				break;
			}
		}));
	}

	private string GetSaveAndRetryDialogText()
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: DataMgr not reachable in DeckCardEdit UI headless
		switch (dataMgr.m_BattleType)
		{
		case DataMgr.BattleType.FreeBattle:
			return Data.SystemText.Get("Card_0242");
		case DataMgr.BattleType.RankBattle:
			return Data.SystemText.Get("Card_0243");
		case DataMgr.BattleType.Story:
			return Data.SystemText.Get("Card_0240");
		case DataMgr.BattleType.Practice:
			return Data.SystemText.Get("Card_0241");
		case DataMgr.BattleType.Quest:
			if (dataMgr.QuestBattleData.IsMockBattle)
			{
				return Data.SystemText.Get("Quest_0027");
			}
			return Data.SystemText.Get("Card_0249");
		case DataMgr.BattleType.BossRushQuest:
			if (!dataMgr.BossRushBattleData.IsMockBattle)
			{
				return Data.SystemText.Get("Card_0249");
			}
			return Data.SystemText.Get("Quest_0027");
		default:
			return string.Empty;
		}
	}

	private string GetRetryDialogText()
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: DataMgr not reachable in DeckCardEdit UI headless
		switch (dataMgr.m_BattleType)
		{
		case DataMgr.BattleType.FreeBattle:
			return Data.SystemText.Get("Card_0238");
		case DataMgr.BattleType.RankBattle:
			return Data.SystemText.Get("Card_0239");
		case DataMgr.BattleType.Story:
			return Data.SystemText.Get("Card_0236");
		case DataMgr.BattleType.Practice:
			return Data.SystemText.Get("Card_0237");
		case DataMgr.BattleType.Quest:
			if (dataMgr.QuestBattleData.IsMockBattle)
			{
				return Data.SystemText.Get("Quest_0028");
			}
			return Data.SystemText.Get("Card_0250");
		default:
			return string.Empty;
		}
	}

	public int CountDeckCardNum()
	{
		DeckCardNum = _deckCardBundle.SelectionAreaList.CountSum;
		if (_formatBehavior.UseSubClass)
		{
			int num = _deckCardBundle.SelectionAreaList.CountClassCard(ClassSet.MainClass);
			int num2 = _deckCardBundle.SelectionAreaList.CountClassCard(ClassSet.SubClass);
			SetDeckCardNumLabel(_useSubClassDeckCardNumLabel, DeckCardNum, IsDeckCardNumToRed(DeckCardNum));
			SetDeckCardNumLabel(_useSubClassMainClassCardNumLabel, num, isRed: false);
			SetDeckCardNumLabel(_useSubClassSubClassCardNumLabel, num2, isRed: false);
		}
		else
		{
			SetDeckCardNumLabel(m_labelDeckCardNum, DeckCardNum, IsDeckCardNumToRed(DeckCardNum));
		}
		m_costCurve.Refresh(_deckCardBundle.SelectionAreaList.IdList.ToArray());
		return DeckCardNum;
	}

	protected override void AccordCardInfo()
	{
		base.AccordCardInfo();
		CountDeckCardNum();
		_redetherNum.text = PlayerStaticData.UserRedEtherCount.ToString();
		UpdateCraftShortageButton();
	}

	private int GetDeckCardCount()
	{
		return _deckCardBundle.SelectionAreaList.CountSum;
	}

	private int GetMainClassCardCount()
	{
		return _deckCardBundle.SelectionAreaList.CountClassCard(ClassSet.MainClass);
	}

	private int GetSubClassCardCount()
	{
		return _deckCardBundle.SelectionAreaList.CountClassCard(ClassSet.SubClass);
	}

	private void DeckCardNumAnim()
	{
		DeckCardNum = _deckCardBundle.SelectionAreaList.CountSum;
		if (_formatBehavior.UseSubClass)
		{
			PlayCardNumAnimation(_useSubClassDeckCardNumLabel, IsDeckCardNumToRed(DeckCardNum), GetDeckCardCount);
			if (int.TryParse(_useSubClassMainClassCardNumLabel.text, out var result) && _deckCardBundle.SelectionAreaList.CountClassCard(ClassSet.MainClass) != result)
			{
				PlayCardNumAnimation(_useSubClassMainClassCardNumLabel, isRed: false, GetMainClassCardCount);
			}
			if (int.TryParse(_useSubClassSubClassCardNumLabel.text, out var result2) && _deckCardBundle.SelectionAreaList.CountClassCard(ClassSet.SubClass) != result2)
			{
				PlayCardNumAnimation(_useSubClassSubClassCardNumLabel, isRed: false, GetSubClassCardCount);
			}
		}
		else
		{
			PlayCardNumAnimation(m_labelDeckCardNum, IsDeckCardNumToRed(DeckCardNum), GetDeckCardCount);
		}
	}

	private void PlayCardNumAnimation(UILabel originLabel, bool isRed, Func<int> latestCardCount)
	{
		Transform transform = originLabel.transform;
		UILabel cloneLabel = UnityEngine.Object.Instantiate(originLabel, transform.position, transform.rotation, transform.parent);
		cloneLabel.transform.localScale = transform.localScale;
		cloneLabel.depth = originLabel.depth + 1;
		cloneLabel.alpha = 0f;
		TweenAlpha anim = TweenAlpha.Begin(originLabel.gameObject, 0.2f, 0f);
		TweenAlpha.Begin(cloneLabel.gameObject, 0.3f, 1f).onFinished.Add(new EventDelegate(delegate
		{
			UnityEngine.Object.Destroy(anim);
			originLabel.alpha = 1f;
			SetDeckCardNumLabel(originLabel, latestCardCount.Call(), isRed);
			UnityEngine.Object.Destroy(cloneLabel.gameObject);
		}));
		SetDeckCardNumLabel(cloneLabel, latestCardCount.Call(), isRed);
	}

	private bool IsDeckCardNumToRed(int num)
	{
		if (!_formatBehavior.IsEmphasizeDeckCardShortage || num >= _formatBehavior.DeckCardNumMin)
		{
			if (_formatBehavior.IsEmphasizeDeckCardOverage)
			{
				return num > _formatBehavior.DeckCardNumMax;
			}
			return false;
		}
		return true;
	}

	private void SetDeckCardNumLabel(UILabel label, int num, bool isRed)
	{
		label.text = num.ToString();
		label.color = (isRed ? LabelDefine.TEXT_COLOR_RED : LabelDefine.TEXT_COLOR_NORMAL);
	}

	private void OnCreateDeckSleeve()
	{
		_stateEdit.RefreshSelectionArea(isImmediate: false);
	}

	private void OnCreateDeckCard()
	{
		CountDeckCardNum();
		base.IsLoading = false;
		_isDisableTouchWhileLoading = false;
		_stateEdit.RefreshSelectionArea(isImmediate: true);
		UpdateCraftShortageButton();
		_enableFitOnChangeDeckCardNumChange = true;
	}

	protected override void OnCreatePagingSleeve()
	{
		CountDeckCardNum();
		base.OnCreatePagingSleeve();
	}

	private void OnInsertDeckCard(int id)
	{
		m_costCurve.Add(id, withAnim: true);
		_hasChanged = true;
		DeckCardNumAnim();
		UpdateCraftShortageButton();
	}

	private void OnRemoveDeckCard(int id)
	{
		m_costCurve.Sub(id, withAnim: true);
		_hasChanged = true;
		DeckCardNumAnim();
		UpdateCraftShortageButton();
	}

	private void OnClickAutoDeckBtn(GameObject obj)
	{

		if (Data.MaintenanceCodeList.Contains(NetworkDefine.MAINTENANCE_TYPE.AUTO_DECK_CREATE))
		{
			DialogBase dialogBase = UIManager.GetInstance().CreateConfirmationDialog(Data.SystemText.Get("Card_0266"));
			dialogBase.SetPanelDepth(2000);
			dialogBase.SetSize(DialogBase.Size.M);
			return;
		}
		bool forceClear = false;
		if (EditDeckFormat == Format.Crossover)
		{
			forceClear = !CanAutoCreateCrossoverDeck();
		}
		if (EditDeckFormat == Format.MyRotation)
		{
			forceClear = NeedMyRotationAutoCreateDeckClear();
		}
		bool flag = base.SelectionAreaList.IdList.Count <= _formatBehavior.DeckCardNumMax;
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase2 = UIManager.GetInstance().CreateDialogClose();
		dialogBase2.SetTitleLabel(systemText.Get("Dia_DeckEdit_010_Title"));
		string text = systemText.Get("Card_0294");
		if (EditDeckFormat == Format.MyRotation)
		{
			text = systemText.Get("MyRotation_ID_07");
		}
		dialogBase2.SetText((flag && forceClear) ? text : systemText.Get("Card_0092"));
		dialogBase2.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase2.SetButtonText(systemText.Get("Dia_DeckEdit_010_Button"));
		dialogBase2.onPushButton1 = delegate
		{
			HideDetail();
			_stateEdit.RemoveSelectionArea();
			CreateAutoDeck(forceClear);
		};
	}

	private void OnCreateAutoDeck()
	{
		SystemText systemText = Data.SystemText;
		_stateEdit.RefreshSelectionArea(isImmediate: true);
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_011_Title"));
		dialogBase.SetText(systemText.Get("Card_0082"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		base.IsLoading = false;
		IsCreateAuto = false;
	}

	private void OnClickSaveBtn(GameObject obj)
	{
		SaveDeck();

	}

	private DeckData CreateDeckData()
	{
		DeckData deckData = new DeckData(EditDeckFormat);
		deckData.SetDeckClassID((int)ClassSet.MainClass);
		deckData.SetDeckSubClassID((int)ClassSet.SubClass);
		if (base.SelectionAreaList.IdList.Count > 0)
		{
			deckData.SetCardIdList(base.SelectionAreaList.IdList);
		}
		else
		{
			deckData.SetEmptyCardIdList();
		}
		deckData.SetDeckName(_deckCardBundle.DeckName);
		if (_myRotationInfo != null)
		{
			deckData.MyRotationId = _myRotationInfo.Id;
		}
		return deckData;
	}

	private void ShowDeckViewer()
	{
		HideDetail();
		_deckViewer.gameObject.SetActive(value: true);
		_deckViewer.SetDeck(CreateDeckData(), _conventionDeckList);
		if (_formatBehavior.CanShowQRCode)
		{
			_deckViewer.SetQRCodeButtonToGray();
		}

	}

	private void HideDeckViewer()
	{
		_deckViewer.RemoveData();
		_deckViewer.gameObject.SetActive(value: false);
	}

	private void OnClickCraftShortageCardButton(GameObject obj)
	{

		if (!CanUseNonPossessionCard || base.IsLoading)
		{
			return;
		}
		_shortageIdList = new List<int>();
		CardMaster instance = CardMaster.GetInstance(base.FormatBehavior.CardMasterId);
		foreach (CardObject card in base.SelectionAreaList.CardList)
		{
			if (card.IsNonPossessionCard)
			{
				CardParameter cardParameterFromId = instance.GetCardParameterFromId(card.CardId);
				if (cardParameterFromId.IsAvailableFormat(classType: ClassUtil.GetClassType(cardParameterFromId, base.Format, ClassSet), inFormat: base.Format, myRotationInfo: _myRotationInfo))
				{
					_shortageIdList.AddRange(Enumerable.Repeat(card.CardId, card.MainCardNum));
				}
			}
		}
		if (_shortageIdList.Count != 0)
		{
			HideDetail();
			ShowShortageCardViewer(null);
		}
	}

	private void UpdateCraftShortageButton()
	{
		CardMaster cardMaster = CardMaster.GetInstance(base.FormatBehavior.CardMasterId);
		UIManager.SetObjectToGrey(_craftShortageCardButton.gameObject, !base.SelectionAreaList.CardList.Any(delegate(CardObject c)
		{
			if (c.IsNonPossessionCard)
			{
				CardParameter cardParameterFromId = cardMaster.GetCardParameterFromId(c.CardId);
				return cardParameterFromId.IsAvailableFormat(classType: ClassUtil.GetClassType(cardParameterFromId, base.Format, ClassSet), inFormat: base.Format, myRotationInfo: _myRotationInfo);
			}
			return false;
		}));
	}

	private void ShowShortageCardViewer(Action callback)
	{
		UIManager.GetInstance().MyPageUICameraObj.SetActive(value: true);
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.gameObject.layer = LayerMask.NameToLayer("MyPage");
		dialogBase.SetBackViewLayer(LayerMask.NameToLayer("MyPage"));
		dialogBase.SetSize(DialogBase.Size.XL);
		dialogBase.SetTitleLabel(Data.SystemText.Get("Dia_DeckEdit_014_Title"));
		dialogBase.SetPanelDepth(100);
		dialogBase.OpenSe = 0;
		DeckBuildShortageCardView _shortageCardView = UnityEngine.Object.Instantiate(_prefabShortageCardView);
		dialogBase.SetObj(_shortageCardView.gameObject);
		dialogBase.OnCloseStart = CloseShortageCardViewer;
		UIManager.GetInstance().AttachAtlas(_shortageCardView.gameObject);
		_cardDetail.IsShortageUI = true;
		_cardDetail.IsOwnCardNum = true;
		_cardDetail.IsShowCraftButtons = true;
		_cardDetail.OnCardBuy = delegate
		{
			int cardId = _cardDetail.CardData.CardId;
			if (_shortageIdList.Contains(cardId))
			{
				RecreateDeckAddCard(new List<int> { cardId });
				_shortageCardView.RemoveCardNum(cardId, 1);
				_shortageIdList.Remove(cardId);
			}
			AccordCardInfo();
			_deckCardBundle.InvalidateFilteredIdListCache();
			FetchPagingCard();
			_shortageCardView.UpdateExplainText();
		};
		_cardDetail.OnCardSellId = delegate(int id)
		{
			RecreateDeckRemoveCard(id);
			_shortageCardView.AddCardNum(id, 1);
			_shortageIdList.Add(id);
			AccordCardInfo();
			_deckCardBundle.InvalidateFilteredIdListCache();
			FetchPagingCard();
			_shortageCardView.UpdateExplainText();
		};
		_shortageCardView.Init(_shortageIdList, _cardDetail, dialogBase, callback, OnCraftShortageCard, base.FormatBehavior.CardMasterId, DeactivateMyPageCamera);
	}

	private void OnCraftShortageCard()
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		SystemText systemText = Data.SystemText;
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_015_Title"));
		dialogBase.SetText(systemText.Get("Dia_DeckEdit_015_Body"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		List<int> addIdList = _shortageIdList.FindAll(delegate(int id)
		{
			if (false /* Pre-Phase-5b: IsMaintenanceCard headless-false */)
			{
				return false;
			}
			return !CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(id).IsNotCraftDestruct;
		});
		RecreateDeckAddCard(addIdList);
		_deckCardBundle.InvalidateFilteredIdListCache();
		FetchPagingCard();
		_redetherNum.text = PlayerStaticData.UserRedEtherCount.ToString();
	}

	private void CloseShortageCardViewer()
	{
		_cardDetail.IsShortageUI = false;
		_cardDetail.IsOwnCardNum = false;
		_cardDetail.IsShowCraftButtons = false;
		_cardDetail.OnCardBuy = null;
		_cardDetail.OnCardSellId = null;
	}

	protected override void OnLiquefy(int cardId)
	{
		ReplaceLiquefiedCardInSelectionArea(cardId);
		base.OnLiquefy(cardId);
	}

	private void ReplaceLiquefiedCardInSelectionArea(int cardId)
	{
		List<int> idList = base.SelectionAreaList.IdList;
		if (!idList.Contains(cardId) || idList.Count((int id) => id == cardId) <= base.FormatBehavior.GetPossessionCardNum(cardId, _isEnableSpotCard))
		{
			return;
		}
		CardMaster instance = CardMaster.GetInstance(base.FormatBehavior.CardMasterId);
		CardParameter cardParameterFromId = instance.GetCardParameterFromId(cardId);
		int normalCardId = cardParameterFromId.NormalCardId;
		int baseCardId = cardParameterFromId.BaseCardId;
		IEnumerable<CardParameter> enumerable = from c in instance.GetAllParameters()
			where c.BaseCardId == baseCardId && !c.IsFoil && c.CardId != normalCardId && c.CardId != baseCardId
			select c;
		IOrderedEnumerable<CardParameter> orderedEnumerable = from c in enumerable
			where c.IsPrizeCard
			orderby c.CardId descending
			select c;
		IOrderedEnumerable<CardParameter> source = from c in enumerable.Except(orderedEnumerable)
			orderby cardId descending
			select c;
		CardParameter cardParameterFromId2 = instance.GetCardParameterFromId(baseCardId);
		List<int> list = new List<int>();
		if (Data.Load.data._userConfig.IsFoilPreferred)
		{
			list.Add(cardParameterFromId.FoilCardId);
			list.AddRange(source.Select((CardParameter c) => c.FoilCardId));
			list.Add(cardParameterFromId2.FoilCardId);
			list.AddRange(orderedEnumerable.Select((CardParameter c) => c.FoilCardId));
			list.Add(cardParameterFromId.NormalCardId);
			list.AddRange(source.Select((CardParameter c) => c.NormalCardId));
			list.Add(cardParameterFromId2.NormalCardId);
			list.AddRange(orderedEnumerable.Select((CardParameter c) => c.NormalCardId));
		}
		else
		{
			list.Add(cardParameterFromId.NormalCardId);
			list.Add(cardParameterFromId.FoilCardId);
			list.AddRange(source.Select((CardParameter c) => c.NormalCardId));
			list.AddRange(source.Select((CardParameter c) => c.FoilCardId));
			list.Add(cardParameterFromId2.NormalCardId);
			list.Add(cardParameterFromId2.FoilCardId);
			list.AddRange(orderedEnumerable.Select((CardParameter c) => c.NormalCardId));
			list.AddRange(orderedEnumerable.Select((CardParameter c) => c.FoilCardId));
		}
		int num = -1;
		foreach (int replaceCardIdChoice in list)
		{
			CardParameter cardParameterFromId3 = instance.GetCardParameterFromId(replaceCardIdChoice);
			if (cardParameterFromId3.IsAvailableFormat(classType: ClassUtil.GetClassType(cardParameterFromId3, base.Format, ClassSet), inFormat: base.Format, myRotationInfo: _myRotationInfo) && idList.Count((int id) => id == replaceCardIdChoice) < base.FormatBehavior.GetPossessionCardNum(replaceCardIdChoice, _isEnableSpotCard))
			{
				num = replaceCardIdChoice;
				break;
			}
		}
		if (CanUseNonPossessionCard && num == -1)
		{
			num = GetNewestCraftableCardId(cardParameterFromId.BaseCardId, _formatBehavior);
		}
		idList.Remove(cardId);
		if (num != -1)
		{
			idList.Add(num);
		}
		_deckCardBundle.ReloadDeckCard(idList);
	}

	private void DeactivateMyPageCamera()
	{
		UIManager.GetInstance().MyPageUICameraObj.SetActive(value: false);
	}

	public override bool IsGetOutOfScene(Action backupExec)
	{
		if (_deckViewer.isActiveAndEnabled)
		{
			HideDeckViewer();

			return false;
		}
		if (_hasChanged && DeckCardNum > 0)
		{
			AskForSave(backupExec);
			return false;
		}
		if (IsBattleRetry && !_hasChanged && GetDeck(base.Format, _deckCardBundle.DeckId).IsUsable())
		{
			AskForBattleRetry(backupExec);
			return false;
		}
		return base.IsGetOutOfScene(backupExec);
	}

	private void AskForBattleRetry(Action onCloseDialog)
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Card_0246"));
		dialogBase.SetText(GetRetryDialogText());
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(systemText.Get("Card_0247"), systemText.Get("Card_0248"));
		dialogBase.onPushButton1 = ChangeSceneToBattle;
		dialogBase.onPushButton2 = onCloseDialog;
		dialogBase.onCloseWithoutSelect = onCloseDialog;
	}

	public void AskForSave(Action onCloseDialog)
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_012_Title"));
		dialogBase.SetText(systemText.Get("Card_0087"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(systemText.Get("Dia_DeckEdit_012_Button"), systemText.Get("Dia_DeckEdit_012_Button_2"));
		dialogBase.onPushButton1 = SaveDeck;
		dialogBase.onPushButton2 = onCloseDialog;
	}

	private List<int> AccordDeck(List<int> copyDeckCardList)
	{
		List<int> list = new CopyDeckFormation(copyDeckCardList, _formatBehavior).EditNormal();
		if (CanUseNonPossessionCard || list.Count < copyDeckCardList.Count)
		{
			AddShortageCardList(copyDeckCardList);
		}
		return list;
	}

	private List<int> AccordDeckPrize(List<int> deckCardList)
	{
		List<int> list = new CopyDeckFormation(deckCardList, _formatBehavior).EditPrize();
		if (CanUseNonPossessionCard || list.Count < deckCardList.Count)
		{
			AddShortageCardList(deckCardList);
		}
		return list;
	}

	private List<int> AccordDeckPremiumAndPrize(List<int> deckCardList)
	{
		List<int> list = new CopyDeckFormation(deckCardList, _formatBehavior).EditPremiumAndPrize();
		if (CanUseNonPossessionCard || list.Count < deckCardList.Count)
		{
			AddShortageCardList(deckCardList);
		}
		return list;
	}

	private List<int> AccordDeckPremium(List<int> deckCardList)
	{
		List<int> list = new CopyDeckFormation(deckCardList, _formatBehavior).EditPremium();
		if (CanUseNonPossessionCard || list.Count < deckCardList.Count)
		{
			AddShortageCardList(deckCardList);
		}
		return list;
	}

	public static List<int> GetShortageCardList(List<int> deckCardList, Format format, IFormatBehavior formatBehavior, MyRotationInfo myRotationInfo)
	{
		List<int> list = new List<int>();
		List<int> source = deckCardList.ConvertAll((int id) => CardMaster.GetInstance(formatBehavior.CardMasterId).GetCardParameterFromId(id).BaseCardId);
		List<int> needBaseIdList = source.Distinct().ToList();
		Dictionary<int, int> possessionBaseCardDictionary = new(); // Pre-Phase-5b: possession lookup not reachable headless
		int i;
		for (i = 0; i < needBaseIdList.Count; i++)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstance(formatBehavior.CardMasterId).GetCardParameterFromId(needBaseIdList[i]);
			ClassType classType = ClassUtil.GetClassType(cardParameterFromId, format, ClassSet);
			if (!cardParameterFromId.IsAvailableFormat(format, classType, myRotationInfo))
			{
				continue;
			}
			int sameKindNumMaxInFormat = cardParameterFromId.GetSameKindNumMaxInFormat(format, formatBehavior, classType, myRotationInfo);
			int num = source.Count((int id) => id == needBaseIdList[i]);
			int value = 0;
			possessionBaseCardDictionary.TryGetValue(needBaseIdList[i], out value);
			int num2 = num - value;
			if (num > sameKindNumMaxInFormat)
			{
				num2 -= num - sameKindNumMaxInFormat;
			}
			num2 = Mathf.Max(0, num2);
			if (num2 > 0)
			{
				int newestCraftableCardId = GetNewestCraftableCardId(cardParameterFromId.BaseCardId, formatBehavior);
				for (int num3 = 0; num3 < num2; num3++)
				{
					list.Add(newestCraftableCardId);
				}
			}
		}
		return list;
	}

	public static int GetNewestCraftableCardId(int baseCardId, IFormatBehavior formatBehavior)
	{
		CardMaster instance = CardMaster.GetInstance(formatBehavior.CardMasterId);
		List<int> sameCardListByBaseCardId = instance.GetSameCardListByBaseCardId(baseCardId);
		int num = baseCardId;
		foreach (int item in sameCardListByBaseCardId)
		{
			CardParameter cardParameterFromId = instance.GetCardParameterFromId(item);
			if (cardParameterFromId.CanCraft && cardParameterFromId.CardId > num)
			{
				num = cardParameterFromId.CardId;
			}
		}
		return num;
	}

	private void AddShortageCardList(List<int> deckCardList)
	{
		_shortageIdList.AddRange(GetShortageCardList(deckCardList, EditDeckFormat, base.FormatBehavior, _myRotationInfo));
	}

	public static bool IsSelectableNonPossessionCard(CardParameter cardParameter)
	{
		if (!cardParameter.IsFoil && !cardParameter.IsPrizeCard)
		{
			return cardParameter.CanCraft;
		}
		return false;
	}

	public static void SendConfigUpdateFoilPreferred(bool isFoilPreferred)
	{
		ConfigUpdateFoilPreferredTask configUpdateFoilPreferredTask = new ConfigUpdateFoilPreferredTask();
		configUpdateFoilPreferredTask.SetParameter(isFoilPreferred);
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(configUpdateFoilPreferredTask, delegate
		{
			Data.Load.data._userConfig.IsFoilPreferred = isFoilPreferred;
		}));
	}

	public static void SendConfigUpdatePrizePreferred(bool isPrizePreferred)
	{
		ConfigUpdatePrizePreferredTask configUpdatePrizePreferredTask = new ConfigUpdatePrizePreferredTask();
		configUpdatePrizePreferredTask.SetParameter(isPrizePreferred);
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(configUpdatePrizePreferredTask, delegate
		{
			Data.Load.data._userConfig.IsPrizePreferred = isPrizePreferred;
		}));
	}

	protected override int GetSameKindNumMaxInFormat(int cardId)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(cardId);
		return cardParameterFromId.GetSameKindNumMaxInFormat(classType: ClassUtil.GetClassType(cardParameterFromId, base.Format, ClassSet), inFormat: base.Format, behavior: base.FormatBehavior, myRotationInfo: _myRotationInfo);
	}

	private void OnClickSwapClassButton(GameObject obj)
	{

		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Card_0274"));
		dialogBase.SetText(Data.SystemText.Get("Card_0275"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(Data.SystemText.Get("Card_0276"));
		dialogBase.onPushButton1 = delegate
		{
			StartCoroutine(SwapClass());
		};
	}

	private void OnClickMyRotationChangeButton(GameObject g)
	{

		MyRotationPeriodSelectDialog.Create(_myRotationInfo, ClassSet.MainClass, delegate(MyRotationInfo selectData)
		{
			if (_myRotationInfo.Id != selectData.Id)
			{
				UpdateMyRotationInfo(selectData, isChangeAllCardView: false);
			}
		});
	}

	private void UpdateMyRotationInfo(MyRotationInfo info, bool isChangeAllCardView)
	{
		bool num = _myRotationInfo.Id != info.Id || isChangeAllCardView;
		_selectCardFilter.SetMyRotationData(info, FilterController.MyRotationFilterType.DECK, base.MyRotationFilterTypeCardPool == FilterController.MyRotationFilterType.CARD_POOL_ALL_PACK);
		_pagingFilter.SetMyRotationData(info, base.MyRotationFilterTypeCardPool, isAllBackVisible: false);
		_myRotationInfo = info;
		SetMyRotationData(info);
		_myRotationPackShortName.text = _myRotationInfo.LastPackText;
		UpdateCardDisplay();
		if (num)
		{
			_selectCardFilter.Reset(isEnableValidateCall: false);
			_pagingFilter.Reset(isEnableValidateCall: false);
		}
		OnValidateSelectionAreaFilter();
		OnValidatePagingFilter();
		if (_isDefaultDeckName)
		{
			_deckCardBundle.DeckName = DeckUtil.CreateDefaultDeckName(ClassSet, useSubClass: false, _otherDeckNames, EditDeckFormat, _myRotationInfo);
		}
		CardMaster.CardMasterId cardMasterId = base.FormatBehavior.CardMasterId;
		List<int> cardPool = UIManager.GetInstance().getUIBase_CardManager().SortIDList(CardMaster.GetInstance(cardMasterId).GetAllCardIds(), cardMasterId);
		_selectCardFilter.UpdateTypeFilterForDeckEdit(cardPool, ClassSet, EditDeckFormat, _myRotationInfo, base.MyRotationFilterTypeCardPool);
		_pagingFilter.UpdateTypeFilterForDeckEdit(cardPool, ClassSet, EditDeckFormat, _myRotationInfo, base.MyRotationFilterTypeCardPool);
		m_labelClassName.text = DeckData.CreateMyRotationClassName((int)ClassSet.MainClass, _myRotationInfo);
		_deckCardBundle.OnCreatePagingCard += UpdateCardDisplay;
		SetupDeckViewerAndText();
		UpdateMyRotationAbilityIcon();
	}

	private void UpdateMyRotationAbilityIcon()
	{
		_myRotationAbilityGrid.transform.DestroyChildren();
		foreach (MyRotationInfo.MyRotationBonus ability in _myRotationInfo.Abilities)
		{
			GameObject obj = NGUITools.AddChild(_myRotationAbilityGrid.gameObject, _myRotationIconOriginal);
			obj.GetComponent<UISprite>().spriteName = ability.IconName;
			obj.gameObject.SetActive(value: true);
		}
		_myRotationAbilityGrid.Reposition();
	}

	private void UpdateCardDisplay()
	{
		_deckCardBundle.UpdateCardDisplay();
		_deckCardBundle.OnCreatePagingCard -= UpdateCardDisplay;
		UpdateCraftShortageButton();
	}

	private IEnumerator SwapClass()
	{
		bool isDefaultDeckName = DeckUtil.IsDefaultDeckName(ClassSet, base.FormatBehavior.UseSubClass, _deckCardBundle.DeckName, EditDeckFormat, _myRotationInfo);
		ClassSet.Swap();
		_needsClearSkin = !_needsClearSkin;
		InitUseSubClassDisplay();
		OnValidateSelectionAreaFilter();
		OnValidatePagingFilter();
		while (base.IsLoading)
		{
			yield return null;
		}
		_deckCardBundle.UpdateCardDisplay();
		if (isDefaultDeckName)
		{
			string text = DeckUtil.CreateDefaultDeckName(ClassSet, base.FormatBehavior.UseSubClass, _otherDeckNames, EditDeckFormat, _myRotationInfo);
			if (CurrentDeckName != null)
			{
				CurrentDeckName = text;
			}
			_deckCardBundle.DeckName = text;
			SetupDeckViewerAndText();
		}
	}

	private void InitUseSubClassDisplay()
	{
		if (!_formatBehavior.UseSubClass)
		{
			_useSubClassRoot.SetActive(value: false);
			return;
		}
		m_labelClassName.transform.gameObject.SetActive(value: false);
		_formatIcon.transform.gameObject.SetActive(value: false);
		m_labelDeckCardNum.transform.parent.gameObject.SetActive(value: false);
		_useSubClassRoot.SetActive(value: true);
		_useSubClassMainClassIconSprite.spriteName = ClassCharaPrm.GetIconSpriteName(ClassSet.MainClass);
		_useSubClassSubClassIconSprite.spriteName = ClassCharaPrm.GetIconSpriteName(ClassSet.SubClass);
		_useSubClassDeckCardNumMaxLabel.text = _labelDeckCardMaxNum.text;
		_useSubClassMainClassCardNumMinLabel.text = "/" + 24;
		_useSubClassSubClassCardNumMinLabel.text = "/" + 9;
		_useSubClassFormatIconSprite.spriteName = _formatBehavior.SmallIconSpriteName;
	}

	private bool NeedMyRotationAutoCreateDeckClear()
	{
		CardMaster instance = CardMaster.GetInstance(_formatBehavior.CardMasterId);
		foreach (int id in base.SelectionAreaList.IdList)
		{
			if (!instance.GetCardParameterFromId(id).IsAvailableFormat(Format.MyRotation, ClassType.None, _myRotationInfo))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanAutoCreateCrossoverDeck()
	{
		List<int> source = _deckCardBundle.FindAllAvailableCardInFormat(base.SelectionAreaList.IdList, base.Format, null);
		CardMaster cardMaster = CardMaster.GetInstance(_formatBehavior.CardMasterId);
		int num = source.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == ClassSet.MainClass);
		int num2 = source.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == ClassSet.SubClass);
		int num3 = source.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == CardBasePrm.ClanType.ALL);
		if (num + num3 <= Crossover.AUTO_CREATE_MAIN_AND_NEUTRAL_MAX && num2 + num3 <= Crossover.AUTO_CREATE_SUB_AND_NEUTRAL_MAX && num3 <= 7)
		{
			return true;
		}
		return false;
	}

	private bool ChangeMyRotationAllPackVisibleValidator(bool newCheck)
	{
		if (!base.IsLoading && base.CurrentState == _stateEdit)
		{
			return true;
		}
		return false;
	}

	private void OnChangeMyRotationAllPackVisible()
	{
		_myRotationAllPackVisible = !_myRotationAllPackVisible;
		base.MyRotationFilterTypeCardPool = (_myRotationAllPackVisible ? FilterController.MyRotationFilterType.CARD_POOL_ALL_PACK : FilterController.MyRotationFilterType.CARD_POOL_SELECT_ONLY);
		UpdateMyRotationInfo(_myRotationInfo, isChangeAllCardView: true);
	}
}
