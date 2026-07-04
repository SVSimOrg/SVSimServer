using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;

public class FilterController : MonoBehaviour
{
	public enum MyRotationFilterType
	{
		None,
		DECK,
		CARD_POOL_SELECT_ONLY,
		CARD_POOL_ALL_PACK
	}

	private enum eFILTER_TYPE
	{
		CLASS,
		ATTACK,
		LIFE}

	[Serializable]
	public class ButtonArray
	{
		public UIButton[] array;

		public UIButton this[int index]
		{
			get
			{
				return array[index];
			}
			set
			{
				array[index] = value;
			}
		}

		public int Length => array.Length;
	}

	private IFormatBehavior _formatBehavior;

	private int[] FlagsArray;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ButtonArray[] BtnArray;

	[SerializeField]
	private UIButton _packBtnOrigin;

	[SerializeField]
	private UIGrid _gridPack;

	private List<UIButton> _packBtnList = new List<UIButton>();

	private List<bool> _packButtonFlagList = new List<bool>();

	[SerializeField]
	private UISprite _spriteRotationBtn;

	[SerializeField]
	private UISprite _spriteUnlimitedBtn;

	[SerializeField]
	private FlexibleGrid _detailGrid;

	[SerializeField]
	private FlexibleGrid _typeListGrid;

	[SerializeField]
	private UIInputWizard _characterVoiceSearchInput;

	private List<string> _keywordFilterList;

	private List<CardBasePrm.TribeType> _filterTypeList = new List<CardBasePrm.TribeType>();

	private List<GameObject> _currentFilterList = new List<GameObject>();

	private List<GameObject> _currentTypeFilterList = new List<GameObject>();

	private List<CardBasePrm.TribeType> _typeFilterList;

	private Dictionary<string, bool> _existKeyWordList;

	private MyRotationInfo _myRotationInfo;

	private MyRotationFilterType _myRotationFilterType;

	private bool _isMyRotationAllPackVisible;

	public ClassSet ClassSet { get; set; }

	public Format FormatState { get; set; } = Format.Max;

	public bool IsAbleFormatFilter { get; set; } = true;

	public bool IsShow { get; set; }

	public event Action OnValidate;

	public void Initialize(IFormatBehavior formatBehavior)
	{
		_formatBehavior = formatBehavior;
	}

	private void Awake()
	{
		FlagsArray = new int[10];
		for (int i = 0; i < FlagsArray.Length; i++)
		{
			FlagsArray[i] = 1;
		}
	}

	public UIBase_CardManager.FilterParameter GetFilterParameter(UIBase_CardManager.FilterParameter param)
	{
		param.Cost = FlagsArray[0] >> 1;
		param.Favorite = FlagsArray[1] >> 1;
		param.Rarity = FlagsArray[2] >> 1;
		param.Type = FlagsArray[3] >> 1;
		param.Foil = FlagsArray[4] >> 1;
		param.Attack = FlagsArray[7] >> 1;
		param.Life = FlagsArray[8] >> 1;
		param.Spot = FlagsArray[9] >> 1;
		param.FormatState = GetFormatFilterParam();
		param.Class = GetClassFilterParam();
		param.KeyWordList = ((_keywordFilterList == null) ? null : new List<string>(_keywordFilterList));
		param.TypeFilter = ((_filterTypeList == null) ? null : new List<CardBasePrm.TribeType>(_filterTypeList));
		param.CharacterVoice = _characterVoiceSearchInput.value;
		if (_myRotationInfo != null && _myRotationFilterType != MyRotationFilterType.DECK)
		{
			param.DisableCardSetidList = Data.MyRotationAllInfo.DisableCardPackIdList;
		}
		CollectPackFilterParam(param);
		return param;
	}

	private Format GetFormatFilterParam()
	{
		Format format = Format.Rotation;
		if (IsAbleFormatFilter)
		{
			if (FormatState != Format.Max)
			{
				return FormatState;
			}
			return GetFormatBtnState();
		}
		return Format.Max;
	}

	private int GetClassFilterParam()
	{
		if (ClassSet.MainClass == CardBasePrm.ClanType.ALL)
		{
			return FlagsArray[5] >> 1;
		}
		int num = 0;
		if (FlagsArray[5] == 1)
		{
			num = (1 << (int)ClassSet.MainClass) + 1;
			if (_formatBehavior.UseSubClass)
			{
				num += GetClassFilterMask(ClassSet.SubClass);
			}
		}
		else
		{
			bool flag = IsEnableBit(ref FlagsArray[5], 1);
			num = (IsEnableBit(ref FlagsArray[5], GetClassBtnIndex(ClassSet.MainClass)) ? GetClassFilterMask(ClassSet.MainClass) : 0);
			num += (flag ? 1 : 0);
			if (_formatBehavior.UseSubClass && IsEnableBit(ref FlagsArray[5], GetClassBtnIndex(ClassSet.SubClass)))
			{
				num += GetClassFilterMask(ClassSet.SubClass);
			}
		}
		return num;
	}

	private void CollectPackFilterParam(UIBase_CardManager.FilterParameter param)
	{
		if (_packButtonFlagList.Count == 0)
		{
			return;
		}
		int num = 1;
		IFormatBehavior formatBehavior = _formatBehavior;
		param.MyRotationInfoForFormatAvailable = null;
		if (_myRotationInfo != null && _myRotationFilterType == MyRotationFilterType.CARD_POOL_SELECT_ONLY)
		{
			param.MyRotationInfoForFormatAvailable = _myRotationInfo;
		}
		if (_myRotationInfo != null)
		{
			SetMyRotationCardPackFilter(param);
			return;
		}
		List<CardSetName> availableCardSetNameList = formatBehavior.AvailableCardSetNameList;
		List<string> list = new List<string>(availableCardSetNameList.Count);
		foreach (CardSetName item in availableCardSetNameList)
		{
			if (_packButtonFlagList[num])
			{
				list.Add(item.ID);
			}
			num++;
		}
		param.CardSetIdList = list;
		if (formatBehavior.IsShowPrizeCardSetFilter)
		{
			param.IsEnabledPrizeCard = _packButtonFlagList[num];
			num++;
		}
		if (formatBehavior.IsShowPhantomCardSetFilter)
		{
			param.IsEnabledPhantomCard = _packButtonFlagList[num];
			num++;
		}
	}

	private void SetMyRotationCardPackFilter(UIBase_CardManager.FilterParameter param)
	{
		int num = 1;
		IFormatBehavior formatBehavior = _formatBehavior;
		SetMyRotationFilterParam(param, _myRotationFilterType, _myRotationInfo);
		if (_myRotationFilterType == MyRotationFilterType.CARD_POOL_ALL_PACK || _isMyRotationAllPackVisible)
		{
			List<CardSetName> availableCardSetNameList = formatBehavior.AvailableCardSetNameList;
			List<string> list = new List<string>(availableCardSetNameList.Count);
			foreach (CardSetName item in availableCardSetNameList)
			{
				if (!Data.MyRotationAllInfo.DisableCardPackIdList.Contains(item.ID))
				{
					if (!_packButtonFlagList[0] && _packButtonFlagList[num])
					{
						list.Add(item.ID);
					}
					num++;
				}
			}
			param.CardSetIdList = list;
			if (formatBehavior.IsShowPrizeCardSetFilter)
			{
				param.IsEnabledPrizeCard = _packButtonFlagList[num];
				num++;
			}
			if (formatBehavior.IsShowPhantomCardSetFilter)
			{
				param.IsEnabledPhantomCard = _packButtonFlagList[num];
				num++;
			}
			return;
		}
		List<string> list2 = new List<string>(_myRotationInfo.CardPadkIdList.Count);
		foreach (string cardPadkId in _myRotationInfo.CardPadkIdList)
		{
			if (_packButtonFlagList[num])
			{
				list2.Add(cardPadkId);
			}
			num++;
		}
		param.CardSetIdList = list2;
		if (formatBehavior.IsShowPrizeCardSetFilter)
		{
			param.IsEnabledPrizeCard = _packButtonFlagList[num];
			num++;
		}
		if (formatBehavior.IsShowPhantomCardSetFilter)
		{
			param.IsEnabledPhantomCard = _packButtonFlagList[num];
			num++;
		}
	}

	public static int GetClassFilterMask(CardBasePrm.ClanType classType)
	{
		if (classType == CardBasePrm.ClanType.NONE)
		{
			return 1;
		}
		return 1 << (int)classType;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		IsShow = false;
	}

	public void Reset(bool isEnableValidateCall = true)
	{
		for (int i = 0; i < 10; i++)
		{
			FlagsArray[i] = 1;
			RenameSpriteArray((eFILTER_TYPE)i, FlagsArray[i]);
		}
		ResetPackButtonFlags();
		UpdatePackButtonSprites();
		if (_keywordFilterList != null)
		{
			_keywordFilterList.Clear();
		}
		if (_filterTypeList != null)
		{
			_filterTypeList.Clear();
		}
		RemoveCurrentKeyWordList();
		RemoveCurrentTypeFilterList();
		_characterVoiceSearchInput.value = "";
		_detailGrid.Reposition();
		OnUpdateTypeOrKeyWordFilter();
		if (isEnableValidateCall)
		{
			this.OnValidate.Call();
		}
		_spriteRotationBtn.spriteName = RenameSpriteString(_spriteRotationBtn.spriteName, isEnable: false);
		_spriteUnlimitedBtn.spriteName = RenameSpriteString(_spriteUnlimitedBtn.spriteName, isEnable: false);
	}

	private int GetClassBtnIndex(CardBasePrm.ClanType ClassType)
	{
		return (int)(ClassType + 1);
	}

	public Format GetFormatBtnState()
	{
		int target = FlagsArray[6];
		if (IsEnableBit(ref target, GetFormatBtnIndex(Format.Rotation)))
		{
			return Format.Rotation;
		}
		if (IsEnableBit(ref target, GetFormatBtnIndex(Format.Unlimited)))
		{
			return Format.Unlimited;
		}
		return Format.Max;
	}

	private int GetFormatBtnIndex(Format inFormat)
	{
		int result = 0;
		if (inFormat == Format.Rotation || inFormat == Format.Unlimited)
		{
			result = (int)(inFormat + 1);
		}
		return result;
	}

	private bool IsEnableBit(ref int target, int index)
	{
		int num = 1 << index;
		return (target & num) != 0;
	}

	private void RenameSpriteArray(eFILTER_TYPE type, int bitStream)
	{
		ButtonArray buttonArray = BtnArray[(int)type];
		for (int i = 0; i < buttonArray.Length; i++)
		{
			RenameBtnSprite(buttonArray[i], (bitStream >> i) % 2 != 0);
		}
	}

	private void RenameBtnSprite(UIButton button, bool isEnable)
	{
		button.normalSprite = RenameSpriteString(button.normalSprite, isEnable);
	}

	private string RenameSpriteString(string str, bool isEnable)
	{
		string oldValue = (isEnable ? "_off" : "_on");
		string newValue = (isEnable ? "_on" : "_off");
		return str.Replace(oldValue, newValue);
	}

	public void SetMyRotationData(MyRotationInfo myRotationInfo, MyRotationFilterType myRotationFilterType, bool isAllBackVisible)
	{
		if (myRotationInfo == _myRotationInfo && myRotationFilterType == _myRotationFilterType && _isMyRotationAllPackVisible == isAllBackVisible)
		{
			return;
		}
		_myRotationFilterType = myRotationFilterType;
		_myRotationInfo = myRotationInfo;
		_isMyRotationAllPackVisible = isAllBackVisible;
		if (_packBtnList.Count > 0)
		{
			foreach (UIButton packBtn in _packBtnList)
			{
				UnityEngine.Object.Destroy(packBtn.gameObject);
			}
			_packBtnList.Clear();
		}
		SetPackButtons();
	}

	private void SetPackButtons()
	{
		if (_packBtnList.Count > 0)
		{
			return;
		}
		_packBtnOrigin.gameObject.SetActive(value: false);
		IFormatBehavior formatBehavior = _formatBehavior;
		CreatePackButton(Data.SystemText.Get("Card_0040")).GetComponentInChildren<UILabel>().height = 26;
		if (_myRotationInfo == null)
		{
			foreach (CardSetName availableCardSetName in formatBehavior.AvailableCardSetNameList)
			{
				CreatePackButton(availableCardSetName.ShortName);
			}
			if (formatBehavior.IsShowPrizeCardSetFilter)
			{
				CreatePackButton(Data.SystemText.Get("Common_0157"));
			}
			if (formatBehavior.IsShowPhantomCardSetFilter)
			{
				CreatePackButton(Data.SystemText.Get("PhantomCardPackName_short"));
			}
		}
		else
		{
			if (_myRotationFilterType == MyRotationFilterType.CARD_POOL_ALL_PACK || _isMyRotationAllPackVisible)
			{
				foreach (CardSetName availableCardSetName2 in formatBehavior.AvailableCardSetNameList)
				{
					if (!Data.MyRotationAllInfo.DisableCardPackIdList.Contains(availableCardSetName2.ID))
					{
						CreatePackButton(availableCardSetName2.ShortName);
					}
				}
			}
			else
			{
				foreach (string cardPadkId in _myRotationInfo.CardPadkIdList)
				{
					CreatePackButton(Data.Master.CardSetNameMgr.Get(cardPadkId).ShortName);
				}
			}
			if (formatBehavior.IsShowPrizeCardSetFilter)
			{
				CreatePackButton(Data.SystemText.Get("Common_0157"));
			}
		}
		_gridPack.Reposition();
		int num = 1;
		for (int i = 0; i < _packBtnList.Count; i++)
		{
			if (!_packBtnList[i].gameObject.activeSelf)
			{
				continue;
			}
			int num2 = num % _gridPack.maxPerLine;
			if (num2 == 1)
			{
				if (num == _packBtnList.Count)
				{
					_packBtnList[i].normalSprite = "pilltab_02_single_off";
					_packBtnList[i].pressedSprite = "pilltab_02_single_on";
				}
				else
				{
					_packBtnList[i].normalSprite = "pilltab_02_left_off";
					_packBtnList[i].pressedSprite = "pilltab_02_left_on";
				}
			}
			else if (num2 == 0 || i == _packBtnList.Count - 1)
			{
				_packBtnList[i].normalSprite = "pilltab_02_right_off";
				_packBtnList[i].pressedSprite = "pilltab_02_right_on";
			}
			else
			{
				_packBtnList[i].normalSprite = "pilltab_02_middle_off";
				_packBtnList[i].pressedSprite = "pilltab_02_middle_on";
			}
			num++;
		}
		ResetPackButtonFlags();
		UpdatePackButtonSprites();
	}

	private GameObject CreatePackButton(string btnText)
	{
		GameObject obj = NGUITools.AddChild(_gridPack.gameObject, _packBtnOrigin.gameObject);
		obj.SetActive(value: true);
		obj.GetComponentInChildren<UILabel>().text = btnText;
		UIButton component = obj.GetComponent<UIButton>();
		int count = _packBtnList.Count;
		SetEventOnClickPackButton(component, count);
		_packBtnList.Add(component);
		_packButtonFlagList.Add(item: false);
		return obj;
	}

	private void SetEventOnClickPackButton(UIButton button, int index)
	{
		EventDelegate.Add(button.onClick, delegate
		{
			OnClickPackButton(index);
		});
	}

	private void OnClickPackButton(int index)
	{
		if (index == 0)
		{
			ResetPackButtonFlags();

		}
		else
		{
			_packButtonFlagList[0] = false;
			_packButtonFlagList[index] = !_packButtonFlagList[index];
			if (_packButtonFlagList.All((bool flag2) => !flag2))
			{
				_packButtonFlagList[0] = true;
			}
			bool flag = _packButtonFlagList[index];

		}
		UpdatePackButtonSprites();
		this.OnValidate.Call();
	}

	private void ResetPackButtonFlags()
	{
		if (_packButtonFlagList.Count != 0)
		{
			_packButtonFlagList[0] = true;
			int i = 1;
			for (int count = _packButtonFlagList.Count; i < count; i++)
			{
				_packButtonFlagList[i] = false;
			}
		}
	}

	private void UpdatePackButtonSprites()
	{
		int i = 0;
		for (int count = _packBtnList.Count; i < count; i++)
		{
			RenameBtnSprite(_packBtnList[i], _packButtonFlagList[i]);
		}
	}

	private void RemoveCurrentKeyWordList()
	{
		foreach (GameObject currentFilter in _currentFilterList)
		{
			UnityEngine.Object.DestroyImmediate(currentFilter);
		}
		_currentFilterList.Clear();
	}

	private void OnUpdateTypeOrKeyWordFilter()
	{
		_scrollView.RestrictWithinBounds(instant: false);
		_scrollView.UpdateScrollbars(recalculateBounds: true);
	}

	private void RemoveCurrentTypeFilterList()
	{
		foreach (GameObject currentTypeFilter in _currentTypeFilterList)
		{
			UnityEngine.Object.DestroyImmediate(currentTypeFilter);
		}
		_currentTypeFilterList.Clear();
		_typeListGrid.Reposition();
	}

	private List<int> RemoveTokenCard(List<int> cardPool)
	{
		List<int> list = new List<int>(cardPool.Count);
		CardMaster instance = CardMaster.GetInstance(_formatBehavior.CardMasterId);
		foreach (int item in cardPool)
		{
			if (!instance.GetCardParameterFromId(item).IsTokenCard)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void InitializeFilterForDeckEdit(List<int> cardPool, ClassSet classSet, Format format, MyRotationInfo myRotationInfo, MyRotationFilterType myRotationFilterType)
	{
		UIBase_CardManager.FilterParameter filterParameter = new UIBase_CardManager.FilterParameter();
		if (classSet.SubClass == CardBasePrm.ClanType.NONE)
		{
			filterParameter.Class = GetClassFilterMask(CardBasePrm.ClanType.NONE) | GetClassFilterMask(classSet.MainClass);
		}
		else
		{
			filterParameter.Class = GetClassFilterMask(CardBasePrm.ClanType.NONE) | GetClassFilterMask(classSet.MainClass) | GetClassFilterMask(classSet.SubClass);
		}
		filterParameter.Craftable = 1;
		filterParameter.FormatState = format;
		filterParameter.TypeFilter = new List<CardBasePrm.TribeType>();
		filterParameter.FixedClassSet = classSet;
		_myRotationInfo = myRotationInfo;
		if (myRotationInfo != null)
		{
			SetMyRotationFilterParam(filterParameter, myRotationFilterType, myRotationInfo);
		}
		ClassSet = classSet;
		cardPool = RemoveTokenCard(cardPool);
		UpdateAllSelectableKeyWord(UIManager.GetInstance().getUIBase_CardManager().SelectCardIDInConditionMask(cardPool, filterParameter, _formatBehavior, myRotationInfo, alreadySorted: true));
		SetTypeFilterSetting(cardPool, filterParameter, myRotationFilterType);
		UIManager.GetInstance().getUIBase_CardManager().AddKeyWordCache(cardPool, _formatBehavior.CardMasterId);
	}

	public static void SetMyRotationFilterParam(UIBase_CardManager.FilterParameter filter, MyRotationFilterType myRotetionFilterType, MyRotationInfo myRotationInfo)
	{
		if (myRotetionFilterType != MyRotationFilterType.DECK)
		{
			filter.DisableCardSetidList = Data.MyRotationAllInfo.DisableCardPackIdList;
		}
		IFormatBehavior defaultBehaviour = FormatBehaviorManager.GetDefaultBehaviour(Format.MyRotation);
		switch (myRotetionFilterType)
		{
		case MyRotationFilterType.DECK:
			filter.CardSetIdList2 = new List<string>();
			break;
		case MyRotationFilterType.CARD_POOL_ALL_PACK:
		{
			List<CardSetName> availableCardSetNameList = defaultBehaviour.AvailableCardSetNameList;
			List<string> list = new List<string>();
			foreach (CardSetName item in availableCardSetNameList)
			{
				if (!Data.MyRotationAllInfo.DisableCardPackIdList.Contains(item.ID))
				{
					list.Add(item.ID);
				}
			}
			filter.CardSetIdList2 = list;
			break;
		}
		case MyRotationFilterType.CARD_POOL_SELECT_ONLY:
			filter.CardSetIdList2 = myRotationInfo.CardPadkIdList;
			break;
		}
	}

	public void UpdateTypeFilterForDeckEdit(List<int> cardPool, ClassSet classSet, Format format, MyRotationInfo myRotationInfo, MyRotationFilterType myRotationFilterType)
	{
		InitializeFilterForDeckEdit(cardPool, classSet, format, myRotationInfo, myRotationFilterType);
	}

	private void SetTypeFilterSetting(List<int> cardPool, UIBase_CardManager.FilterParameter filter, MyRotationFilterType myRotationFilterType)
	{
		_typeFilterList = new List<CardBasePrm.TribeType>();
		IList<int> list = UIManager.GetInstance().getUIBase_CardManager().SelectCardIDInConditionMask(cardPool, filter, _formatBehavior, _myRotationInfo, alreadySorted: true);
		List<CardParameter> list2 = new List<CardParameter>(list.Count);
		CardMaster instance = CardMaster.GetInstance(_formatBehavior.CardMasterId);
		foreach (int item in list)
		{
			list2.Add(instance.GetCardParameterFromId(item));
		}
		foreach (CardBasePrm.TribeType value in Enum.GetValues(typeof(CardBasePrm.TribeType)))
		{
			if (value == CardBasePrm.TribeType.ALL || value == CardBasePrm.TribeType.MAX)
			{
				continue;
			}
			foreach (CardParameter item2 in list2)
			{
				if (!item2.IsTribeAll && item2.Tribe.Contains(value))
				{
					_typeFilterList.Add(value);
					break;
				}
			}
		}
	}

	private void UpdateAllSelectableKeyWord(IList<int> cardPool)
	{
		_existKeyWordList = new Dictionary<string, bool>();
		CardMaster instance = CardMaster.GetInstance(_formatBehavior.CardMasterId);
		foreach (int item in cardPool)
		{
			CardParameter cardParameterFromId = instance.GetCardParameterFromId(item);
			foreach (string item2 in UIManager.GetInstance().getUIBase_CardManager().GetKeyword(cardParameterFromId))
			{
				_existKeyWordList[item2] = true;
			}
		}
	}
}
