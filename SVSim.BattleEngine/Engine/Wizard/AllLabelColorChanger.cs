using System.Collections.Generic;
using UnityEngine;
using Wizard.Bingo;

namespace Wizard;

public class AllLabelColorChanger
{
	public struct ColorSet
	{
		public Color32 FontColor { get; private set; }

		public Color32 FrameColor { get; private set; }

		public UILabel.Effect Effect { get; private set; }

		public Vector2 EffectDistance { get; private set; }

		public Color32? ButtonFontColor { get; private set; }

		public bool DontChangeEffect { get; private set; }

		public ColorSet(Color32 fontColor, Color32 frameColor, UILabel.Effect effect, Vector2 effectDistance, Color32? buttonFontColor = null)
		{
			FontColor = fontColor;
			FrameColor = frameColor;
			Effect = effect;
			EffectDistance = effectDistance;
			DontChangeEffect = false;
			ButtonFontColor = buttonFontColor;
		}

		public ColorSet(Color32 fontColor)
		{
			FontColor = fontColor;
			DontChangeEffect = true;
			FrameColor = fontColor;
			Effect = UILabel.Effect.None;
			EffectDistance = Vector2.zero;
			ButtonFontColor = null;
		}
	}

	private static bool deactivateTextColorChange = false;

	private static Dictionary<Color32, ColorSet> COLOR_TABLE = new Dictionary<Color32, ColorSet>
	{
		{
			LabelDefine.TEXT_COLOR_NORMAL,
			new ColorSet(FromHexRGB(14182531), FromHexRGB(16777215), UILabel.Effect.None, new Vector2(2f, 2f))
		},
		{
			LabelDefine.TEXT_COLOR_CREAM,
			new ColorSet(FromHexRGB(14182531), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0), UILabel.Effect.None, Vector2.zero)
		},
		{
			LabelDefine.TEXT_COLOR_BUTTON_DISABLE,
			new ColorSet(FromHexRGB(11838375), FromHexRGB(7171180), UILabel.Effect.None, new Vector2(2f, 2f), FromHexRGB(6106424))
		},
		{
			LabelDefine.TEXT_COLOR_KEYWORD,
			new ColorSet(FromHexRGB(13803266), FromHexRGB(16777215), UILabel.Effect.None, Vector2.zero)
		},
		{
			LabelDefine.TEXT_COLOR_ORANGE,
			new ColorSet(FromHexRGB(16777215), FromHexRGB(16777215), UILabel.Effect.None, Vector2.zero)
		}
	};

	public static Dictionary<Color32, ColorSet> COLOR_TABLE_DETAIL = new Dictionary<Color32, ColorSet>
	{
		{
			LabelDefine.TEXT_COLOR_NORMAL,
			new ColorSet(FromHexRGB(14182531), FromHexRGB(16777215), UILabel.Effect.None, new Vector2(2f, 2f))
		},
		{
			LabelDefine.TEXT_COLOR_CREAM,
			new ColorSet(FromHexRGB(11892614), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0), UILabel.Effect.None, Vector2.zero)
		},
		{
			LabelDefine.TEXT_COLOR_BUTTON_DISABLE,
			new ColorSet(FromHexRGB(6106424), FromHexRGB(7171180), UILabel.Effect.Outline8, new Vector2(2f, 2f))
		}
	};

	public static Dictionary<Color32, ColorSet> COLOR_TABLE_DECK_SELECTION = new Dictionary<Color32, ColorSet>
	{
		{
			LabelDefine.TEXT_COLOR_NORMAL,
			new ColorSet(FromHexRGB(16777215), FromHexRGB(14182531), UILabel.Effect.None, new Vector2(2f, 2f))
		},
		{
			LabelDefine.TEXT_COLOR_ORANGE,
			new ColorSet(FromHexRGB(16777215), FromHexRGB(16777215), UILabel.Effect.None, Vector2.zero)
		}
	};

	public static Dictionary<Color32, ColorSet> COLOR_TABLE_QUEST_BUTTON = new Dictionary<Color32, ColorSet>
	{
		{
			LabelDefine.TEXT_COLOR_NORMAL,
			new ColorSet(FromHexRGB(16777201))
		},
		{
			LabelDefine.TEXT_COLOR_ORANGE,
			new ColorSet(FromHexRGB(16777215), FromHexRGB(16777215), UILabel.Effect.None, Vector2.zero)
		}
	};

	private static Dictionary<Color32, ColorSet> COLOR_TABLE_BINGO_BUTTON = new Dictionary<Color32, ColorSet>
	{
		{
			LabelDefine.TEXT_COLOR_NORMAL,
			new ColorSet(FromHexRGB(4599345), FromHexRGB(16777215), UILabel.Effect.None, new Vector2(2f, 2f))
		},
		{
			LabelDefine.TEXT_COLOR_BUTTON_DISABLE,
			new ColorSet(FromHexRGB(4599345), FromHexRGB(16777215), UILabel.Effect.None, new Vector2(2f, 2f))
		}
	};

	private static Color32 FromHexRGB(int hex)
	{
		byte r = (byte)((hex & 0xFF0000) >> 16);
		byte g = (byte)((hex & 0xFF00) >> 8);
		byte b = (byte)(hex & 0xFF);
		return new Color32(r, g, b, byte.MaxValue);
	}

	public static void ChangeAllLabel(GameObject root, Dictionary<Color32, ColorSet> colorDict = null)
	{
		if (deactivateTextColorChange)
		{
			return;
		}
		if (colorDict == null)
		{
			colorDict = COLOR_TABLE;
		}
		if (root.GetComponentInChildren<DeckSelectUI>(includeInactive: true) != null)
		{
			colorDict = COLOR_TABLE_DECK_SELECTION;
		}
		if (root.GetComponent<QuestSelectionButtonBase>() != null)
		{
			colorDict = COLOR_TABLE_QUEST_BUTTON;
		}
		if (root.GetComponent<CardDetailUI>() != null)
		{
			colorDict = COLOR_TABLE_DETAIL;
		}
		// BingoPage removed (DEAD-COLD engine cleanup Task 12)
		if (colorDict == COLOR_TABLE_DETAIL && (root.name == "CardTexture" || root.name == "CardText"))
		{
			return;
		}
		UILabel[] components = root.GetComponents<UILabel>();
		foreach (UILabel uILabel in components)
		{
			bool flag = false;
			bool flag2 = false;
			ColorOverwrite component = uILabel.GetComponent<ColorOverwrite>();
			if (component != null)
			{
				if (component.ColorChange == ColorOverwrite.Change.No)
				{
					continue;
				}
				if (component.ColorChange == ColorOverwrite.Change.UseDeckColorSet)
				{
					colorDict = COLOR_TABLE_DECK_SELECTION;
				}
				else if (component.ColorChange == ColorOverwrite.Change.UseBingoButtonSet)
				{
					colorDict = COLOR_TABLE_BINGO_BUTTON;
				}
				flag2 = component.DontChangeEffectDistance;
				flag = component.DontChangeEffectStyle;
			}
			if (uILabel.GetComponent<ParameterOverwriterBase>() != null || !colorDict.TryGetValue(uILabel.color, out var value))
			{
				continue;
			}
			if ((bool)uILabel.gameObject.GetComponentInParent<UIButton>(includeInactive: true) && (uILabel.color == LabelDefine.TEXT_COLOR_NORMAL || uILabel.color == LabelDefine.TEXT_COLOR_BUTTON_DISABLE) && value.ButtonFontColor.HasValue)
			{
				uILabel.color = value.ButtonFontColor.Value;
			}
			else
			{
				uILabel.color = value.FontColor;
			}
			if (!value.DontChangeEffect)
			{
				uILabel.effectColor = value.FrameColor;
				if (!flag)
				{
					uILabel.effectStyle = value.Effect;
				}
				if (!flag2)
				{
					uILabel.effectDistance = value.EffectDistance;
				}
			}
		}
		for (int j = 0; j < root.transform.childCount; j++)
		{
			ChangeAllLabel(root.transform.GetChild(j).gameObject, colorDict);
		}
	}
}
