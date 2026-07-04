using System;
using System.Collections.Generic;
using Cute;
using LitJson;
using UnityEngine;
// TODO(engine-cleanup-pass2): 74 of 75 methods unrun in baseline
//   Type: Wizard.PlayerStaticData
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class PlayerStaticData : MonoBehaviour
{
	public class UserTex
	{
		public enum Type
		{
			Emblem,
			RankIcon,
			Country
		}

		public Texture[] m_loadedList;

		public List<UITexture>[] m_requiredAttachList;

		public List<string> m_unloadList = new List<string>();

		private readonly Type m_type;

		private readonly int m_sizeDiffNum;

		public int m_loadingCnt;

		public Format Format;

		private List<string> _rankIconPathList = new List<string>();

		public UserTex(Type type)
		{
			m_type = type;
			m_sizeDiffNum = GetSizeDiffNum(m_type);
			m_loadedList = new Texture[m_sizeDiffNum];
			m_requiredAttachList = new List<UITexture>[m_sizeDiffNum];
			for (int i = 0; i < m_requiredAttachList.Length; i++)
			{
				m_requiredAttachList[i] = new List<UITexture>();
			}
		}

		public void LoadTexture()
		{
			ResourcesManager resourcesManager = Toolbox.ResourcesManager;
			List<string> pathList = null;
			if (m_type == Type.RankIcon)
			{
				pathList = GetRankTexturePath();
				for (int i = 0; i < m_sizeDiffNum; i++)
				{
					m_loadedList[i] = null;
				}
			}
			else
			{
				for (int j = 0; j < m_sizeDiffNum; j++)
				{
					m_loadedList[j] = null;
					m_loadingCnt++;
					string texturePath = GetTexturePath(GetTextureName(), j);
					if (pathList == null)
					{
						pathList = new List<string>();
					}
					pathList.Add(texturePath);
				}
			}
			resourcesManager.StartCoroutine_LoadAssetGroupSync(pathList, delegate
			{
				m_loadingCnt = 0;
				UpdateTexture();
				UnloadTexture();
				if (m_type == Type.RankIcon)
				{
					_rankIconPathList.AddRange(pathList);
				}
			});
		}

		private List<string> GetRankTexturePath()
		{
			List<string> list = new List<string>();
			GetRankTexturePathSub(Format.Unlimited, list);
			GetRankTexturePathSub(Format.Rotation, list);
			m_loadingCnt += list.Count;
			return list;
		}

		private static void GetRankTexturePathSub(Format format, List<string> list)
		{
			list.Add(Toolbox.ResourcesManager.GetAssetTypePath(UserRank(format).ToString("00"), ResourcesManager.AssetLoadPathType.RankIcon_S));
			list.Add(Toolbox.ResourcesManager.GetAssetTypePath(UserRank(format).ToString("00"), ResourcesManager.AssetLoadPathType.RankIcon_L));
		}

		public void ReLoadTexture(string oldId, string newId)
		{
			if (m_type != Type.RankIcon && !string.IsNullOrEmpty(oldId) && !m_unloadList.Contains(oldId))
			{
				m_unloadList.Add(oldId);
			}
			if (!string.IsNullOrEmpty(newId))
			{
				m_unloadList.Remove(newId);
				LoadTexture();
				return;
			}
			for (int i = 0; i < m_sizeDiffNum; i++)
			{
				m_loadedList[i] = null;
			}
		}

		private void UpdateTexture()
		{
			ResourcesManager resourcesManager = Toolbox.ResourcesManager;
			for (int i = 0; i < m_sizeDiffNum; i++)
			{
				Texture mainTexture = (m_loadedList[i] = resourcesManager.LoadObject(GetTexturePath(GetTextureName(), i, isfetch: true)) as Texture);
				List<UITexture> list = m_requiredAttachList[i];
				foreach (UITexture item in list)
				{
					item.mainTexture = mainTexture;
				}
				list.Clear();
			}
		}

		private void UnloadTexture()
		{
			List<string> list = new List<string>();
			foreach (string unload in m_unloadList)
			{
				for (int i = 0; i < m_sizeDiffNum; i++)
				{
					list.Add(GetTexturePath(unload, i));
				}
			}
			m_unloadList.Clear();
			list.AddRange(_rankIconPathList);
			_rankIconPathList.Clear();
			Toolbox.ResourcesManager.RemoveAssetGroup(list);
		}

		private static int GetSizeDiffNum(Type type)
		{
			return type switch
			{
				Type.Emblem => 2, 
				Type.RankIcon => 2, 
				Type.Country => 2, 
				_ => 0, 
			};
		}

		private string GetTexturePath(string name, int sizeId, bool isfetch = false)
		{
			int num = 0;
			switch (m_type)
			{
			case Type.Emblem:
				num = 42;
				break;
			case Type.RankIcon:
				num = 49;
				break;
			case Type.Country:
				num = 51;
				break;
			}
			return Toolbox.ResourcesManager.GetAssetTypePath(name, (ResourcesManager.AssetLoadPathType)(num + sizeId), isfetch);
		}

		private string GetTextureName()
		{
			return m_type switch
			{
				Type.Emblem => UserEmblemID.ToString(), 
				Type.RankIcon => UserRank(Format).ToString("00"), // Pre-Phase-5b: headless has no display-high-rank flag
				Type.Country => UserCountryCode, 
				_ => "", 
			};
		}
	}

	public enum EmblemTexSize
	{
		S}

	public enum RankTexSize
	{
		S}

	public enum CountryTexSize
	{
		S}

	public enum AgreementState
	{
		NotAgree,
		Reset
	}

	public enum LootBoxType
	{
		TWOPICK,
		SEALED,
		COLOSSEUM,
		COMPETITION}

	public static AgreementState _tosAgreementState = AgreementState.NotAgree;

	public static AgreementState _privacyPolicyAgreementState = AgreementState.NotAgree;

	private static UserTex UserEmblemTexture = new UserTex(UserTex.Type.Emblem);

	private static UserTex UserCountryTexture = new UserTex(UserTex.Type.Country);

	private static UserTex UserRankTexture = new UserTex(UserTex.Type.RankIcon);

	public static TimeData UserTime = new TimeData();

	public static string UserRegionCode => PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.CURRENT_REGION_CODE);

	public static AgreementState KorAuthorityAgreementState { get; set; } = AgreementState.NotAgree;

	public static int UserCrystalCount
	{
		get
		{
			if (Data.Load.data == null)
			{
				return 0;
			}
			return Data.Load.data._userCrystalCount.total_crystal;
		}
		set
		{
			Data.Load.data._userCrystalCount.total_crystal = value;
		}
	}

	public static int UserRupyCount
	{
		get
		{
			if (Data.Load.data == null)
			{
				return 0;
			}
			return Data.Load.data._userCrystalCount.rupy;
		}
		set
		{
			Data.Load.data._userCrystalCount.rupy = value;
		}
	}

	public static int UserRedEtherCount
	{
		get
		{
			if (Data.Load.data == null)
			{
				return 0;
			}
			return Data.Load.data._userCrystalCount.red_ether;
		}
		set
		{
			Data.Load.data._userCrystalCount.red_ether = value;
		}
	}

	public static int UserSpotCardPointCount
	{
		get
		{
			if (Data.Load.data == null)
			{
				return 0;
			}
			return Data.Load.data._userCrystalCount._spotcardPoint;
		}
		set
		{
			Data.Load.data._userCrystalCount._spotcardPoint = value;
		}
	}

	public static long UserEmblemID
	{
		get
		{
			return Data.Load.data._userInfo.selected_emblem_id;
		}
		set
		{
			UserInfo userInfo = Data.Load.data._userInfo;
			long selected_emblem_id = userInfo.selected_emblem_id;
			if (selected_emblem_id != value)
			{
				userInfo.selected_emblem_id = value;
				UserEmblemTexture.ReLoadTexture(selected_emblem_id.ToString(), value.ToString());
			}
		}
	}

	public static string UserCountryCode
	{
		get
		{
			return Data.Load.data._userInfo.country_code;
		}
		set
		{
			UserInfo userInfo = Data.Load.data._userInfo;
			string country_code = userInfo.country_code;
			if (country_code != value)
			{
				userInfo.country_code = value;
				UserCountryTexture.ReLoadTexture(country_code, value);
			}
		}
	}

	public static void UpdateItemNum(int itemId, int num)
	{
		if (Data.Load.data._userItemDict == null)
		{
			Data.Load.data._userItemDict = new Dictionary<int, int>();
		}
		if (Data.Load.data._userItemDict.ContainsKey(itemId))
		{
			Data.Load.data._userItemDict[itemId] = num;
		}
		else
		{
			Data.Load.data._userItemDict.Add(itemId, num);
		}
	}

	public static void Clear()
	{
		UserEmblemTexture = new UserTex(UserTex.Type.Emblem);
		UserCountryTexture = new UserTex(UserTex.Type.Country);
		UserRankTexture = new UserTex(UserTex.Type.RankIcon);
	}

	public static int UserRank(Format inFormat)
	{
		if (inFormat == Format.PreRotation)
		{
			inFormat = Format.Rotation;
		}
		if (Data.Load.data == null)
		{
			return 0;
		}
		return Data.Load.data._userRank[(int)inFormat].rank;
	}

	public static bool IsMasterRankCurrentFormat()
	{
		return IsMasterRank(Data.CurrentFormat);
	}

	public static bool IsMasterRank(Format inFormat)
	{
		return Data.Load.data._userRank[(int)inFormat].is_master_rank;
	}

	public static void UpdateHaveUserGoodsNumByJsonData(JsonData userGoodsList)
	{
		for (int i = 0; i < userGoodsList.Count; i++)
		{
			JsonData jsonData = userGoodsList[i];
			UserGoods.Type type = (UserGoods.Type)jsonData["reward_type"].ToInt();
			long userGoodsId = jsonData["reward_id"].ToLong();
			int num = jsonData["reward_num"].ToInt();
			UpdateHaveUserGoodsNum(type, userGoodsId, num);
		}
	}

	public static void UpdateHaveUserGoodsNum(UserGoods.Type type, long userGoodsId, int num)
	{
		switch (type)
		{
		case UserGoods.Type.RedEther:
			UserRedEtherCount = num;
			break;
		case UserGoods.Type.Crystal:
			UserCrystalCount = num;
			if (MyPageMenu.Instance != null)
			{
				MyPageMenu.Instance.UpdateCrystalCount();
			}
			break;
		case UserGoods.Type.Item:
			UpdateItemNum((int)userGoodsId, num);
			break;
		case UserGoods.Type.Card:
		{
			int cardId = (int)userGoodsId;
			/* Pre-Phase-5b: headless has no user card data */
			break;
		}
		case UserGoods.Type.SpotCard:
		case UserGoods.Type.SpotCardOnlyLatestCardPack:
		{
			int cardId = (int)userGoodsId;
			/* Pre-Phase-5b: headless has no spot card data */
			break;
		}
		case UserGoods.Type.Rupy:
			UserRupyCount = num;
			if (MyPageMenu.Instance != null)
			{
				MyPageMenu.Instance.UpdateRupyCount();
			}
			break;
		case UserGoods.Type.Skin:
			/* Pre-Phase-5b: headless has no chara master */
			break;
		case UserGoods.Type.Sleeve:
			Data.Master.SleeveMgr.Acquired(userGoodsId);
			break;
		case UserGoods.Type.Emblem:
			Data.Master.EmblemMgr.Acquired(userGoodsId);
			break;
		case UserGoods.Type.Degree:
			Data.Master.DegreeMgr.Acquired((int)userGoodsId);
			break;
		case UserGoods.Type.SpotCardPoint:
			UserSpotCardPointCount = num;
			break;
		case UserGoods.Type.MyPageBG:
			Data.Load.data.AcquiredMyPageBGList.Add(userGoodsId.ToString());
			break;
		case (UserGoods.Type)3:
		case UserGoods.Type.FreeGachaCount:
			break;
		}
	}
}
