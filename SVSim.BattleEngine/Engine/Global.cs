using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Cute;
using UnityEngine;
using Wizard;

public static class Global
{
	public enum CHAR_TYPE
	{
		ENEMY,
		NONE	}

	public enum CardRarity
	{
	}

	public enum LANG_TYPE
	{
		Jpn,
		Eng,
		Kor,
		Chs,
		Cht,
		Fre,
		Ita,
		Ger,
		Spa	}

	public struct LanguageProps
	{
		public string LangType;

		public string Name;

		public string Font;

		public string DisplayName;

		public LanguageProps(string langType, string name, string font, string display)
		{
			LangType = langType;
			Name = name;
			Font = font;
			DisplayName = display;
		}
	}

	public const int NONE = -1;

	public static Color CARD_SELECT_COLOR;

	public static Color CARD_PASSIVE_COLOR;

	public static Color CARD_DEFAULT_COLOR;

	public static Color CARD_INACTIVE_COLOR;

	public static Color CARD_BLESS_EFFECT_COLOR;

	public static Color CARD_POWERDOWN_EFFECT_COLOR;

	public static Color CARD_LABEL_FRAME_TEXT_COLOR;

	public static Color CARD_LABEL_FRAME_TEXT_RED_COLOR;

	public static Color CARD_LABEL_FRAME_COST_COLOR;

	public static Color CARD_HBP_LABEL_COST_COLOR;

	public static Color CARD_LABEL_FRAME_ATTACK_COLOR;

	public static Color CARD_LABEL_FRAME_HEALTH_COLOR;

	public static Color CARD_LABEL_FRAME_LESS_THAN_MAX_COLOR;

	public static Color CARD_LABEL_FRAME_LESS_THAN_BASE_COLOR;

	public static readonly Color FRAME_COLOR_CAN_ACT;

	public static readonly Color FRAME_COLOR_CAN_ACT_RESTRICTED;

	public static readonly Color FRAME_COLOR_SKILL_YELLOW;

	public static readonly Color FRAME_COLOR_SKILL_PURPLE;

	public static readonly Color FRAME_COLOR_SKILL_LIGHT_BLUE;

	public static readonly Color FRAME_COLOR_SELECTABLE;

	public static readonly Color FRAME_COLOR_FUSION_METAMORPHOSE;

	public static readonly Color PROTECTION_COLOR_DAMAGE_CUT;

	public static readonly Color PROTECTION_COLOR_INDESTRUCTIBLE;

	public static readonly Color PROTECTION_COLOR_MULTI_INVALID;

	public static readonly Color PROTECTION_COLOR_DAMAGE_REFLECTION;

	public static readonly Color EVOLVE_TRAIL_COLOR_NORMAL;

	public static readonly Color EVOLVE_TRAIL_COLOR_SKILL;

	public static readonly Color32 EFFECT_COLOR_ELF;

	public static readonly Color32 EFFECT_COLOR_ROYAL;

	public static readonly Color32 EFFECT_COLOR_WITCH_1;

	public static readonly Color32 EFFECT_COLOR_WITCH_2;

	public static readonly Color32 EFFECT_COLOR_DRAGON;

	public static readonly Color32 EFFECT_COLOR_NECROMANCER;

	public static readonly Color32 EFFECT_COLOR_VANPIRE;

	public static readonly Color32 EFFECT_COLOR_BISHOP;

	public static readonly Color32 EFFECT_COLOR_NEMESIS;

	public static readonly Rect CARD_2D_UV_RECT;

	public static readonly Vector3 CARD_BASE_POS;

	public static readonly Vector3 CARD_BASE_ROT;

	public static readonly Vector3 CARD_BASE_SCALE;

	public static readonly Vector3 CARD_BASE_STAY_SCALE;

	public static readonly Vector3 CARD_BASE_SELECT_SCALE;

	public static readonly Vector3 CARD_LIST_SCALE;

	public static readonly Vector3 CARD_BATTLE_SCALE;

	public static readonly Vector3 CARD_BATTLE_ROTATION;

	public static readonly Vector3 CLASS_BATTLE_SCALE;

	public static readonly Vector3 CLASS_BATTLE_POSITION_PLAYER;

	public static readonly Vector3 CLASS_BATTLE_POSITION_ENEMY;

	public static readonly Vector3 EP_PANEL_POSITION_PLAYER;

	public static readonly Vector3 PLAYER_CHOICE_BRAVE_BUTTON_POSITION;

	public static readonly Vector3 ENEMY_CHOICE_BRAVE_BUTTON_POSITION;

	public static readonly Vector3 PLAYER_CHOICE_BRAVE_BUTTON_POSITION_ZOOM;

	public static Vector2 WEBVIEW_NORMAL_SIZE;

	public static Vector3 POSITION_COST_ICON;

	public static Vector3 POSITION_ATK_ICON;

	public static Vector3 POSITION_LIFE_ICON;

	public static Vector3 POSITION_NAME_TEXT;

	public static Vector3 POSITION_SKILL_TEXT;

	public static Vector3 SCALE_CARD_ICON;

	public static Vector3 SCALE_NAME_TEXT;

	public static Vector3 SCALE_SKILL_TEXT;

	public static bool IS_LOAD_ALLDONE;

	public static int NormalFieldOfView;

	public static int WideFieldOfView;

	public static string GAME_FONT_NAME;

	public static string[] fontFileNames;

	public static List<int> PreLoadSkinId;

	public static List<int> SeSysSummonLandingDuplicateCheckId;

	public static string jpn_font;

	public static LanguageProps[] LanguagePropList;

	public static UnityEngine.Font GAME_FONT;

	private static Vector2 CARD_NAME_POS_SHORT;

	private static Vector2 CARD_NAME_POS_NORMAL;

	private static Vector3 CARD_NAME_POSTION_ADD;

	private static Vector2 CARD_NAME_POS_SHORT_2D;

	private static Vector2 CARD_NAME_POS_NORMAL_2D;

	private static float CARD_NAME_Z_ALPHABET_LANGUAGE;

	private static int CARD_NAME_SIZE_ALPHABET_LANGUAGE;

	private static readonly LANG_TYPE[] WordBreakLanguages;

	private static readonly LANG_TYPE[] AlphabetLanguages;

	private static readonly string[] WordBreakLanguageNames;

	private static readonly string[] AlphabetLanguageNames;

	static Global()
	{
		CARD_SELECT_COLOR = Color.cyan;
		CARD_PASSIVE_COLOR = Color.red;
		CARD_DEFAULT_COLOR = Color.white;
		CARD_INACTIVE_COLOR = new Color(0.2f, 0.2f, 0.2f);
		CARD_BLESS_EFFECT_COLOR = new Color(0.7f, 1f, 0f, 1f);
		CARD_POWERDOWN_EFFECT_COLOR = new Color(1f, 0.8f, 0.4f);
		CARD_LABEL_FRAME_TEXT_COLOR = new Color(1f, 0.992f, 0.922f);
		CARD_LABEL_FRAME_TEXT_RED_COLOR = new Color(1f, 0.7f, 0.7f);
		CARD_LABEL_FRAME_COST_COLOR = new Color(0f, 0.2f, 0f);
		CARD_HBP_LABEL_COST_COLOR = new Color(0.27f, 0.207f, 0.176f, 1f);
		CARD_LABEL_FRAME_ATTACK_COLOR = new Color(0.2f, 0.2f, 0.4f);
		CARD_LABEL_FRAME_HEALTH_COLOR = new Color(0.4f, 0.2f, 0.2f);
		CARD_LABEL_FRAME_LESS_THAN_MAX_COLOR = new Color(0.9f, 0.75f, 0.7f);
		CARD_LABEL_FRAME_LESS_THAN_BASE_COLOR = new Color(32f / 51f, 26f / 51f, 14f / 51f);
		FRAME_COLOR_CAN_ACT = new Color(0f, 1f, 32f / 51f);
		FRAME_COLOR_CAN_ACT_RESTRICTED = new Color(1f, 1f, 0f);
		FRAME_COLOR_SKILL_YELLOW = new Color(64f / 85f, 1f, 0f);
		FRAME_COLOR_SKILL_PURPLE = new Color(0.5019608f, 0.4392157f, 1f);
		FRAME_COLOR_SKILL_LIGHT_BLUE = new Color(0f, 0.7921569f, 1f, 1f);
		FRAME_COLOR_SELECTABLE = new Color(1f, 32f / 51f, 0f);
		FRAME_COLOR_FUSION_METAMORPHOSE = new Color(64f / 85f, 1f, 0f);
		PROTECTION_COLOR_DAMAGE_CUT = new Color(0f, 0.2509804f, 1f);
		PROTECTION_COLOR_INDESTRUCTIBLE = new Color(1f, 0.5019608f, 0f);
		PROTECTION_COLOR_MULTI_INVALID = new Color(0f, 1f, 0.6901961f);
		PROTECTION_COLOR_DAMAGE_REFLECTION = new Color(1f, 0.1254902f, 0.1254902f);
		EVOLVE_TRAIL_COLOR_NORMAL = new Color(1f, 0.8f, 0.2f, 1f);
		EVOLVE_TRAIL_COLOR_SKILL = new Color(0f, 0.2f, 0.4f, 1f);
		EFFECT_COLOR_ELF = new Color32(64, byte.MaxValue, 128, byte.MaxValue);
		EFFECT_COLOR_ROYAL = new Color32(byte.MaxValue, 224, 64, byte.MaxValue);
		EFFECT_COLOR_WITCH_1 = new Color32(224, 64, byte.MaxValue, byte.MaxValue);
		EFFECT_COLOR_WITCH_2 = new Color32(67, 82, 155, byte.MaxValue);
		EFFECT_COLOR_DRAGON = new Color32(byte.MaxValue, 128, 32, byte.MaxValue);
		EFFECT_COLOR_NECROMANCER = new Color32(128, 64, byte.MaxValue, byte.MaxValue);
		EFFECT_COLOR_VANPIRE = new Color32(byte.MaxValue, 64, 64, byte.MaxValue);
		EFFECT_COLOR_BISHOP = new Color32(byte.MaxValue, 240, 160, byte.MaxValue);
		EFFECT_COLOR_NEMESIS = new Color32(64, 128, byte.MaxValue, byte.MaxValue);
		CARD_2D_UV_RECT = new Rect(0f, 0f, 1f, 1.1f);
		CARD_BASE_POS = new Vector3(0f, 0f, 0f);
		CARD_BASE_ROT = new Vector3(0f, -180f, 0f);
		CARD_BASE_SCALE = new Vector3(1f, 1f, 1f);
		CARD_BASE_STAY_SCALE = new Vector3(18f, 18f, 18f);
		CARD_BASE_SELECT_SCALE = new Vector3(2.42f, 1f, 3.2f);
		CARD_LIST_SCALE = new Vector3(170f, 226f, 1f);
		CARD_BATTLE_SCALE = Vector3.one;
		CARD_BATTLE_ROTATION = new Vector3(-10f, 0f, 0f);
		CLASS_BATTLE_SCALE = Vector3.one;
		CLASS_BATTLE_POSITION_PLAYER = new Vector3(0f, 0f, 0f);
		CLASS_BATTLE_POSITION_ENEMY = new Vector3(0f, 0f, 0f);
		EP_PANEL_POSITION_PLAYER = new Vector3(-229f, -11.29f, 0f);
		PLAYER_CHOICE_BRAVE_BUTTON_POSITION = new Vector3(-440f, 200f, 15f);
		ENEMY_CHOICE_BRAVE_BUTTON_POSITION = new Vector3(390f, -126f, 10f);
		PLAYER_CHOICE_BRAVE_BUTTON_POSITION_ZOOM = new Vector3(PLAYER_CHOICE_BRAVE_BUTTON_POSITION.x, PLAYER_CHOICE_BRAVE_BUTTON_POSITION.y + 25f, PLAYER_CHOICE_BRAVE_BUTTON_POSITION.z);
		WEBVIEW_NORMAL_SIZE = new Vector2(1100f, 440f);
		POSITION_COST_ICON = new Vector3(-1.68f, 2.1f, -0.2f);
		POSITION_ATK_ICON = new Vector3(-1.6f, -2.2f, -0.2f);
		POSITION_LIFE_ICON = new Vector3(1.6f, -2.2f, -0.2f);
		POSITION_NAME_TEXT = new Vector3(0f, 2f, -0.2f);
		POSITION_SKILL_TEXT = new Vector3(0f, -28f, -0.03f);
		SCALE_CARD_ICON = new Vector3(0.4f, 0.4f, 1f);
		SCALE_NAME_TEXT = new Vector3(0.0024f, 0.0024f, 1f);
		SCALE_SKILL_TEXT = new Vector3(1.25f, 1.25f, 0f);
		IS_LOAD_ALLDONE = false;
		NormalFieldOfView = 60;
		WideFieldOfView = 70;
		GAME_FONT_NAME = "A-OTF-KaiminTuStd-Bold";
		fontFileNames = new string[5] { "A-OTF-KaiminTuStd-Bold.otf", "TT0818M.TTF", "2002L.otf", "DFPT_W7_0.ttf", "DFGBWB7-900.ttf" };
		PreLoadSkinId = new List<int> { 3918, 3904 };
		SeSysSummonLandingDuplicateCheckId = new List<int> { 116024010, 130324010, 130514010 };
		jpn_font = "A-OTF-KaiminTuStd-Bold";
		LanguagePropList = new LanguageProps[8]
		{
			new LanguageProps(LANG_TYPE.Eng.ToString(), "English", "TT0818M", "English"),
			new LanguageProps(LANG_TYPE.Kor.ToString(), "Korean", "2002L", "한국어"),
			new LanguageProps(LANG_TYPE.Cht.ToString(), "ChineseTraditional", "DFPT_W7_0", "繁體中文"),
			new LanguageProps(LANG_TYPE.Fre.ToString(), "French", "TT0818M", "Français"),
			new LanguageProps(LANG_TYPE.Ita.ToString(), "Italian", "TT0818M", "Italiano"),
			new LanguageProps(LANG_TYPE.Ger.ToString(), "German", "TT0818M", "Deutsch"),
			new LanguageProps(LANG_TYPE.Spa.ToString(), "Spanish", "TT0818M", "Español"),
			new LanguageProps(LANG_TYPE.Chs.ToString(), "ChineseSimplified", "DFGBWB7-900", "简体中文")
		};
		GAME_FONT = null;
		CARD_NAME_POS_SHORT = new Vector2(0f, 0f);
		CARD_NAME_POS_NORMAL = new Vector2(75f, 0f);
		CARD_NAME_POSTION_ADD = new Vector3(0f, 10f, 0f);
		CARD_NAME_POS_SHORT_2D = new Vector2(0f, 109f);
		CARD_NAME_POS_NORMAL_2D = new Vector2(10f, 109f);
		CARD_NAME_Z_ALPHABET_LANGUAGE = -0.1f;
		CARD_NAME_SIZE_ALPHABET_LANGUAGE = 32;
		WordBreakLanguages = new LANG_TYPE[6]
		{
			LANG_TYPE.Eng,
			LANG_TYPE.Fre,
			LANG_TYPE.Ita,
			LANG_TYPE.Ger,
			LANG_TYPE.Spa,
			LANG_TYPE.Kor
		};
		AlphabetLanguages = new LANG_TYPE[5]
		{
			LANG_TYPE.Eng,
			LANG_TYPE.Fre,
			LANG_TYPE.Ita,
			LANG_TYPE.Ger,
			LANG_TYPE.Spa
		};
		WordBreakLanguageNames = null;
		AlphabetLanguageNames = null;
		WordBreakLanguageNames = new string[WordBreakLanguages.Length];
		for (int i = 0; i < WordBreakLanguageNames.Length; i++)
		{
			WordBreakLanguageNames[i] = WordBreakLanguages[i].ToString();
		}
		AlphabetLanguageNames = new string[AlphabetLanguages.Length];
		for (int j = 0; j < AlphabetLanguageNames.Length; j++)
		{
			AlphabetLanguageNames[j] = AlphabetLanguages[j].ToString();
		}
	}

	public static string GetConvertWrapText(UILabel label, string orgText)
	{
		try
		{
			if (label.bitmapFont == null && label.trueTypeFont != GAME_FONT)
			{
				label.trueTypeFont = GAME_FONT;
			}
			SystemText systemText = Data.SystemText;
			string text = systemText.Get("System_LineHeadWrap", enableDebugReturn: false);
			string text2 = systemText.Get("System_LineEndWrap", enableDebugReturn: false);
			label.text = orgText;
			string text3 = orgText;
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = text3.Replace("\n", "\n\u200b");
			}
			string final = "";
			if (!label.Wrap(text3, out final))
			{
				final = label.text;
			}
			if (final.Equals(text3))
			{
				return final;
			}
			if (text == string.Empty && text2 == string.Empty)
			{
				return final;
			}
			string text4 = Regex.Escape(text);
			string text5 = Regex.Escape(text2);
			string pattern = "[" + text5 + "]+(\\[[a-z0-9\\/\\-]*\\])*$";
			string pattern2 = "^(\\[[a-z0-9\\/\\-]*\\])*[" + text4 + "]";
			string pattern3 = "[" + text5 + "]*.[" + text4 + "]*(\\[[a-z0-9\\/\\-]*\\])*$";
			List<string> list = new List<string>(final.Split('\n'));
			string text6 = string.Empty;
			for (int i = 0; i < list.Count && i <= final.Length; i++)
			{
				string text7 = list[i];
				string text8 = ((i + 1 < list.Count) ? list[i + 1] : string.Empty);
				Match match = Regex.Match(text7, pattern);
				if (match.Success && match.ToString() != text7)
				{
					string value = text7.Substring(match.Index, match.Length);
					if (text8 == string.Empty)
					{
						list.Add(text8 = "");
					}
					text8 = text8.Insert(0, value);
					text7 = text7.Substring(0, match.Index);
				}
				if (text6 != string.Empty)
				{
					if (Regex.Match(text7, pattern2).Success)
					{
						Match match2 = Regex.Match(text6, pattern3);
						if (match2.Success && match2.Index > 0)
						{
							text7 = text7.Insert(0, text6.Substring(match2.Index, match2.Length));
							text6 = text6.Substring(0, match2.Index);
						}
					}
					if (text7.Length > 1)
					{
						string text9 = text7.Replace('\u200b', '\n');
						if (!label.Wrap(text9, out var final2))
						{
							final2 = text9;
						}
						if (final2.Contains('\n'))
						{
							int num = final2.LastIndexOf('\n');
							if (num > 0)
							{
								if (text8 == string.Empty)
								{
									list.Add(text8 = "");
								}
								text8 = text8.Insert(0, final2.Substring(num + 1));
								text7 = final2.Substring(0, num);
							}
						}
					}
				}
				if (text6 != string.Empty)
				{
					list[i - 1] = text6;
				}
				if (text8 != string.Empty)
				{
					list[i + 1] = text8;
				}
				text6 = (list[i] = text7);
			}
			string text11 = "";
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				text11 += list[j];
				if (j < count - 1)
				{
					text11 += "\n";
				}
			}
			return text11;
		}
		catch (Exception ex)
		{
			label.text = orgText;
			throw new Exception(ex.Message);
		}
	}

	public static int GetTextLineCount(string text)
	{
		int num = 1;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\n')
			{
				num++;
			}
		}
		return num;
	}

	public static string ConvertToWithoutBBCode(string text)
	{
		return Regex.Replace(text, "(\\[[a-z0-9\\/\\-]*(rub\\<[^\\>]*\\>)*\\])", "");
	}

	public static void SetRepositionNameLabel(UILabel label, string orgText, bool is2D)
	{
		if (IsAlphabetLanguage())
		{
			if (orgText.Length <= 11)
			{
				label.transform.localPosition = (is2D ? CARD_NAME_POS_SHORT_2D : CARD_NAME_POS_SHORT);
			}
			else
			{
				label.transform.localPosition = (is2D ? CARD_NAME_POS_NORMAL_2D : CARD_NAME_POS_NORMAL);
			}
		}
		else if (ConvertToWithoutBBCode(orgText).Length <= 5)
		{
			label.transform.localPosition = (is2D ? CARD_NAME_POS_SHORT_2D : CARD_NAME_POS_SHORT);
		}
		else
		{
			label.transform.localPosition = (is2D ? CARD_NAME_POS_NORMAL_2D : CARD_NAME_POS_NORMAL);
		}
		if (!is2D && IsAlphabetLanguage())
		{
			Vector3 localPosition = label.transform.parent.localPosition;
			float num = CARD_NAME_Z_ALPHABET_LANGUAGE;
			if (label.transform.parent.localPosition.z > 0f)
			{
				num *= -1f;
			}
			localPosition.z = num;
			label.transform.localPosition += CARD_NAME_POSTION_ADD;
			label.transform.parent.localPosition = localPosition;
			label.fontSize = CARD_NAME_SIZE_ALPHABET_LANGUAGE;
		}
	}

	public static bool IsAlphabetLanguage()
	{
		string textLanguage = CustomPreference.GetTextLanguage();
		return AlphabetLanguageNames.Contains(textLanguage);
	}
}
