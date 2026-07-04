using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;

public class AreaSelInfo : MonoBehaviour
{
	private enum eTableCategory
	{
	}

	private static readonly string[] CLEARPRESENT_NAME = new string[10] { "", "Common_0205", "", "Common_0201", "", "", "", "", "", "Common_0115" };

	public static string GetPresentItemName(int itemID, long userGoodsId)
	{
		switch ((UserGoods.Type)itemID)
		{
		case UserGoods.Type.RedEther:
		case UserGoods.Type.Rupy:
			return Data.SystemText.Get(CLEARPRESENT_NAME[itemID]);
		case UserGoods.Type.Item:
		{
			Item item = Data.Master.ItemList.Find((Item data) => data.UserGoodsId == userGoodsId);
			if (item == null)
			{
				return string.Empty;
			}
			return item.name;
		}
		case UserGoods.Type.Sleeve:
		{
			Sleeve sleeve = Data.Master.SleeveMgr.Get(userGoodsId);
			if (sleeve == null)
			{
				return string.Empty;
			}
			return sleeve.sleeve_name;
		}
		case UserGoods.Type.Emblem:
		{
			Emblem emblem = Data.Master.EmblemMgr.Get(userGoodsId);
			if (emblem == null)
			{
				return string.Empty;
			}
			return emblem._name;
		}
		case UserGoods.Type.Degree:
		{
			Degree degree = Data.Master.DegreeMgr.Get((int)userGoodsId);
			if (degree == null)
			{
				return string.Empty;
			}
			return degree._name;
		}
		case UserGoods.Type.Skin:
		{
			ClassCharacterMasterData charaPrmByCharaId = null; // Pre-Phase-5b: no chara master headless
			if (charaPrmByCharaId == null)
			{
				return string.Empty;
			}
			return charaPrmByCharaId.chara_name;
		}
		case UserGoods.Type.SpotCardPoint:
			return Data.SystemText.Get("Common_0161");
		case UserGoods.Type.MyPageBG:
			return Data.Master.MyPageCustomBGMaster[userGoodsId.ToString()].Name;
		default:
			return string.Empty;
		}
	}
}
