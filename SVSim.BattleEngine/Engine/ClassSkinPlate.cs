using System;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class ClassSkinPlate : MonoBehaviour
{

	private readonly Vector3 POS_VIEW_SINGLE_PRODUCT_NAME = new Vector3(13f, -52f, 0f);

	private readonly Vector3 POS_VIEW_MULTI_PRODUCT_NAME = new Vector3(0f, -52f, 0f);

	[SerializeField]
	private UIEventListener _eventListenerSkinImage;

	[SerializeField]
	private UISprite _spritePlateBG;

	[SerializeField]
	private UILabel _labelFree;

	[SerializeField]
	private UILabel _labelCostCrystal;

	[SerializeField]
	private UILabel _labelCostRupy;

	[SerializeField]
	private UILabel _labelCostTicket;

	[SerializeField]
	private UIButton m_BtnBuy;

	[SerializeField]
	private UILabel m_LabelBuy;

	[SerializeField]
	private UILabel m_LabelPurchased;

	[SerializeField]
	private UILabel _LabelProductName;

	[SerializeField]
	private UITexture _uiClassSkinTexture;

	[SerializeField]
	private UITexture _uiClassSkinTextureLarge;

	[SerializeField]
	private UISprite _spriteClassColorIcon;

	[SerializeField]
	private UITexture _leaderSkinTicketIcon;

	public SkinProductInfo ProductInfo { get; private set; }

	public SkinSeriesPurchaseInfo SeriesInfo { get; private set; }

	public void SetMultiData(SkinSeriesPurchaseInfo seriesInfo, EventDelegate onPushBuyBtnCallback = null, Action onTapSkinImage = null)
	{
		SetBuyButtonToGrey(isGrey: false);
		bool isLargeImage = Data.Master.LeaderSkinSeriesIdDic[seriesInfo.series_id].IsLargeImage;
		ProductInfo = null;
		SeriesInfo = seriesInfo;
		Texture mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(seriesInfo.saleInfo.path, ResourcesManager.AssetLoadPathType.ShopClassSkin, isfetch: true));
		if (isLargeImage)
		{
			_uiClassSkinTexture.gameObject.SetActive(value: false);
			_uiClassSkinTextureLarge.gameObject.SetActive(value: true);
			_uiClassSkinTextureLarge.mainTexture = mainTexture;
			_spritePlateBG.width = 411;
			int maxLength = (Global.IsAlphabetLanguage() ? 39 : 20);
			_LabelProductName.text = ShopCommonUtility.TrimProductName(seriesInfo.saleInfo.name, maxLength);
		}
		else
		{
			_uiClassSkinTexture.gameObject.SetActive(value: true);
			_uiClassSkinTextureLarge.gameObject.SetActive(value: false);
			_uiClassSkinTexture.mainTexture = mainTexture;
			_spritePlateBG.width = 357;
			_LabelProductName.text = ShopCommonUtility.TrimProductName(seriesInfo.saleInfo.name);
		}
		_LabelProductName.transform.localPosition = POS_VIEW_MULTI_PRODUCT_NAME;
		_LabelProductName.effectStyle = UILabel.Effect.None;
		_spriteClassColorIcon.gameObject.SetActive(value: false);
		m_BtnBuy.onClick.Clear();
		m_BtnBuy.onClick.Add(onPushBuyBtnCallback);
		_eventListenerSkinImage.onClick = null;
		_eventListenerSkinImage.onClick = delegate
		{
			onTapSkinImage.Call();
		};
		if (seriesInfo.saleInfo.costTicketItemId.HasValue)
		{
			_leaderSkinTicketIcon.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(ShopCommonUtility.GetTicketIconPath(seriesInfo.saleInfo.costTicketItemId.Value.ToString(), isFetch: true));
		}
		if (seriesInfo.is_completed && seriesInfo._rewardStatus != SkinSeriesPurchaseInfo.RewardStatus.not_got)
		{
			_labelCostCrystal.gameObject.SetActive(value: false);
			_labelCostRupy.gameObject.SetActive(value: false);
			_labelFree.gameObject.SetActive(value: false);
			_labelCostTicket.gameObject.SetActive(value: false);
		}
		else
		{
			ShopCommonUtility.SetCostInfo(seriesInfo.saleInfo, _labelCostCrystal, _labelCostRupy, _labelFree, _labelCostTicket);
		}
		_SetMultiBuyButton(seriesInfo);
	}

	public void SetBuyButtonToGrey(bool isGrey)
	{
		UIManager.SetObjectToGrey(m_BtnBuy.gameObject, isGrey);
	}

	public void SetData(SkinProductInfo productInfo, EventDelegate onPushBuyBtnCallback = null, Action onTapSkinImage = null)
	{
		SeriesInfo = null;
		ProductInfo = productInfo;
		ClassCharacterMasterData charaPrmBySkinId = null; // Pre-Phase-5b: no chara master headless
		_LabelProductName.transform.localPosition = POS_VIEW_SINGLE_PRODUCT_NAME;
		int maxLength = (Global.IsAlphabetLanguage() ? 35 : 15);
		_LabelProductName.text = ShopCommonUtility.TrimProductName(productInfo.saleInfo.name, maxLength);
		ClassCharaPrm.SetClassLabelSetting(_LabelProductName, charaPrmBySkinId.ClassColorId);
		_uiClassSkinTextureLarge.gameObject.SetActive(value: false);
		_uiClassSkinTexture.gameObject.SetActive(value: true);
		_uiClassSkinTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(productInfo.saleInfo.path, ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail, isfetch: true));
		_spritePlateBG.width = 357;
		_spriteClassColorIcon.gameObject.SetActive(value: true);
		_spriteClassColorIcon.spriteName = ClassCharaPrm.GetIconSpriteName(charaPrmBySkinId.clan);
		m_BtnBuy.onClick.Clear();
		m_BtnBuy.onClick.Add(onPushBuyBtnCallback);
		_eventListenerSkinImage.onClick = null;
		_eventListenerSkinImage.onClick = delegate
		{
			onTapSkinImage.Call();
		};
		if (productInfo.IsEnableBuyTicket)
		{
			_leaderSkinTicketIcon.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(ShopCommonUtility.GetTicketIconPath(productInfo.saleInfo.costTicketItemId.Value.ToString(), isFetch: true));
		}
		ShopCommonUtility.SetCostInfo(productInfo.saleInfo, _labelCostCrystal, _labelCostRupy, _labelFree, _labelCostTicket);
		_SetBuyButton(productInfo);
	}

	private void _SetBuyButton(SkinProductInfo productInfo)
	{
		if (!productInfo.is_purchased)
		{
			m_BtnBuy.gameObject.SetActive(value: true);
			m_BtnBuy.isEnabled = true;
			m_LabelPurchased.gameObject.SetActive(value: false);
			if (productInfo.saleInfo.isFree)
			{
				m_LabelBuy.text = Data.SystemText.Get("Shop_0099");
			}
			else
			{
				m_LabelBuy.text = Data.SystemText.Get("Shop_0095");
			}
		}
		else
		{
			m_BtnBuy.gameObject.SetActive(value: false);
			m_LabelPurchased.gameObject.SetActive(value: true);
		}
	}

	private void _SetMultiBuyButton(SkinSeriesPurchaseInfo seriesInfo)
	{
		if (seriesInfo.is_completed && seriesInfo._rewardStatus != SkinSeriesPurchaseInfo.RewardStatus.not_got)
		{
			m_BtnBuy.gameObject.SetActive(value: false);
			m_LabelPurchased.gameObject.SetActive(value: true);
			return;
		}
		m_BtnBuy.gameObject.SetActive(value: true);
		m_BtnBuy.isEnabled = true;
		m_LabelPurchased.gameObject.SetActive(value: false);
		if (seriesInfo.saleInfo.isFree)
		{
			m_LabelBuy.text = Data.SystemText.Get("Shop_0099");
		}
		else
		{
			m_LabelBuy.text = Data.SystemText.Get("Shop_0095");
		}
	}
}
