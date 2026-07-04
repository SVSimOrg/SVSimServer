using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.DeckCardEdit;

public class UICardList : MonoBehaviour
{

	private readonly Vector3 MY_ROTATION_FORMAT_ICON_POSITION = new Vector3(167f, 0f, 0f);

	private readonly Vector3 MY_ROTATION_CARD_NUMBER_ICON_POSITION = new Vector3(60f, -74.3f, -1.1f);

	private readonly Vector3 MY_ROTATION_CARD_NUMBER_POSITION = new Vector3(28f, 0f, 0f);

	private readonly Vector3 MY_ROTATION_CARD_NUMBER_MAX_POSITION = new Vector3(59f, 0f, 0f);

	[SerializeField]
	private GameObject _cardListTemplatePrefab;

	private List<CardListTemplate> scrollItems;

	[SerializeField]
	private UIGrid GridBase;

	[SerializeField]
	private UIScrollView scrollView;

	[SerializeField]
	private CostCurveUI CostCurve;

	[SerializeField]
	private NguiObjs CloseBtnObj;

	[SerializeField]
	private UILabel DeckNameLabel;

	[SerializeField]
	private UICamera _detailCamera;

	[SerializeField]
	private UIPanel _rootPanel;

	[SerializeField]
	private UILabel _deckClassLabel;

	[SerializeField]
	private UILabel _deckNumLabel;

	[SerializeField]
	private UILabel _deckDenoNumLabel;

	[SerializeField]
	private UILabel _deckFollowerNumLabel;

	[SerializeField]
	private UILabel _deckSpellNumLabel;

	[SerializeField]
	private UILabel _deckAmuletNumLabel;

	[SerializeField]
	private GameObject _deckNameLine;

	[SerializeField]
	private UIAnchor _anchorTop;

	[SerializeField]
	private UIAnchor _anchorTopRight;

	[SerializeField]
	private UISprite _formatIcon;

	[SerializeField]
	private UIButton _shareButton;

	[SerializeField]
	private UITexture _qrCodeSmallTexture;

	[SerializeField]
	private GameObject _deckIntroductionRoot;

	[SerializeField]
	private GameObject _qrCodeRoot;

	[SerializeField]
	private UIButton _redUtilButton;

	[SerializeField]
	private GameObject _blueUtilButtonRoot;

	[SerializeField]
	private UILabel _blueUtilButtonLabel;

	[SerializeField]
	private UIButton _blueUtilButton;

	[SerializeField]
	private UIButton _showQRCodeButton;

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
	private GameObject _myRotationIconOriginal;

	[SerializeField]
	private UIGrid _myRotationGrid;

	[SerializeField]
	private GameObject _myRotationRoot;

	[SerializeField]
	private GameObject _cardNumberIcon;

	[SerializeField]
	private UISprite _myRotationClassIcon;

	[SerializeField]
	private UILabel _myRotationPackName;

	[SerializeField]
	private UISprite _classNameUnderLine;

	[SerializeField]
	private GameObject _avatarBattleRoot;

	[NonSerialized]
	public GenerateDeckCodeTask.SubmitDeckType SubmitDeckType = GenerateDeckCodeTask.SubmitDeckType.NORMAL;

	private List<int> PlayerDataIds;

	private Dictionary<int, CardListTemplate> PlayerDataIdDict;

	private int CardCount;

	private bool m_ResetFlg;

	private bool _isActive;

	private int _followerNum;

	private int _spellNum;

	private int _amuletNum;

	private int _maxCardNum;

	private DeckData _deck;

	private Format _format = Format.Max;

	private IFormatBehavior _formatBehavior;

	private string _deckName;

	private MyRotationInfo _myRotationInfo;

	private CardBasePrm.ClanType? _clanType;

	private ClassSet _classSet;

	private bool _isLabelMoved;

	public Action OnCardDetailOpen { get; set; }

	public Action OnCardDetailClose { get; set; }

	public CardDetailUI CardDetail { get; private set; }

	public bool IsEnableMyRotationDisplay { get; set; } = true;

	private bool _isShareButtonUse { get; set; }

	private void Awake()
	{
		scrollItems = new List<CardListTemplate>();
		PlayerDataIds = new List<int>();
		PlayerDataIdDict = new Dictionary<int, CardListTemplate>();
		CloseBtnObj.buttons[0].onClick.Clear();
		UIManager.GetInstance().AttachAtlas(base.gameObject);
		_deckIntroductionRoot.gameObject.SetActive(value: false);
		_redUtilButton.gameObject.SetActive(value: false);
		_qrCodeRoot.gameObject.SetActive(value: false);
		_showQRCodeButton.onClick.Clear();
		_showQRCodeButton.gameObject.SetActive(value: false);
		_formatBehavior = FormatBehaviorManager.GetDefaultBehaviour(Format.Max);
		_format = Format.Max;
		CostCurve.Initialize(_formatBehavior.CardMasterId);
	}

	private void OnEnable()
	{
		if (!_isActive)
		{
			_isActive = true;
		}
	}

	private void OnDisable()
	{
		if (_isActive)
		{
			_isActive = false;
		}
	}

	public void Init(GameObject in_ParentObject, CardDetailUI in_CardDetail, string in_DeckName, Action in_CloseButtonEvent, string in_LayerName = "Detail", bool in_DetailCameraUse = false, CardBasePrm.ClanType? clan = null, int in_MaxCardNum = 0)
	{
		RemoveData();
		UpdateFormatIconPosition(isMyRotation: false);
		UpdataAvatarFormatDisplay(isAvatar: false);
		SetMaxCardNum(in_MaxCardNum);
		if (in_ParentObject != null)
		{
			base.gameObject.transform.parent = in_ParentObject.transform;
			base.gameObject.transform.localScale = Vector3.one;
			base.gameObject.transform.localPosition = Vector3.zero;
		}
		int num = LayerMask.NameToLayer(in_LayerName);
		UIManager.GetInstance().SetLayerRecursive(base.gameObject.transform, num);
		CardDetailUI cardDetailUI = (CardDetail = in_CardDetail);
		if (cardDetailUI != null)
		{
			CardDetail.IsShowCraftButtons = false;
		}
		SetDeckName(in_DeckName);
		SetClan(clan);
		if (_detailCamera != null)
		{
			if (in_DetailCameraUse)
			{
				_detailCamera.GetComponent<Camera>().cullingMask = 1 << num;
			}
			_detailCamera.gameObject.SetActive(in_DetailCameraUse);
		}
		CloseBtnObj.buttons[0].onClick.Clear();
		if (in_CloseButtonEvent != null)
		{
			CloseBtnObj.buttons[0].onClick.Add(new EventDelegate(delegate
			{
				in_CloseButtonEvent();

			}));
		}
		else
		{
			CloseBtnObj.buttons[0].onClick.Add(new EventDelegate(delegate
			{
				SetActive(in_Active: false);

			}));
		}
		SetShareButton(clan);
		UpdateDeckInfo();
		SetEnableBlueButton(isEnable: false);
	}

	public void SetFormat(Format inFormat, ConventionDeckList conventionDeckList)
	{
		_formatBehavior = FormatBehaviorManager.Create(inFormat, conventionDeckList);
		_format = inFormat;
		CostCurve.Initialize(_formatBehavior.CardMasterId);
		_formatIcon.spriteName = _formatBehavior.SmallIconSpriteName;
		_formatIcon.gameObject.SetActive(value: true);
		_deckDenoNumLabel.gameObject.SetActive(_formatBehavior.DeckCardNumMin == _formatBehavior.DeckCardNumMax);
		if (!_formatBehavior.IsShowDeckName)
		{
			SetLabelPositionToNoDeckName();
		}
		UpdateDeckInfo();
	}

	public void SetLabelPositionToNoDeckName()
	{
		if (!_isLabelMoved)
		{
			UIUtil.AddPositionY(_deckClassLabel.transform, 24f);
			UIUtil.AddPositionY(_deckNumLabel.transform.parent, 24f);
			UIUtil.AddPositionY(_deckFollowerNumLabel.transform.parent.parent, 21f);
			_isLabelMoved = true;
		}
	}

	public void SetClan(CardBasePrm.ClanType? clan)
	{
		if (!clan.HasValue)
		{
			if (_deckClassLabel != null)
			{
				_deckClassLabel.gameObject.SetActive(value: false);
			}
		}
		else
		{
			_deckClassLabel.gameObject.SetActive(value: true);
			_deckClassLabel.text = ClassCharaPrm.GetNameText(clan.Value);
			_myRotationClassIcon.spriteName = ClassCharaPrm.GetIconSpriteName(clan.Value);
		}
		_clanType = clan;
		SetShareButton(clan);
		InitUseSubClassDisplay();
	}

	public void SetClassSet(ClassSet classSet)
	{
		_classSet = classSet;
		_clanType = classSet.MainClass;
		SetShareButton(classSet.MainClass, classSet.SubClass);
		InitUseSubClassDisplay();
	}

	public void SetDeckName(string in_DeckName)
	{
		_deckName = in_DeckName;
		DeckNameLabel.text = _deckName;
		bool active = false;
		if (!string.IsNullOrEmpty(_deckName))
		{
			active = true;
		}
		DeckNameLabel.gameObject.SetActive(active);
		if (_deckNameLine != null)
		{
			_deckNameLine.SetActive(active);
		}
	}

	public void SetCamera(Camera inCamera)
	{
		_anchorTop.uiCamera = inCamera;
		_anchorTopRight.uiCamera = inCamera;
	}

	public void SetMaxCardNum(int inMaxCardNum = 0)
	{
		_maxCardNum = inMaxCardNum;
		if (_deckDenoNumLabel != null)
		{
			_deckDenoNumLabel.text = "/" + inMaxCardNum;
		}
	}

	public List<string> GetLoadFileList(List<int> inCardIdList)
	{
		List<string> list = new List<string>();
		CardParameter cardParameter = null;
		if (inCardIdList != null)
		{
			for (int i = 0; i < inCardIdList.Count; i++)
			{
				cardParameter = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(inCardIdList[i]);
				CardParameter cardParameterFromId = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(cardParameter.BaseCardId);
				list.Add(Toolbox.ResourcesManager.GetAssetTypePath(cardParameterFromId.ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial));
			}
			UIBase_CardManager uIBase_CardManager = UIManager.GetInstance().getUIBase_CardManager();
			list.AddRange(uIBase_CardManager.AddAssetPath(inCardIdList, is2D: false, _formatBehavior.CardMasterId, isAddSleeve: true, 3000011L));
			list = list.Distinct().ToList();
		}
		list.Add(Toolbox.ResourcesManager.GetAssetTypePath("foiltextures", ResourcesManager.AssetLoadPathType.FoilTextures));
		return list;
	}

	public void SetActive(bool in_Active)
	{
		_rootPanel.alpha = (in_Active ? 1f : 0f);
		CostCurve.gameObject.GetComponent<UIPanel>().alpha = (in_Active ? 1f : 0f);
		if (in_Active)
		{
			OnEnable();
		}
		else
		{
			OnDisable();
		}
	}

	public void RemoveData()
	{
		for (int i = 0; i < scrollItems.Count; i++)
		{
			GridBase.RemoveChild(scrollItems[i].transform);
			UnityEngine.Object.Destroy(scrollItems[i].gameObject);
		}
		GridBase.enabled = true;
		CostCurve.Refresh();
		scrollItems.Clear();
		PlayerDataIds.Clear();
		PlayerDataIdDict.Clear();
		CardCount = 0;
		_followerNum = 0;
		_spellNum = 0;
		_amuletNum = 0;
		_qrCodeSmallTexture.mainTexture = null;
		_qrCodeRoot.gameObject.SetActive(value: false);
		_showQRCodeButton.onClick.Clear();
		_showQRCodeButton.gameObject.SetActive(value: false);
		UpdateDeckInfo();
	}

	public void SetDeck(DeckData deck, ConventionDeckList conventionDeckList, bool isSortIdList = false)
	{
		_deck = deck;
		if (isSortIdList)
		{
			_deck.SetCardIdList(UIManager.GetInstance().getUIBase_CardManager().SortIDList(_deck.GetCardIdList(), CardMaster.CardMasterId.Default));
		}
		List<int> cardIdList = deck.GetCardIdList();
		SetFormat(deck.Format, conventionDeckList);
		SetDeckName(deck.GetDeckName());
		CardBasePrm.ClanType deckSubClassID = (CardBasePrm.ClanType)deck.GetDeckSubClassID();
		if (deckSubClassID == CardBasePrm.ClanType.ALL || deckSubClassID == CardBasePrm.ClanType.NONE)
		{
			SetClan((CardBasePrm.ClanType)deck.GetDeckClassID());
		}
		else
		{
			SetClassSet(new ClassSet(deck.GetDeckClassID(), deck.GetDeckSubClassID()));
		}
		foreach (int item in (IEnumerable<int>)cardIdList)
		{
			addScrollItem(item);
		}
		UIDestroyUtility.RemoveChildren(_myRotationGrid.transform);
		if (deck.MyRotationId != null && IsEnableMyRotationDisplay)
		{
			_myRotationInfo = Data.MyRotationAllInfo.Get(deck.MyRotationId);
			SetMyRotationInfo(deck.GetDeckClassID(), _myRotationInfo);
		}
		UpdateFormatIconPosition(deck.MyRotationId != null && IsEnableMyRotationDisplay);
		UpdataAvatarFormatDisplay(deck.Format == Format.Avatar, deck);
	}

	private void UpdateFormatIconPosition(bool isMyRotation)
	{
		_myRotationRoot.SetActive(isMyRotation);
		if (isMyRotation)
		{
			_formatIcon.transform.localPosition = MY_ROTATION_FORMAT_ICON_POSITION;
			_cardNumberIcon.transform.localPosition = MY_ROTATION_CARD_NUMBER_ICON_POSITION;
			_deckNumLabel.transform.localPosition = MY_ROTATION_CARD_NUMBER_POSITION;
			_deckDenoNumLabel.transform.localPosition = MY_ROTATION_CARD_NUMBER_MAX_POSITION;
			_deckClassLabel.width = 110;
			_deckClassLabel.text = string.Empty;
			_classNameUnderLine.width = 182;
		}
	}

	public void UpdataAvatarFormatDisplay(bool isAvatar, DeckData deck = null)
	{
		_avatarBattleRoot.SetActive(isAvatar);
		if (!isAvatar)
		{
			return;
		}
		// Pre-Phase-5b: SetPlayerAvatarBattleInfo + TryGetPlayerAvatarBattleInfo dropped;
		// headless has no avatar battle info to display.
	}

	public void SetMyRotationInfo(int classId, MyRotationInfo myRotationInfo)
	{
		if (_deckClassLabel != null)
		{
			_deckClassLabel.text = string.Empty;
		}
		_myRotationPackName.text = myRotationInfo.LastPackText;
		foreach (MyRotationInfo.MyRotationBonus ability in myRotationInfo.Abilities)
		{
			GameObject obj = NGUITools.AddChild(_myRotationGrid.gameObject, _myRotationIconOriginal);
			obj.SetActive(value: true);
			obj.GetComponent<UISprite>().spriteName = ability.IconName;
		}
		_myRotationGrid.Reposition();
	}

	public void addScrollItem(int inCardId)
	{
		CardBasePrm.CharaType charType = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(inCardId).CharType;
		switch (charType)
		{
		case CardBasePrm.CharaType.NORMAL:
			_followerNum++;
			break;
		case CardBasePrm.CharaType.FIELD:
		case CardBasePrm.CharaType.CHANT_FIELD:
			_amuletNum++;
			break;
		default:
			_spellNum++;
			break;
		}
		if (PlayerDataIdDict.ContainsKey(inCardId))
		{
			int num = PlayerDataIdDict[inCardId].GetNum();
			PlayerDataIdDict[inCardId].SetNum(++num);
			PlayerDataIds.Add(inCardId);
			CostCurve.Add(inCardId, withAnim: false);
			CardCount = PlayerDataIds.Count;
			UpdateDeckInfo();
			return;
		}
		CardListTemplate obj = UnityEngine.Object.Instantiate(_cardListTemplatePrefab).GetComponent<CardListTemplate>();
		obj.gameObject.transform.parent = GridBase.transform;
		obj.name = inCardId.ToString();
		obj.gameObject.SetActive(value: true);
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
		obj.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
		obj.gameObject.AddComponent<CharIdx>().SetCardId(inCardId);
		CardParameter cardParameterFromId = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(inCardId);
		Material material = UIBase_CardManager.Get2dCardMaterial(cardParameterFromId);
		CardShaderDefine.ReplaceBaseShader(material, cardParameterFromId.IsFoil);
		obj._cardTexture.material = material;
		obj.SetFrame(cardParameterFromId);
		UILabel nameLabel = obj._nameLabel;
		nameLabel.text = cardParameterFromId.CardName;
		UIManager.GetInstance().getUIBase_CardManager().SetNameLabelStyle(nameLabel, cardParameterFromId.IsFoil);
		Global.SetRepositionNameLabel(nameLabel, cardParameterFromId.CardName, is2D: true);
		nameLabel.gameObject.SetActive(value: true);
		if (charType == CardBasePrm.CharaType.NORMAL)
		{
			obj._atkLabel.text = cardParameterFromId.Atk.ToString();
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(obj._atkLabel, cardParameterFromId.IsFoil);
			obj._lifeLabel.text = cardParameterFromId.Life.ToString();
			UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(obj._lifeLabel, cardParameterFromId.IsFoil);
		}
		else
		{
			obj._atkLabel.text = "";
			obj._lifeLabel.text = "";
		}
		obj._costLabel.text = cardParameterFromId.Cost.ToString();
		UIManager.GetInstance().getUIBase_CardManager().SetNumberLabelStyle(obj._costLabel, cardParameterFromId.IsFoil);
		obj.SetNum(1);
		obj._newLabel.gameObject.SetActive(value: false);
		obj.SetId(inCardId);
		obj._classIconTexture.mainTexture = ClassCharaPrm.GetClassIconTexture((int)cardParameterFromId.Clan);
		obj.RotationOnlyIconVisible = cardParameterFromId.IsResurgentCard;
		scrollItems.Add(obj);
		PlayerDataIds.Add(inCardId);
		PlayerDataIdDict.Add(inCardId, obj);
		CostCurve.Add(inCardId, withAnim: false);
		GridBase.Reposition();
		CardCount = PlayerDataIds.Count;
		obj.gameObject.AddComponent<UIEventListener>().onClick = delegate
		{
			CardDetail.OnPushCardDetailOn(obj.gameObject);
			OnCardDetailOpen.Call();
		};
		obj.gameObject.AddComponent<BoxCollider>().size = new Vector3(obj._cardTexture.localSize.x, obj._cardTexture.localSize.y, 1f);
		CardDetail.OnClose = delegate
		{
			OnCardDetailClose.Call();
		};
		obj.gameObject.AddComponent<UIDragScrollView>().scrollView = scrollView;
		ResetScroll();
		UpdateDeckInfo();
	}

	protected void UpdateDeckInfo()
	{
		if (_deckNumLabel != null)
		{
			_deckNumLabel.text = CardCount.ToString();
			_deckFollowerNumLabel.text = _followerNum.ToString();
			_deckSpellNumLabel.text = _spellNum.ToString();
			_deckAmuletNumLabel.text = _amuletNum.ToString();
			bool flag = (_formatBehavior.IsEmphasizeDeckCardShortage && CardCount < _formatBehavior.DeckCardNumMin) || (_formatBehavior.IsEmphasizeDeckCardOverage && _formatBehavior.DeckCardNumMax < CardCount);
			_deckNumLabel.color = (flag ? LabelDefine.TEXT_COLOR_RED : LabelDefine.TEXT_COLOR_NORMAL);
		}
		UpdateUseSubClassDeckInfo();
		CheckEnableShareButton();
	}

	private void UpdateUseSubClassDeckInfo()
	{
		if (_formatBehavior.UseSubClass)
		{
			CardMaster cardMaster = CardMaster.GetInstance(_formatBehavior.CardMasterId);
			int num = PlayerDataIds.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == _classSet.MainClass);
			int num2 = PlayerDataIds.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == _classSet.SubClass);
			bool isRed = (_formatBehavior.IsEmphasizeDeckCardShortage && CardCount < _formatBehavior.DeckCardNumMin) || (_formatBehavior.IsEmphasizeDeckCardOverage && CardCount > _formatBehavior.DeckCardNumMax);
			SetCardNumLabel(_useSubClassDeckCardNumLabel, CardCount, isRed);
			SetCardNumLabel(_useSubClassMainClassCardNumLabel, num, isRed: false);
			SetCardNumLabel(_useSubClassSubClassCardNumLabel, num2, isRed: false);
		}
		static void SetCardNumLabel(UILabel label, int num3, bool isRed)
		{
			label.text = num3.ToString();
			label.color = (isRed ? LabelDefine.TEXT_COLOR_RED : LabelDefine.TEXT_COLOR_NORMAL);
		}
	}

	public void ResetScroll()
	{
		if (!m_ResetFlg)
		{
			m_ResetFlg = true;
			scrollView.GetComponent<UIPanel>().alpha = 0f;
		}
	}

	public void SetShareButtonUse(bool isUse)
	{
		_isShareButtonUse = isUse;
		if (_classSet != null)
		{
			SetShareButton(_classSet.MainClass, _classSet.SubClass);
		}
		else
		{
			SetShareButton(_clanType);
		}
	}

	private void SetShareButton(CardBasePrm.ClanType? clan, CardBasePrm.ClanType subClass = CardBasePrm.ClanType.NONE)
	{
		_shareButton.gameObject.SetActive(clan.HasValue);
		_shareButton.gameObject.SetActive(value: false);
		if (!clan.HasValue)
		{
			return;
		}
		CheckEnableShareButton();
		_shareButton.onClick.Clear();
		_shareButton.onClick.Add(new EventDelegate(delegate
		{
			List<int> excludedPhantomCardList = PlayerDataIds;
			List<int> phantomCardList = null;
			ClassSet classSet = new ClassSet(clan.Value, CardBasePrm.ClanType.NONE);
			string rotationId = string.Empty;
			if (CardBasePrm.ClanTypeIsUseable(subClass))
			{
				classSet.SubClass = subClass;
				SubmitDeckType = _formatBehavior.DeckCodeType;
			}
			if (_format == Format.MyRotation && _deck != null)
			{
				SubmitDeckType = _formatBehavior.DeckCodeType;
				rotationId = _deck.MyRotationId;
			}
			if (SubmitDeckType == GenerateDeckCodeTask.SubmitDeckType.SEALED)
			{
				SealedData.GroupByPhantomCard(PlayerDataIds, out excludedPhantomCardList, out phantomCardList, isConvertToOriginalCardId: true);
			}
			_shareButton.gameObject.GetComponent<Twitter>().TweetDataFromPortal(excludedPhantomCardList.ToArray(), classSet, SubmitDeckType, phantomCardList?.ToArray(), rotationId);
		}));
	}

	public bool CanShowQRCodeUseSubclass()
	{
		CardMaster cardMaster = CardMaster.GetInstance(_formatBehavior.CardMasterId);
		if (PlayerDataIds.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == _classSet.MainClass) < 24)
		{
			return false;
		}
		if (PlayerDataIds.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == _classSet.SubClass) < 9)
		{
			return false;
		}
		return true;
	}

	public void SetQRCodeButtonToGray()
	{
		_showQRCodeButton.gameObject.SetActive(value: true);
		UIManager.SetObjectToGrey(_showQRCodeButton.gameObject, b: true);
	}

	private void CheckEnableShareButton()
	{
		bool flag = _formatBehavior.IsEnableDeckShareButton(CardCount, _maxCardNum);
		if (_format == Format.MyRotation && _deck != null)
		{
			flag &= IsAllCardCorrectMyRotation();
		}
		if (_formatBehavior.UseSubClass)
		{
			flag &= CanShowQRCodeUseSubclass();
		}
		UIManager.SetObjectToGrey(_shareButton.gameObject, !flag);
	}

	public bool IsAllCardCorrectMyRotation()
	{
		MyRotationInfo myRotationInfo = Data.MyRotationAllInfo.Get(_deck.MyRotationId);
		foreach (int item in PlayerDataIds.Distinct())
		{
			CardMaster instance = CardMaster.GetInstance(_formatBehavior.CardMasterId);
			CardParameter cardParameterFromId = instance.GetCardParameterFromId(item);
			if (cardParameterFromId.IsPrizeCard)
			{
				cardParameterFromId = instance.GetCardParameterFromId(cardParameterFromId.BaseCardId);
			}
			if (!myRotationInfo.CardPadkIdList.Contains(cardParameterFromId.CardSetId) && !IsRePrintCardAvailable(myRotationInfo, cardParameterFromId))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsRePrintCardAvailable(MyRotationInfo myRotationInfo, CardParameter cardParameter)
	{
		if (myRotationInfo.IsRePrintCard(cardParameter.BaseCardId))
		{
			foreach (string cardPadkId in myRotationInfo.CardPadkIdList)
			{
				if (myRotationInfo.IsRePrintCardAvailablePack(cardParameter.BaseCardId, cardPadkId))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetEnableBlueButton(bool isEnable, string label = null, Action onClickCallback = null)
	{
		_blueUtilButtonRoot.SetActive(isEnable);
		if (!isEnable)
		{
			return;
		}
		_blueUtilButtonLabel.text = label;
		_blueUtilButton.onClick.Clear();
		_blueUtilButton.onClick.Add(new EventDelegate(delegate
		{
			if (onClickCallback != null)
			{
				onClickCallback();
			}
		}));
	}

	private void InitUseSubClassDisplay()
	{
		if (!_formatBehavior.UseSubClass)
		{
			if (_clanType.HasValue)
			{
				_deckClassLabel.gameObject.SetActive(value: true);
				_deckClassLabel.text = ClassCharaPrm.GetNameText(_clanType.Value);
				_formatIcon.transform.gameObject.SetActive(value: true);
				_deckNumLabel.transform.parent.gameObject.SetActive(value: true);
			}
			_useSubClassRoot.SetActive(value: false);
		}
		else
		{
			_deckClassLabel.transform.gameObject.SetActive(value: false);
			_formatIcon.transform.gameObject.SetActive(value: false);
			_deckNumLabel.transform.parent.gameObject.SetActive(value: false);
			_useSubClassRoot.SetActive(value: true);
			_useSubClassMainClassIconSprite.spriteName = ClassCharaPrm.GetIconSpriteName(_classSet.MainClass);
			_useSubClassSubClassIconSprite.spriteName = ClassCharaPrm.GetIconSpriteName(_classSet.SubClass);
			_useSubClassDeckCardNumMaxLabel.text = _deckDenoNumLabel.text;
			_useSubClassMainClassCardNumMinLabel.text = "/" + 24;
			_useSubClassSubClassCardNumMinLabel.text = "/" + 9;
			_useSubClassFormatIconSprite.spriteName = _formatBehavior.SmallIconSpriteName;
		}
	}
}
