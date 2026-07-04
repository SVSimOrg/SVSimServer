using System;
using Cute;
using UnityEngine;
using Wizard.ErrorDialog;

namespace Wizard;

public static class ShopCommonUtility
{
	public enum SalesType
	{
		free,
		crystal,
		rupy,
		ticket
	}

	private static readonly Vector3 POS_COST_LABLE_RIGHT = new Vector3(40f, -86f, 0f);

	private static readonly Vector3 POS_COST_LABLE_LEFT = new Vector3(-141f, -86f, 0f);

	private static readonly Vector3 POS_COST_LABLE_CENTER = new Vector3(-47f, -86f, 0f);

	public static void SetButtonLabelStyle(UIButton button, UILabel label)
	{
		if (button.isEnabled)
		{
			label.color = LabelDefine.TEXT_COLOR_BUTTON_ENABLE;
		}
		else
		{
			label.color = LabelDefine.TEXT_COLOR_BUTTON_DISABLE;
		}
	}

	public static bool IsHaveEnoughCost(ShopCommonSaleInfo info, SalesType costType, Action funcCrystalShortage)
	{
		switch (costType)
		{
		case SalesType.free:
			if (!info.isFree)
			{
				return false;
			}
			break;
		case SalesType.crystal:
			if (!info.costCrystal.HasValue)
			{
				return false;
			}
			if (PlayerStaticData.UserCrystalCount < info.costCrystal)
			{
				funcCrystalShortage.Call();
				return false;
			}
			break;
		case SalesType.rupy:
			if (!info.costRupy.HasValue)
			{
				return false;
			}
			if (PlayerStaticData.UserRupyCount < info.costRupy)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public static DialogBase CreateBasePopupPurchaseConfirm(EventDelegate del_OkBtn)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(Data.SystemText.Get("Shop_0082"));
		dialogBase.SetButtonDelegate(del_OkBtn);
		dialogBase.SetPanelDepth(100);
		return dialogBase;
	}

	public static DialogBase CreatePurchaseConfirmPopup(ShopCommonSaleInfo info, SalesType costType, PurchaseConfirm prefabPurchaseConfirm, Action buyApiFunc, string warningTextId = null)
	{
		DialogBase dialogBase = CreateBasePopupPurchaseConfirm(new EventDelegate(delegate
		{
			buyApiFunc.Call();
		}));
		PurchaseConfirm purchaseConfirm = UnityEngine.Object.Instantiate(prefabPurchaseConfirm);
		dialogBase.SetObj(purchaseConfirm.gameObject);
		string purchaseText = Data.SystemText.Get("Shop_0101", info.name.Replace("\n", ""));
		switch (costType)
		{
		case SalesType.crystal:
			purchaseConfirm.SetClystalConfirmDialog(info.costCrystal.Value, purchaseText, PlayerStaticData.UserCrystalCount, info.expirtyTimeInfo);
			break;
		case SalesType.rupy:
			purchaseConfirm.SetRupyConfirmDialog(info.costRupy.Value, purchaseText, PlayerStaticData.UserRupyCount);
			break;
		case SalesType.ticket:
			purchaseConfirm.SetLeaderSkinTicketConfirmDialog(info.costTicket.Value, purchaseText, info.haveTicketNum.Value, info.costTicketItemId.Value);
			break;
		}
		if (warningTextId != null)
		{
			purchaseConfirm.SetWarningTextId(warningTextId);
		}
		return dialogBase;
	}

	public static DialogBase CreateCrystalShortagePopup()
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Shop_0092"));
		dialogBase.SetText(Data.SystemText.Get("Shop_0013"), isWrapText: true);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		dialogBase.SetPanelDepth(100);
		return dialogBase;
	}

	public static DialogBase CreatePurchaseSuccess(string productName)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetText(Data.SystemText.Get("Shop_0022", productName), isWrapText: true);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		return dialogBase;
	}

	public static void GetRewardNames(ShopCommonRewardInfo rewardInfo, out string typeName, out string detailName)
	{
		int type = rewardInfo.Type;
		long userGoodsId = rewardInfo.UserGoodsId;
		int num = rewardInfo.Num;
		GetRewardNames(type, userGoodsId, num, out typeName, out detailName);
	}

	public static void GetRewardNames(int type, long userGoodsId, int num, out string typeName, out string detailName)
	{
		typeName = "";
		detailName = "";
		switch ((UserGoods.Type)type)
		{
		case UserGoods.Type.RedEther:
			typeName = Data.SystemText.Get("Common_0205");
			detailName = num + Data.SystemText.Get("Common_0116");
			break;
		case UserGoods.Type.Crystal:
			typeName = Data.SystemText.Get("Common_0201");
			detailName = num + Data.SystemText.Get("Common_0116");
			break;
		case UserGoods.Type.Card:
		{
			typeName = Data.SystemText.Get("Common_0204");
			CardParameter cardParameterFromId = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId((int)userGoodsId);
			detailName = cardParameterFromId.CardName + num + Data.SystemText.Get("Common_0117");
			break;
		}
		case UserGoods.Type.Sleeve:
			typeName = ((!Data.Master.SleeveMgr.Get(userGoodsId).IsPremiumSleeve) ? Data.SystemText.Get("Common_0203") : Data.SystemText.Get("Common_0160"));
			if (Data.Master.SleeveMgr.IsContainsInMaster(userGoodsId))
			{
				detailName = Data.Master.SleeveMgr.Get(userGoodsId).sleeve_name;
			}
			else
			{
				detailName = "";
			}
			break;
		case UserGoods.Type.Emblem:
			typeName = Data.SystemText.Get("Common_0145");
			if (Data.Master.EmblemMgr.IsContainsInMaster(userGoodsId))
			{
				detailName = Data.Master.EmblemMgr.Get(userGoodsId)._name;
			}
			else
			{
				detailName = "";
			}
			break;
		case UserGoods.Type.Degree:
			typeName = Data.SystemText.Get("Common_0144");
			if (Data.Master.DegreeMgr.IsContainsInMaster((int)userGoodsId))
			{
				detailName = Data.Master.DegreeMgr.Get((int)userGoodsId)._name;
			}
			else
			{
				detailName = "";
			}
			break;
		case UserGoods.Type.Rupy:
			typeName = Data.SystemText.Get("Common_0115");
			detailName = num + Data.SystemText.Get("Common_0120");
			break;
		case UserGoods.Type.Item:
		{
			string text = Data.SystemText.Get("Common_0117");
			foreach (Item item in Data.Master.ItemList)
			{
				if (item.UserGoodsId == userGoodsId)
				{
					typeName = item.name;
					text = item.unit;
				}
			}
			detailName = num + text;
			break;
		}
		case UserGoods.Type.Skin:
			typeName = Data.SystemText.Get("Common_0143");
			detailName = ""; // Pre-Phase-5b: no chara master headless
			break;
		case UserGoods.Type.SpotCardPoint:
			typeName = Data.SystemText.Get("Common_0161");
			detailName = num + Data.SystemText.Get("Common_0162");
			break;
		case (UserGoods.Type)3:
		case UserGoods.Type.SpotCard:
			break;
		}
	}

	public static string TrimProductName(string text)
	{
		int maxLength = (Global.IsAlphabetLanguage() ? 35 : 18);
		return TrimProductName(text, maxLength);
	}

	public static string TrimProductName(string text, int maxLength)
	{
		text = text.Replace("\n", "");
		if (text.Length > maxLength)
		{
			text = text.Remove(maxLength - 1);
			text += "...";
		}
		return text;
	}

	public static void SetCostInfo(ShopCommonSaleInfo saleInfo, UILabel _labelCostCrystal, UILabel _labelCostRupy, UILabel _labelFree, UILabel _labelTicket)
	{
		SystemText systemText = Data.SystemText;
		if (_labelTicket != null)
		{
			_labelTicket.gameObject.SetActive(value: false);
		}
		if (saleInfo.isFree)
		{
			_labelCostCrystal.gameObject.SetActive(value: false);
			_labelCostRupy.gameObject.SetActive(value: false);
			_labelFree.gameObject.SetActive(value: true);
			_labelFree.text = systemText.Get("Shop_0103");
			return;
		}
		_labelFree.gameObject.SetActive(value: false);
		if (saleInfo.costCrystal.HasValue && saleInfo.costRupy.HasValue)
		{
			_labelCostCrystal.gameObject.SetActive(value: true);
			_labelCostCrystal.gameObject.transform.localPosition = POS_COST_LABLE_RIGHT;
			_labelCostCrystal.text = systemText.Get("Shop_0112", saleInfo.costCrystal.Value.ToString());
			_labelCostRupy.gameObject.SetActive(value: true);
			_labelCostRupy.gameObject.transform.localPosition = POS_COST_LABLE_LEFT;
			_labelCostRupy.text = systemText.Get("Shop_0113", saleInfo.costRupy.Value.ToString());
		}
		else if (saleInfo.costCrystal.HasValue)
		{
			_labelCostCrystal.gameObject.SetActive(value: true);
			_labelCostCrystal.gameObject.transform.localPosition = POS_COST_LABLE_CENTER;
			_labelCostCrystal.text = systemText.Get("Shop_0112", saleInfo.costCrystal.Value.ToString());
			_labelCostRupy.gameObject.SetActive(value: false);
		}
		else if (saleInfo.costRupy.HasValue)
		{
			_labelCostRupy.gameObject.SetActive(value: true);
			_labelCostRupy.gameObject.transform.localPosition = POS_COST_LABLE_CENTER;
			_labelCostRupy.text = systemText.Get("Shop_0113", saleInfo.costRupy.Value.ToString());
			_labelCostCrystal.gameObject.SetActive(value: false);
		}
		else if (saleInfo.costTicket.HasValue)
		{
			_labelCostCrystal.gameObject.SetActive(value: false);
			_labelCostRupy.gameObject.SetActive(value: false);
			_labelTicket.gameObject.SetActive(value: true);
			_labelTicket.gameObject.transform.localPosition = POS_COST_LABLE_CENTER;
			_labelTicket.text = systemText.Get("Shop_0189", saleInfo.costTicket.Value.ToString());
		}
		else
		{
			_labelCostCrystal.gameObject.SetActive(value: false);
			_labelCostRupy.gameObject.SetActive(value: false);
			_labelFree.gameObject.SetActive(value: false);
		}
	}

	public static string GetTicketIconPath(string itemId, bool isFetch)
	{
		return Toolbox.ResourcesManager.GetAssetTypePath("ticket_" + itemId + "_icon", ResourcesManager.AssetLoadPathType.ShopItem, isFetch);
	}

	public static string GetTicketIconRightDownPath(string itemId, bool isFetch)
	{
		return Toolbox.ResourcesManager.GetAssetTypePath("ticket_" + itemId + "_icon_right_down", ResourcesManager.AssetLoadPathType.ShopItem, isFetch);
	}
}
