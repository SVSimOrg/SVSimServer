using UnityEngine;

namespace Wizard;

public class LabelDefine : MonoBehaviour
{
	public static readonly Color32 TEXT_COLOR_NORMAL = new Color32(byte.MaxValue, 253, 235, byte.MaxValue);

	public static readonly Color32 TEXT_COLOR_KEYWORD = new Color32(byte.MaxValue, 205, 69, byte.MaxValue);

	public static readonly Color32 TEXT_COLOR_CREAM = new Color32(200, 200, 176, byte.MaxValue);

	public static readonly Color32 TEXT_COLOR_RED = new Color32(byte.MaxValue, 84, 69, byte.MaxValue);

	public static readonly Color32 TEXT_COLOR_ORANGE = new Color32(byte.MaxValue, 211, 66, byte.MaxValue);

	public static readonly Color32 TEXT_COLOR_BUTTON_ENABLE = new Color32(byte.MaxValue, 253, 235, byte.MaxValue);

	public static readonly Color32 TEXT_COLOR_BUTTON_DISABLE = new Color32(85, 85, 85, byte.MaxValue);

	public static readonly Color32 OUTLINE_COLOR_FOOTER_DEFAULT = new Color32(51, 34, 0, byte.MaxValue);

	public static readonly Color32 OUTLINE_COLOR_FOOTER_DISABLE_DEFAULT = new Color32(19, 20, 12, byte.MaxValue);

	public static readonly Color32 OUTLINE_COLOR_FOOTER_DISABLE_PUSH = new Color32(29, 28, 19, byte.MaxValue);

	public static readonly UILabel.Effect OUTLINE_STYLE_CLASS_NAME = UILabel.Effect.Outline8;

	public static readonly Vector2 OUTLINE_DISTANCE_CLASS_NAME = new Vector2(1.5f, 1.5f);
}
