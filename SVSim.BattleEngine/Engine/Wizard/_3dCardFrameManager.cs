using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class _3dCardFrameManager
{
	public enum eFrameKind
	{
		Normal,
		Phantom	}

	private enum eCardKind
	{
		Follower,
		Spell,
		Amulet,
		HeroSkill	}

	private class FrameInfo
	{
		public string FrameMaterialNameFormat { get; set; }

		public eCardKind[] TargetCardKinds { get; set; }

		public int[] TargetRarities { get; set; }
	}

	private static readonly eCardKind[] CARD_KINDS = new eCardKind[4]
	{
		eCardKind.Follower,
		eCardKind.Spell,
		eCardKind.Amulet,
		eCardKind.HeroSkill
	};

	private static readonly string[] CARD_KIND_NAME_TABLE = new string[4] { "F", "S", "SF", "HS" };

	private static readonly string[] RARITY_NAME_TABLE = new string[5]
	{
		string.Empty,
		"Bronze",
		"Silver",
		"Gold",
		"Legend"
	};

	private static readonly FrameInfo[] FRAME_INFO_TABLE = new FrameInfo[2]
	{
		new FrameInfo
		{
			FrameMaterialNameFormat = "CardFrame_{0}_{1}",
			TargetCardKinds = CARD_KINDS,
			TargetRarities = new int[4] { 1, 2, 3, 4 }
		},
		new FrameInfo
		{
			FrameMaterialNameFormat = "CardFrame_{0}_{1}_p",
			TargetCardKinds = new eCardKind[3]
			{
				eCardKind.Follower,
				eCardKind.Spell,
				eCardKind.Amulet
			},
			TargetRarities = new int[2] { 3, 4 }
		}
	};

	private Texture _rainbowTexture;

	private Texture[] _legendFrameMaskList;

	private readonly Material[,,] _frameMaterialList = new Material[2, 4, 5];

	public static List<string> GetLoadAssetList(eFrameKind frameKind)
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		List<string> list = new List<string>();
		FrameInfo frameInfo = FRAME_INFO_TABLE[(int)frameKind];
		eCardKind[] targetCardKinds = frameInfo.TargetCardKinds;
		foreach (eCardKind eCardKind in targetCardKinds)
		{
			if (!BattleManagerBase.IsTutorial || eCardKind != eCardKind.HeroSkill)
			{
				int[] targetRarities = frameInfo.TargetRarities;
				foreach (int rarity in targetRarities)
				{
					list.Add(resourcesManager.GetAssetTypePath(GetFrameMaterialName(frameKind, eCardKind, rarity), ResourcesManager.AssetLoadPathType.CardFrameMaterial));
				}
			}
		}
		return list;
	}

	public void InitFrameMaterials(eFrameKind frameKind)
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		if (_rainbowTexture == null)
		{
			_rainbowTexture = resourcesManager.LoadObject<Texture>(resourcesManager.GetAssetTypePath("tx_foil_rainbow", ResourcesManager.AssetLoadPathType.CardFrameTextureCommon, isfetch: true));
		}
		eCardKind[] cARD_KINDS;
		if (_legendFrameMaskList == null)
		{
			_legendFrameMaskList = new Texture[4];
			cARD_KINDS = CARD_KINDS;
			foreach (eCardKind eCardKind in cARD_KINDS)
			{
				if (!BattleManagerBase.IsTutorial || eCardKind != eCardKind.HeroSkill)
				{
					_legendFrameMaskList[(int)eCardKind] = resourcesManager.LoadObject<Texture>(resourcesManager.GetAssetTypePath(GetLegendFrameMaskName(eCardKind), ResourcesManager.AssetLoadPathType.CardFrameTextureCommon, isfetch: true));
				}
			}
		}
		FrameInfo frameInfo = FRAME_INFO_TABLE[(int)frameKind];
		cARD_KINDS = frameInfo.TargetCardKinds;
		foreach (eCardKind eCardKind2 in cARD_KINDS)
		{
			if (BattleManagerBase.IsTutorial && eCardKind2 == eCardKind.HeroSkill)
			{
				continue;
			}
			int num = (int)eCardKind2;
			int[] targetRarities = frameInfo.TargetRarities;
			foreach (int num2 in targetRarities)
			{
				Material material = (_frameMaterialList[(int)frameKind, num, num2] = resourcesManager.LoadObject<Material>(resourcesManager.GetAssetTypePath(GetFrameMaterialName(frameKind, eCardKind2, num2), ResourcesManager.AssetLoadPathType.CardFrameMaterial, isfetch: true)));
				material.shader = Shader.Find(material.shader.name);
				if (num2 == 4)
				{
					material.SetTexture("_MaskTex", _legendFrameMaskList[num]);
					material.SetTexture("_Front1Tex", _rainbowTexture);
				}
			}
		}
	}

	public void ClearFrameMaterials(eFrameKind frameKind)
	{
		FrameInfo frameInfo = FRAME_INFO_TABLE[(int)frameKind];
		eCardKind[] targetCardKinds = frameInfo.TargetCardKinds;
		for (int i = 0; i < targetCardKinds.Length; i++)
		{
			int num = (int)targetCardKinds[i];
			int[] targetRarities = frameInfo.TargetRarities;
			foreach (int num2 in targetRarities)
			{
				_frameMaterialList[(int)frameKind, num, num2] = null;
			}
		}
	}

	private static string GetLegendFrameMaskName(eCardKind cardKind)
	{
		return $"tx_mask_CardFrame_{CARD_KIND_NAME_TABLE[(int)cardKind]}_Legend";
	}

	private static string GetFrameMaterialName(eFrameKind frameKind, eCardKind cardKind, int rarity)
	{
		return string.Format(FRAME_INFO_TABLE[(int)frameKind].FrameMaterialNameFormat, CARD_KIND_NAME_TABLE[(int)cardKind], RARITY_NAME_TABLE[rarity]);
	}
}
