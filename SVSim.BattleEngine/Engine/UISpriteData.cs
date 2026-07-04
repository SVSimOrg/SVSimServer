using System;

[Serializable]
public class UISpriteData
{
	public string name = "Sprite";

	public int x;

	public int y;

	public int width;

	public int height;

	public int borderLeft;

	public int borderRight;

	public int borderTop;

	public int borderBottom;

	public int paddingLeft;

	public int paddingRight;

	public int paddingTop;

	public int paddingBottom;

	public bool hasBorder => (borderLeft | borderRight | borderTop | borderBottom) != 0;

	public bool hasPadding => (paddingLeft | paddingRight | paddingTop | paddingBottom) != 0;
}
