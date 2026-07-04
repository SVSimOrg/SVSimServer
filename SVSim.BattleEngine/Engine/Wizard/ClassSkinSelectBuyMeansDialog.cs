using System;
using Cute;
using UnityEngine;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

namespace Wizard;

public class ClassSkinSelectBuyMeansDialog : BaseSelectBuyMeansDialog
{
	[SerializeField]
	private SkinProductDetail _skinProductDetail;

	[SerializeField]
	private UITexture _leaderSkinTicketIcon;

	public void Init(SkinSeriesPurchaseInfo sInfo, DialogBase dialog, Action onPushBuyCrystalBtnCallBack, Action onPushBuyRupyBtnCallBack, Action onPushBuyTicketButtonCallBack)
	{
		_Init(sInfo.saleInfo, dialog, onPushBuyCrystalBtnCallBack, onPushBuyRupyBtnCallBack, onPushBuyTicketButtonCallBack);
		_skinProductDetail.SetMultiProductDetail(sInfo);
		if (sInfo.saleInfo.costTicketItemId.HasValue)
		{
			_leaderSkinTicketIcon.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(ShopCommonUtility.GetTicketIconPath(sInfo.saleInfo.costTicketItemId.Value.ToString(), isFetch: true));
		}
	}

	public void Init(SkinProductInfo pInfo, DialogBase dialog, Action onPushBuyCrystalBtnCallBack, Action onPushBuyRupyBtnCallBack, Action onPushBuyTicketButtonCallBack)
	{
		_Init(pInfo.saleInfo, dialog, onPushBuyCrystalBtnCallBack, onPushBuyRupyBtnCallBack, onPushBuyTicketButtonCallBack);
		if (pInfo.rewardInfoList.FindIndex((ShopCommonRewardInfo rewardInfo) => rewardInfo.IsAlreadyGet) >= 0)
		{
			SetDescriptionLabel(Data.SystemText.Get("Shop_0238", GetDescriptionText(pInfo.saleInfo)));
		}
		_skinProductDetail.SetSingleProductDetail(pInfo);
		if (pInfo.IsEnableBuyTicket)
		{
			_leaderSkinTicketIcon.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(ShopCommonUtility.GetTicketIconPath(pInfo.saleInfo.costTicketItemId.Value.ToString(), isFetch: true));
		}
	}
}
