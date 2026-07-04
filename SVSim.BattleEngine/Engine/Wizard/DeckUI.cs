using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class DeckUI : MonoBehaviour
{
	public enum eViewType
	{
		Normal,
		Empty,
		CreateNew
	}

	public class DeckViewData
	{
		public eViewType ViewType { get; private set; }

		public DeckData Deck { get; private set; }

		public DeckViewData(eViewType type, DeckData deck)
		{
			ViewType = type;
			Deck = deck;
		}

		public static List<DeckViewData> CreateDeckViewList(List<DeckData> deckList, bool isCreateNewButton)
		{
			List<DeckViewData> list = new List<DeckViewData>();
			if (deckList.Count > 0)
			{
				foreach (DeckData deck in deckList)
				{
					eViewType type = eViewType.Normal;
					if (deck.IsNoCard())
					{
						if (isCreateNewButton && IsVisibleCreateNewButtonAttribute(deck.DeckAttributeType))
						{
							type = eViewType.CreateNew;
							isCreateNewButton = false;
						}
						else
						{
							type = eViewType.Empty;
						}
					}
					list.Add(new DeckViewData(type, deck));
				}
			}
			return list;
		}

		private static bool IsVisibleCreateNewButtonAttribute(DeckAttributeType attributeType)
		{
			return attributeType == DeckAttributeType.CustomDeck;
		}
	}

	public class ClearOptionUISetting
	{
		public bool SkipAppealLabelLeft { get; set; }
	}

	private static readonly Color32 DECK_COLOR_GREY = new Color32(102, 102, 102, byte.MaxValue);

	private static readonly Color32 DECK_COLOR_DEFAULT = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[SerializeField]
	private UIWidget _rootDeckUIWidget;

	[SerializeField]
	private GameObject _rootNormalDeckView;

	[SerializeField]
	private GameObject _rootEmptyDeckView;

	[SerializeField]
	private GameObject _rootCreateNewDeckView;

	[SerializeField]
	private UITexture _classTexture;

	[SerializeField]
	private UITexture _sleeve;

	[SerializeField]
	private UILabel _deckName;

	[SerializeField]
	private UISprite _warningSprite;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private UILabel _centerLabel;

	[SerializeField]
	private UISprite _checkMark;

	[SerializeField]
	private UILabel _appealLabelLeft;

	[SerializeField]
	private UILabel _appealLabelRight;

	[SerializeField]
	private UISprite _skinRandomIcon;

	[SerializeField]
	private ClassInfoParts _classInfoParts;

	[SerializeField]
	private TweenScale _clickAnimationTween;

	[SerializeField]
	private GameObject _myRotationRoot;

	[SerializeField]
	private FlexibleGrid _myRotationGrid;

	[SerializeField]
	private UILabel _myRotationPackName;

	[SerializeField]
	private UISprite _myRotationIconBG;

	[SerializeField]
	private GameObject _myRotationPackNamePadding;

	[SerializeField]
	private GameObject _myRotationIconOriginal;

	private List<UISprite> _myRotationIconList = new List<UISprite>();

	private bool _isSelectable = true;

	private bool _enablePremiumSleeveAnimation = true;

	private Action<DeckUI> OnLongPress;

	private bool _isLongPress;

	private float _pressStartTime;

	private static readonly ClearOptionUISetting ClearOptionUiSetting = new ClearOptionUISetting
	{
		SkipAppealLabelLeft = true
	};

	public DeckData Deck { get; private set; }

	public bool IsCheckeMark { get; private set; }

	public eViewType ViewType { get; private set; }

	public void Initialize(Action<DeckUI> onClick, Action<DeckUI> onLongPress = null)
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject g, bool b)
		{
			if (_isSelectable && ViewType != eViewType.Empty)
			{
				PushedAnimation(b);
			}
		});
		if (onLongPress != null)
		{
			OnLongPress = onLongPress;
			UIEventListener uIEventListener2 = UIEventListener.Get(base.gameObject);
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, (UIEventListener.BoolDelegate)delegate(GameObject g, bool isPress)
			{
				_isLongPress = isPress;
				if (isPress)
				{
					_pressStartTime = Time.realtimeSinceStartup;
				}
			});
		}
		UIEventListener uIEventListener3 = UIEventListener.Get(base.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (_isSelectable && ViewType != eViewType.Empty)
			{
				onClick.Call(this);
			}
		});
	}

	public void UpdateView(DeckViewData deckViewData, bool canUseNonPossessionCard = false, bool enablePremiumSleeveAnimation = true)
	{
		UpdateView(deckViewData.Deck, deckViewData.ViewType, canUseNonPossessionCard, enablePremiumSleeveAnimation);
	}

	public void UpdateView(DeckData deckData, eViewType viewType, bool canUseNonPossessionCard = false, bool enablePremiumSleeveAnimation = true)
	{
		Deck = deckData;
		ViewType = viewType;
		_enablePremiumSleeveAnimation = enablePremiumSleeveAnimation;
		_rootNormalDeckView.gameObject.SetActive(value: false);
		_rootEmptyDeckView.gameObject.SetActive(value: false);
		_rootCreateNewDeckView.gameObject.SetActive(value: false);
		PushedAnimation(isStart: false);
		switch (ViewType)
		{
		case eViewType.Normal:
			_rootNormalDeckView.gameObject.SetActive(value: true);
			ClearAllOptionUI(ClearOptionUiSetting);
			SetDisplayByDeckData(Deck, canUseNonPossessionCard);
			break;
		case eViewType.Empty:
			_rootEmptyDeckView.gameObject.SetActive(value: true);
			_isSelectable = false;
			break;
		case eViewType.CreateNew:
			_rootCreateNewDeckView.gameObject.SetActive(value: true);
			_isSelectable = true;
			if (Data.MaintenanceCodeList.Contains(NetworkDefine.MAINTENANCE_TYPE.DECK_MAINTENANCE))
			{
				UIManager.SetObjectToGrey(_rootCreateNewDeckView, b: true);
			}
			break;
		}
	}

	public void ClearAllOptionUI(ClearOptionUISetting setting = null)
	{
		if (setting == null)
		{
			setting = new ClearOptionUISetting();
		}
		SetVisibleCheckMark(isVisible: false);
		SetTextCenterLabe(string.Empty);
		if (!setting.SkipAppealLabelLeft)
		{
			SetTextAppealLabelLeft(string.Empty);
		}
		SetTextAppealLabelRight(string.Empty);
		SetVisibleSkinRandomIcon(isVisible: false);
		_warningSprite.gameObject.SetActive(value: false);
	}

	public void SetColorToGrey(bool isGrey)
	{
		Color color = (isGrey ? LabelDefine.TEXT_COLOR_BUTTON_DISABLE : LabelDefine.TEXT_COLOR_BUTTON_ENABLE);
		_deckName.color = color;
		_warningLabel.color = color;
		_myRotationPackName.color = color;
		Color color2 = (isGrey ? DECK_COLOR_GREY : DECK_COLOR_DEFAULT);
		_classTexture.color = color2;
		_sleeve.color = color2;
		_warningSprite.color = color2;
		_skinRandomIcon.color = color2;
		_classInfoParts.ClassIconSprite.color = color2;
		_classInfoParts.SubClassIconSprite.color = color2;
		foreach (UISprite myRotationIcon in _myRotationIconList)
		{
			myRotationIcon.color = color2;
		}
	}

	public void UpdateUIAlpha(float alpha)
	{
		_rootDeckUIWidget.alpha = alpha;
	}

	public void SetVisibleCheckMark(bool isVisible)
	{
		IsCheckeMark = isVisible;
		_checkMark.gameObject.SetActive(isVisible);
	}

	public void SetDeckNameText(string text)
	{
		_deckName.text = text;
	}

	public void SetTextCenterLabe(string text)
	{
		_centerLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
		_centerLabel.color = LabelDefine.TEXT_COLOR_NORMAL;
		_centerLabel.text = text;
	}

	public void SetTextAppealLabelLeft(string text)
	{
		_appealLabelLeft.gameObject.SetActive(!string.IsNullOrEmpty(text));
		_appealLabelLeft.text = text;
	}

	public void SetTextAppealLabelRight(string text)
	{
		_appealLabelRight.gameObject.SetActive(!string.IsNullOrEmpty(text));
		_appealLabelRight.text = text;
	}

	public void SetSelectable(bool isSelectable)
	{
		_isSelectable = isSelectable;
		SetColorToGrey(!isSelectable);
	}

	public void ClearLongPress()
	{
		_isLongPress = false;
	}

	public void SetVisibleSkinRandomIcon(bool isVisible)
	{
		_skinRandomIcon.gameObject.SetActive(isVisible);
	}

	private void PushedAnimation(bool isStart)
	{
		if (isStart)
		{
			_clickAnimationTween.PlayForward();
		}
		else
		{
			_clickAnimationTween.PlayReverse();
		}
	}

	private void SetDisplayByDeckData(DeckData deck, bool canUseNonPossessionCard)
	{
		SetSelectable(isSelectable: true);
		SetDeckNameText(deck.GetDeckName());
		int skinId = deck.GetSkinId();
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(skinId.ToString(), ResourcesManager.AssetLoadPathType.DeckListTexture, isfetch: true);
		_classTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath);
		Sleeve sleeve = Data.Master.SleeveMgr.Get(deck.GetDeckSleeveID());
		if (_enablePremiumSleeveAnimation)
		{
			UIManager.GetInstance().getUIBase_CardManager().SetSleeveTexture(_sleeve, sleeve.sleeve_id);
		}
		else
		{
			UIManager.GetInstance().getUIBase_CardManager().SetSleeveTextureWithoutPremium(_sleeve, sleeve.sleeve_id);
		}
		SetClassIcons(deck);
		SetVisibleSkinRandomIcon(deck.IsVisibleRandomIcon());
		string textAppealLabelLeft = (deck.IsRecommend ? Data.SystemText.Get("Story_0071") : string.Empty);
		SetTextAppealLabelLeft(textAppealLabelLeft);
		DeckData.UnusableReason reason;
		bool flag = deck.IsUsable(out reason, canUseNonPossessionCard);
		_warningSprite.gameObject.SetActive(!flag);
		if (!flag)
		{
			_warningLabel.text = GetDeckUnusableReasonText(reason);
		}
		_myRotationIconOriginal.SetActive(value: false);
		MyRotationInfo myRotationInfo = Data.MyRotationAllInfo.Get(deck.MyRotationId);
		if (myRotationInfo != null)
		{
			_myRotationPackName.transform.parent = base.transform;
			_myRotationPackNamePadding.transform.parent = base.transform;
			_myRotationRoot.SetActive(value: true);
			_myRotationGrid.transform.DestroyChildren();
			GameObject gameObject = _myRotationPackName.gameObject;
			bool flag2 = true;
			for (int num = myRotationInfo.Abilities.Count - 1; num >= 0; num--)
			{
				MyRotationInfo.MyRotationBonus myRotationBonus = myRotationInfo.Abilities[num];
				GameObject gameObject2 = NGUITools.AddChild(_myRotationGrid.gameObject, _myRotationIconOriginal);
				UISprite component = gameObject2.GetComponent<UISprite>();
				_myRotationIconList.Add(component);
				component.spriteName = myRotationBonus.IconName;
				gameObject2.SetActive(value: true);
				if (flag2)
				{
					gameObject = gameObject2;
					flag2 = false;
				}
			}
			_myRotationPackNamePadding.transform.parent = _myRotationGrid.transform;
			_myRotationPackName.transform.parent = _myRotationGrid.transform;
			_myRotationPackName.text = myRotationInfo.LastPackText;
			_myRotationPackName.ProcessText();
			_myRotationGrid.Reposition();
			Vector3 localPosition = _myRotationPackName.transform.localPosition;
			localPosition.y = -1f;
			_myRotationPackName.transform.localPosition = localPosition;
			if (gameObject != null)
			{
				_myRotationIconBG.rightAnchor.target = gameObject.transform;
				_myRotationIconBG.ResetAndUpdateAnchors();
			}
		}
		else
		{
			_myRotationRoot.SetActive(value: false);
		}
	}

	private void SetClassIcons(DeckData deck)
	{
		bool useSubClass = FormatBehaviorManager.GetDefaultBehaviour(deck.Format).UseSubClass;
		_classInfoParts.gameObject.SetActive(useSubClass);
		if (useSubClass)
		{
			_classInfoParts.InitByCharaId(deck.GetDeckClassID());
			_classInfoParts.SetSubClass((CardBasePrm.ClanType)deck.GetDeckSubClassID());
		}
	}

	private string GetDeckUnusableReasonText(DeckData.UnusableReason deckUnusableReason)
	{
		string result = string.Empty;
		switch (deckUnusableReason)
		{
		case DeckData.UnusableReason.MaintenanceCard:
			result = Data.SystemText.Get("System_0038");
			break;
		case DeckData.UnusableReason.FormatRestrictCard:
			result = Data.SystemText.Get("Card_0193");
			break;
		case DeckData.UnusableReason.TooLittleCards:
			result = Data.SystemText.Get("Card_0070");
			break;
		case DeckData.UnusableReason.TooMuchCards:
			result = Data.SystemText.Get("Card_0139");
			break;
		case DeckData.UnusableReason.NonPossessionCard:
			result = Data.SystemText.Get("Card_0253");
			break;
		case DeckData.UnusableReason.ShortageMainClassCards:
			result = Data.SystemText.Get("Card_0286");
			break;
		case DeckData.UnusableReason.ShortageSubClassCards:
			result = Data.SystemText.Get("Card_0287");
			break;
		case DeckData.UnusableReason.ShortageBothClassCards:
			result = Data.SystemText.Get("Card_0288");
			break;
		case DeckData.UnusableReason.Unknown:
			Debug.LogError("unknown deck unusable reason.");
			break;
		}
		return result;
	}
}
