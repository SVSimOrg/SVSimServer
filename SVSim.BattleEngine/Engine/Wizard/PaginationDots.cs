using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class PaginationDots : MonoBehaviour
{
	[SerializeField]
	private UIGrid _dotsGrid;

	[SerializeField]
	private UISprite _dotSpriteOriginal;

	private List<UISprite> _dotSprites = new List<UISprite>();

	public void Init(int pageNum, int dotSpriteSize, int gridCellWidth, int depth)
	{
		UISprite dotSpriteOriginal = _dotSpriteOriginal;
		int width = (_dotSpriteOriginal.height = dotSpriteSize);
		dotSpriteOriginal.width = width;
		_dotSpriteOriginal.depth = depth;
		_dotsGrid.cellWidth = gridCellWidth;
		ChangePageNum(pageNum);
	}

	public void SetActivePageNumber(int pageNo)
	{
		SetActivePageIndex(pageNo - 1);
	}

	public void SetActivePageIndex(int pageIndex)
	{
		for (int i = 0; i < _dotSprites.Count; i++)
		{
			_dotSprites[i].spriteName = ((i == pageIndex) ? "carousel_marker_on" : "carousel_marker_off");
		}
	}

	public void ChangePageNum(int num)
	{
		if (num <= 1)
		{
			foreach (UISprite dotSprite in _dotSprites)
			{
				dotSprite.gameObject.SetActive(value: false);
			}
			return;
		}
		for (int i = _dotSprites.Count; i < num; i++)
		{
			UISprite component = NGUITools.AddChild(_dotsGrid.gameObject, _dotSpriteOriginal.gameObject).GetComponent<UISprite>();
			_dotSprites.Add(component);
		}
		for (int j = 0; j < num; j++)
		{
			_dotSprites[j].gameObject.SetActive(value: true);
		}
		for (int k = num; k < _dotSprites.Count; k++)
		{
			_dotSprites[k].gameObject.SetActive(value: false);
		}
		_dotsGrid.Reposition();
	}
}
