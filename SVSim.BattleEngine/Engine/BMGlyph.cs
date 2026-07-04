using System;
using System.Collections.Generic;

[Serializable]
public class BMGlyph
{
	public int index;

	public int x;

	public int y;

	public int width;

	public int height;

	public int offsetX;

	public int offsetY;

	public void Trim(int xMin, int yMin, int xMax, int yMax)
	{
		int num = x + width;
		int num2 = y + height;
		if (x < xMin)
		{
			int num3 = xMin - x;
			x += num3;
			width -= num3;
			offsetX += num3;
		}
		if (y < yMin)
		{
			int num4 = yMin - y;
			y += num4;
			height -= num4;
			offsetY += num4;
		}
		if (num > xMax)
		{
			width -= num - xMax;
		}
		if (num2 > yMax)
		{
			height -= num2 - yMax;
		}
	}
}
