using System;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class CardObject
{
	public class UseInfo
	{
		public GameObject Parent;

		public UIWidget CardInfoObj;

		public UILabel CardInfoText;

		public CardListTemplate CardTemplateObj;

		public GameObject BlackOut;

		public UseInfo(GameObject parent, CardMaster.CardMasterId cardMasterId)
		{
			Parent = parent;
			CardInfoObj = parent.transform.Find("Info").GetComponent<UIWidget>();
			CardInfoText = CardInfoObj.transform.Find("Text").GetComponent<UILabel>();
			BlackOut = parent.transform.Find("BlackoutPanel").gameObject;
			CardTemplateObj = parent.transform.parent.gameObject.GetComponent<CardListTemplate>();
			CardInfoObj.alpha = 0f;
			BlackOut.gameObject.SetActive(value: false);
			CardTemplateObj.AttachShaders(cardMasterId);
		}

		public void SetInfo(int haveNum, int haveNumWithFoil, int usedNum, int usedNumWithFoil, bool isMaintenance, int sameKindNumMax, CardMaster.CardMasterId cardMasterId)
		{
			if ((bool)CardInfoObj)
			{
				SystemText systemText = Data.SystemText;
				if (isMaintenance)
				{
					SetMaintenance();
				}
				else if (usedNumWithFoil <= 0)
				{
					TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 0f);
				}
				else if (usedNumWithFoil < sameKindNumMax)
				{
					TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 1f);
					CardInfoText.text = systemText.Get("Card_0071", usedNumWithFoil.ToString());
				}
				else
				{
					TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 1f);
					CardInfoText.text = systemText.Get("Card_0072", sameKindNumMax.ToString());
				}
				if (haveNum <= 0)
				{
					CardTemplateObj.AttachGrayShader();
				}
				else
				{
					CardTemplateObj.AttachShaders(cardMasterId);
				}
				BlackOut.gameObject.SetActive(haveNum > 0 && (usedNumWithFoil >= Mathf.Min(sameKindNumMax, haveNumWithFoil) || usedNum >= haveNum || isMaintenance));
			}
		}

		public void SetMaintenance()
		{
			TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 1f);
			CardInfoText.text = Data.SystemText.Get("System_0022");
		}

		public void SetBanCard()
		{
			TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 1f);
			CardTemplateObj.AttachRedMaterial();
			CardInfoText.text = Data.SystemText.Get("Card_0189");
		}

		public void UpdateSameKindNumMaxCard(bool isOverNum, CardMaster.CardMasterId cardMasterId, bool isNonPossessionCard)
		{
			if (isOverNum)
			{
				TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 1f);
				CardTemplateObj.AttachRedMaterial();
				CardInfoText.text = Data.SystemText.Get("Card_0190");
				return;
			}
			TweenAlpha.Begin(CardInfoObj.gameObject, 0.3f, 0f);
			if (isNonPossessionCard)
			{
				CardTemplateObj.AttachGrayShader();
			}
			else
			{
				CardTemplateObj.AttachShaders(cardMasterId);
			}
		}
	}

	private static readonly Vector3 SLEEVE_POSITION = new Vector3(0f, -0.5f, 0f);

	private static readonly Vector3 SLEEVE_SCALE = new Vector3(1.025f, 1.025f, 1f);

	private Transform _parent;

	private GameObject _sleeveParent;

	private UITexture _sleeve;

	private UseInfo _info;

	private GameObject _cursorEffect;

	private float _cardScale;

	private readonly IFormatBehavior _formatBehavior;

	private UIBase_CardManager.CardObjData BaseCard { get; set; }

	public bool IsAttachedCardObjData => BaseCard.Cost != null;

	public GameObject CardObj
	{
		get
		{
			return BaseCard.CardObj;
		}
		set
		{
			BaseCard.CardObj = value;
			AttachParent(_parent);
		}
	}

	public int MainCardNum
	{
		get
		{
			return BaseCard.mainCardNum;
		}
		set
		{
			BaseCard.mainCardNum = value;
			UpdateCardNumLabel();
		}
	}

	public int SubCardNum
	{
		get
		{
			return BaseCard.subCardNum;
		}
		set
		{
			BaseCard.subCardNum = value;
			UpdateCardNumLabel();
		}
	}

	public int TotalCardNum => BaseCard.TotalCardNum;

	public int CardId
	{
		get
		{
			return BaseCard.ids;
		}
		set
		{
			BaseCard.ids = value;
		}
	}

	public bool IsVisibleSleeve { get; private set; }

	public bool IsWaitingRotateAnimation { get; private set; }

	public bool IsVisibleCursorEffect
	{
		get
		{
			if (_cursorEffect != null)
			{
				return _cursorEffect.activeSelf;
			}
			return false;
		}
	}

	public bool IsDisplaySpotCardNum { get; set; }

	public bool IsHideZeroSpotCardNum { get; set; }

	public bool IsNonPossessionCard { get; set; }

	public CardObject(UIBase_CardManager.CardObjData card, Transform parent, float scale, IFormatBehavior formatBehavior, bool isDisplaySpotCardNum = false, bool isHideZeroSpotCardNum = false, bool isNonPossessionCard = false)
	{
		_formatBehavior = formatBehavior;
		_cardScale = scale;
		IsDisplaySpotCardNum = isDisplaySpotCardNum && !isNonPossessionCard;
		IsHideZeroSpotCardNum = isHideZeroSpotCardNum;
		IsNonPossessionCard = isNonPossessionCard;
		AttachParent(parent);
		AttachCardObjData(card);
	}

	public CardObject(Transform parent, float scale, IFormatBehavior formatBehavior, bool isDisplaySpotCardNum = false, bool isHideZeroSpotCardNum = false, bool isNonPossessionCard = false)
	{
		_formatBehavior = formatBehavior;
		_cardScale = scale;
		IsDisplaySpotCardNum = isDisplaySpotCardNum && !isNonPossessionCard;
		IsHideZeroSpotCardNum = isHideZeroSpotCardNum;
		IsNonPossessionCard = isNonPossessionCard;
		AttachParent(parent);
		AttachCardObjData(new UIBase_CardManager.CardObjData());
	}

	public void UpdateCardNumLabel()
	{
		if (BaseCard.CardObj != null)
		{
			CardListTemplate component = BaseCard.CardObj.GetComponent<CardListTemplate>();
			if ((bool)component)
			{
				component.IsIncludingSpotCard = IsIncludingSpotCard();
				component.SetNum(BaseCard.mainCardNum, BaseCard.subCardNum);
			}
		}
	}

	private bool IsIncludingSpotCard()
	{
		if (false /* Pre-Phase-5b: no spot card headless */)
		{
			if (IsHideZeroSpotCardNum)
			{
				return BaseCard.subCardNum > 0;
			}
			return true;
		}
		return false;
	}

	public void AttachParent(Transform parent)
	{
		_parent = parent;
		if (BaseCard != null)
		{
			AttachCardObjData(BaseCard);
		}
	}

	public void AttachCardObjData(UIBase_CardManager.CardObjData card)
	{
		BaseCard = card;
		if (BaseCard.CardObj != null)
		{
			int width = 0;
			int height = 0;
			if (_sleeve != null)
			{
				width = _sleeve.width;
				height = _sleeve.height;
			}
			bool activeSelf = BaseCard.CardObj.activeSelf;
			BaseCard.CardObj.transform.parent = _parent;
			BaseCard.CardObj.SetActive(!activeSelf);
			BaseCard.CardObj.SetActive(activeSelf);
			BaseCard.CardObj.transform.localScale = Vector3.one * _cardScale;
			if (_sleeveParent != null)
			{
				_sleeveParent.transform.parent = BaseCard.CardObj.transform;
				_sleeveParent.transform.localPosition = Vector3.zero;
				_sleeveParent.transform.localScale = Vector3.one;
			}
			if (_sleeve != null)
			{
				_sleeve.MakePixelPerfect();
				_sleeve.width = width;
				_sleeve.height = height;
				_sleeve.transform.localPosition = SLEEVE_POSITION;
				_sleeve.transform.localScale = SLEEVE_SCALE;
			}
			CardListTemplate component = BaseCard.CardObj.GetComponent<CardListTemplate>();
			component.SetId(card.ids);
			component.IsIncludingSpotCard = IsIncludingSpotCard();
			component.ChangeShowNumType(isNormal: false);
		}
	}

	public void ShowSleeve(UITexture original)
	{
		if (_sleeve == null)
		{
			_sleeveParent = (BaseCard.CardObj = new GameObject());
			_sleeveParent.name = "SleeveParent";
			Transform transform = _sleeveParent.transform;
			transform.parent = _parent;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.identity;
			_sleeve = UnityEngine.Object.Instantiate(original);
			_sleeve.name = "Sleeve";
			Transform transform2 = _sleeve.transform;
			transform2.parent = _sleeveParent.transform;
			transform2.localPosition = SLEEVE_POSITION;
			transform2.localScale = SLEEVE_SCALE * _cardScale;
			transform2.localRotation = Quaternion.identity;
		}
		_sleeve.gameObject.SetActive(value: true);
		_sleeve.alpha = 0f;
		BaseCard.CardObj.transform.parent = _parent;
		TweenAlpha.Begin(BaseCard.CardObj, 0.18f, 1f);
		IsVisibleSleeve = true;
	}

	public void TakeOffSleeve()
	{
		if (_sleeve != null)
		{
			_sleeve.gameObject.SetActive(value: false);
		}
		ActiveCullObjs(isActive: true);
		IsVisibleSleeve = false;
	}

	public void CompleteSleeveTweenAlpha()
	{
		if (!(_sleeve == null))
		{
			TweenAlpha component = _sleeve.transform.parent.GetComponent<TweenAlpha>();
			if (!(component == null) && component.enabled)
			{
				component.value = component.to;
				component.enabled = false;
			}
		}
	}

	public void ActiveCullObjs(bool isActive)
	{
		if ((bool)BaseCard.CardObj)
		{
			CardListTemplate component = BaseCard.CardObj.GetComponent<CardListTemplate>();
			if ((bool)component)
			{
				component.SetFace(isActive);
			}
		}
	}

	public void RotateAnim(Action<CardObject> on90Degree = null, Action<CardObject> onFinish = null)
	{
		if (!BaseCard.CardObj)
		{
			return;
		}
		IsWaitingRotateAnimation = true;
		TweenRotation tween = TweenRotation.Begin(BaseCard.CardObj, 0.125f, Quaternion.AngleAxis(90f, Vector3.up));
		EventDelegate ev = null;
		ev = new EventDelegate(delegate
		{
			tween.RemoveOnFinished(ev);
			on90Degree.Call(this);
			if ((bool)BaseCard.CardObj)
			{
				tween = TweenRotation.Begin(BaseCard.CardObj, 0.125f, Quaternion.AngleAxis(0f, Vector3.up));
				ev = new EventDelegate(delegate
				{
					tween.RemoveOnFinished(ev);
					onFinish.Call(this);
					IsWaitingRotateAnimation = false;
				});
				tween.AddOnFinished(ev);
			}
		});
		tween.AddOnFinished(ev);
	}

	public void NotifyRotateAnimation()
	{
		IsWaitingRotateAnimation = true;
	}

	private GameObject CreateCardInfo(GameObject original)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		gameObject.name = "CardInfo";
		gameObject.gameObject.SetActive(value: true);
		Vector3 localPosition = gameObject.transform.localPosition;
		gameObject.transform.parent = BaseCard.CardObj.transform;
		gameObject.transform.localPosition = localPosition;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private void PrepareCardInfo(GameObject originalCardInfo)
	{
		if (_info == null && !(originalCardInfo == null))
		{
			GameObject parent = CreateCardInfo(originalCardInfo);
			_info = new UseInfo(parent, _formatBehavior.CardMasterId);
		}
	}

	public void UpdateCardInfo(GameObject original, int haveNum, int haveNumWithFoil, int usedNum, int usedNumWithFoil, int sameKindNumMax, bool isMaintenance)
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		MainCardNum = Mathf.Max(_formatBehavior.GetPossessionCardNum(BaseCard.ids, isIncludingSpotCard: false) - usedNum, 0);
		SubCardNum = Mathf.Max(Mathf.Min(haveNum - usedNum, dataMgr.SpotCardData.GetSpotCardNum(BaseCard.ids)), 0);
		PrepareCardInfo(original);
		_info.SetInfo(haveNum, haveNumWithFoil, usedNum, usedNumWithFoil, isMaintenance, sameKindNumMax, _formatBehavior.CardMasterId);
		VisibleCardNumLabel(haveNum > 0);
	}

	public void SetCardToMaintenance(GameObject original)
	{
		PrepareCardInfo(original);
		_info.SetMaintenance();
	}

	public void SetCardToBanCard(GameObject original = null)
	{
		PrepareCardInfo(original);
		_info.SetBanCard();
	}

	public void UpdateSameKindNumMaxCard(bool isOverNum, GameObject original = null)
	{
		PrepareCardInfo(original);
		_info.UpdateSameKindNumMaxCard(isOverNum, _formatBehavior.CardMasterId, IsNonPossessionCard);
	}

	public void ActiveCardInfo(bool isActive)
	{
		if (_info != null)
		{
			_info.Parent.SetActive(isActive);
		}
	}

	public void ResetMaterial()
	{
		if (BaseCard != null && !(BaseCard.CardObj == null))
		{
			CardListTemplate component = BaseCard.CardObj.GetComponent<CardListTemplate>();
			if (!(component == null))
			{
				CardParameter cardParameterFromId = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(CardId);
				CardParameter cardParameterFromId2 = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(BaseCard.ids);
				ResourcesManager.AssetLoadPathType type = ((cardParameterFromId.CharType == CardBasePrm.CharaType.NORMAL) ? ResourcesManager.AssetLoadPathType.UnitCardMaterial : ResourcesManager.AssetLoadPathType.SpellCardMaterial);
				Material material = Toolbox.ResourcesManager.FindCardMaterial(cardParameterFromId2.ResourceCardId, type);
				component._cardTexture.mainTexture = null;
				component._cardTexture.material = material;
			}
		}
	}

	public void VisibleCardNumLabel(bool isVisible)
	{
		CardListTemplate component = CardObj.GetComponent<CardListTemplate>();
		if ((bool)component)
		{
			if (isVisible)
			{
				component.ShowNum();
			}
			else
			{
				component.HideNum();
			}
		}
	}

	public void CreateCursorEffect()
	{
		if (BaseCard != null && BaseCard.CardObj != null)
		{
			string path = "cmn_frame_card_1";
			switch (BaseCard.cardType)
			{
			case CardBasePrm.CharaType.SPELL:
				path = "cmn_frame_card_3";
				break;
			case CardBasePrm.CharaType.FIELD:
			case CardBasePrm.CharaType.CHANT_FIELD:
				path = "cmn_frame_card_2";
				break;
			}
			_cursorEffect = UnityEngine.Object.Instantiate(Toolbox.ResourcesManager.LoadObject<GameObject>(Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true)));
			MotionUtils.ChangeParticleSystemColor(_cursorEffect, Color.cyan);
			_cursorEffect.transform.parent = BaseCard.CardObj.transform;
			_cursorEffect.transform.localPosition = Vector3.forward * -1f;
			_cursorEffect.transform.localRotation = Quaternion.identity;
			_cursorEffect.transform.localScale = Vector3.one * 500f;
			_cursorEffect.layer = BaseCard.CardObj.layer;
			Transform[] componentsInChildren = _cursorEffect.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = BaseCard.CardObj.layer;
			}
		}
	}

	public void ChangeSelectingState(bool isSelect)
	{
		if (BaseCard != null && BaseCard.CardObj != null)
		{
			if (_cursorEffect == null)
			{
				CreateCursorEffect();
			}
			ActiveCursorEffect(isSelect);
			Vector3 scale = Vector3.one * (isSelect ? (_cardScale * 1.2f) : _cardScale);
			TweenScale.Begin(BaseCard.CardObj, 0.1f, scale);
		}
	}

	public void ActiveCursorEffect(bool isActive)
	{
		if (_cursorEffect != null)
		{
			_cursorEffect.SetActive(isActive);
		}
	}

	public void SetScale(float scale)
	{
		_cardScale = scale;
		if (CardObj != null)
		{
			TweenScale component = CardObj.GetComponent<TweenScale>();
			if (component != null)
			{
				component.enabled = false;
			}
			CardObj.transform.localScale = Vector3.one * _cardScale;
		}
	}

	public CardObject Clone(Transform parent = null)
	{
		if (parent == null)
		{
			parent = _parent;
		}
		UIBase_CardManager.CardObjData cardObjData = new UIBase_CardManager.CardObjData();
		UIBase_CardManager.cardDataCopy(cardObjData, BaseCard);
		return new CardObject(cardObjData, parent, _cardScale, _formatBehavior, IsDisplaySpotCardNum, IsHideZeroSpotCardNum, IsNonPossessionCard);
	}

	public void Destroy(bool isRemoveAsset = true)
	{
		UnityEngine.Object.Destroy(CardObj);
		if (_info != null && (bool)_info.Parent)
		{
			UnityEngine.Object.Destroy(_info.Parent);
		}
		if (isRemoveAsset)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstance(_formatBehavior.CardMasterId).GetCardParameterFromId(CardId);
			Toolbox.ResourcesManager.RemoveAsset(Toolbox.ResourcesManager.GetAssetTypePath(cardParameterFromId.ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial));
			if (cardParameterFromId.IsFoil)
			{
				int normalCardId = cardParameterFromId.NormalCardId;
				Toolbox.ResourcesManager.RemoveAsset(Toolbox.ResourcesManager.GetAssetTypePath(normalCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial));
			}
		}
	}

	public void DestroySleeve()
	{
		if (_sleeveParent != null)
		{
			UnityEngine.Object.Destroy(_sleeveParent);
		}
	}

	public void DestroyUseInfo()
	{
		if (_info != null)
		{
			UnityEngine.Object.Destroy(_info.Parent);
			_info = null;
		}
	}

	public void DestroyCursorEffect()
	{
		if (_cursorEffect != null)
		{
			UnityEngine.Object.Destroy(_cursorEffect);
		}
	}

	public void DestroyTween()
	{
		if (BaseCard != null && BaseCard.CardObj != null)
		{
			TweenAlpha component = BaseCard.CardObj.GetComponent<TweenAlpha>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			TweenRotation component2 = BaseCard.CardObj.GetComponent<TweenRotation>();
			if (component2 != null)
			{
				UnityEngine.Object.Destroy(component2);
			}
			BaseCard.CardObj.transform.localRotation = Quaternion.identity;
		}
	}

	public void AttachColorShader()
	{
		CardListTemplate component = CardObj.GetComponent<CardListTemplate>();
		if (!(component == null))
		{
			component.AttachShaders(_formatBehavior.CardMasterId);
		}
	}

	public void AttachGrayShader()
	{
		CardListTemplate component = CardObj.GetComponent<CardListTemplate>();
		if (!(component == null))
		{
			component.AttachGrayShader();
		}
	}

	public void AttachRedShader()
	{
		CardListTemplate component = CardObj.GetComponent<CardListTemplate>();
		if (!(component == null))
		{
			component.AttachRedMaterial();
		}
	}
}
