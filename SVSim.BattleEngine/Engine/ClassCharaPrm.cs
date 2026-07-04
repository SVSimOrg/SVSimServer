using System.Collections.Generic;
using Cute;
using LitJson;
using UnityEngine;
using Wizard;

public class ClassCharaPrm
{
	public enum MotionType
	{
		idle = 1,
		positive,
		negative,
		extra,
		damage,
		think,
		greet,
		shock,
		positive_2,
		negative_2,
		extra_2,
		negative_2_a,
		extra_1_a,
		extra_1_b,
		extra_1_c,
		extra_2_a,
		extra_2_b,
		extra_2_c,
		z_extra_2,
		z_damage,
		z_greet,
		z_idle,
		z_negative,
		z_negative_2,
		z_negative_2_a,
		z_positive,
		z_positive_2,
		z_shock,
		z_think
	}

	public enum FaceType
	{
		skin_01 = 1	}

	public enum EmotionType
	{
		NULL,
		GREET,
		THANK,
		APOLOGY,
		PRAISE,
		SURPRISE,
		CONFUSE,
		PROVOCATION,
		LOSE,
		SURRENDER_LOSE,
		NEGOTIATION_1,
		NEGOTIATION_2,
		NEGOTIATION_3,
		PLAYER_TURN_START_1
	}

	private static readonly Dictionary<CardBasePrm.ClanType, eColorCodeId> OUTLINE_COLOR = new Dictionary<CardBasePrm.ClanType, eColorCodeId>
	{
		{
			CardBasePrm.ClanType.MIN,
			eColorCodeId.CLASS_ELF_OUTLINE
		},
		{
			CardBasePrm.ClanType.ROYAL,
			eColorCodeId.CLASS_ROYAL_OUTLINE
		},
		{
			CardBasePrm.ClanType.WITCH,
			eColorCodeId.CLASS_WITCH_OUTLINE
		},
		{
			CardBasePrm.ClanType.DRAGON,
			eColorCodeId.CLASS_DRAGON_OUTLINE
		},
		{
			CardBasePrm.ClanType.NECRO,
			eColorCodeId.CLASS_NECROMANCER_OUTLINE
		},
		{
			CardBasePrm.ClanType.VAMPIRE,
			eColorCodeId.CLASS_VANPIRE_OUTLINE
		},
		{
			CardBasePrm.ClanType.BISHOP,
			eColorCodeId.CLASS_BISHOP_OUTLINE
		},
		{
			CardBasePrm.ClanType.NEMESIS,
			eColorCodeId.CLASS_NEMESIS_OUTLINE
		},
		{
			CardBasePrm.ClanType.SHADOW,
			eColorCodeId.CLASS_SHADOW_OUTLINE
		}
	};

	private int _currentCharaId;

	private int ClassCharaLv;

	public ClassCharacterMasterData DefaultCharaData => null /* Pre-Phase-5b: headless has no chara master; DefaultCharaData surface preserved for typing */;

	public ClassCharacterMasterData CurrentCharaData
	{
		get
		{
			ClassCharacterMasterData classCharacterMasterData = null; // Pre-Phase-5b: headless has no chara master
			if (classCharacterMasterData == null)
			{
				classCharacterMasterData = DefaultCharaData;
			}
			return classCharacterMasterData;
		}
	}

	public bool IsRandomLeaderSkin { get; set; }

	public List<int> LeaderSkinIdList { get; private set; } = new List<int>();

	public void SetCurrentCharaId(int charaId)
	{
		_currentCharaId = charaId;
	}

	public void SetLeaderRandomSkinIdList(JsonData skinIdList)
	{
		LeaderSkinIdList.Clear();
		for (int i = 0; i < skinIdList.Count; i++)
		{
			LeaderSkinIdList.Add(skinIdList[i].ToInt());
		}
	}

	public int GetClassCharaLv()
	{
		return ClassCharaLv;
	}

	public static Texture GetClassIconTexture(int clan_id)
	{
		return Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath("class_card_" + clan_id.ToString("00"), ResourcesManager.AssetLoadPathType.CardFrameClassIcon, isfetch: true)) as Texture;
	}

	public static bool IsEvolutionEmotionType(EmotionType type)
	{
		if ((uint)(type - 17) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static string GetIconSpriteName(CardBasePrm.ClanType inClassId)
	{
		int num = (int)inClassId;
		return "icon_class_color_" + num.ToString("00");
	}

	public static string GetNameText(CardBasePrm.ClanType inClassId)
	{
		return Data.SystemText.Get("Common_" + ((int)(104 + inClassId)).ToString("0000"));
	}

	public static void SetClassLabelSetting(UILabel inLabel, CardBasePrm.ClanType inClassId)
	{
		inLabel.effectStyle = LabelDefine.OUTLINE_STYLE_CLASS_NAME;
		inLabel.effectDistance = LabelDefine.OUTLINE_DISTANCE_CLASS_NAME;
		inLabel.effectColor = ColorCode.Get(OUTLINE_COLOR[inClassId]);
	}
}
