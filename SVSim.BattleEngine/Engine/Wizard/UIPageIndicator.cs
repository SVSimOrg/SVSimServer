using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class UIPageIndicator : UIBase
{
	[SerializeField]
	private UIGrid _gridParentObj;

	[SerializeField]
	private UIToggle _indicatorOriginal;

	private List<UIToggle> _indicatorList = new List<UIToggle>();

	private int _maxPageNum;

	public void Init(int maxPageNum, int defaultPage = 1)
	{
		_maxPageNum = maxPageNum;
		if (maxPageNum <= 1)
		{
			_gridParentObj.gameObject.SetActive(value: false);
		}
		else
		{
			for (int i = 0; i < maxPageNum; i++)
			{
				_indicatorList.Add(NGUITools.AddChild(_gridParentObj.gameObject, _indicatorOriginal.gameObject).GetComponent<UIToggle>());
			}
			_indicatorOriginal.gameObject.SetActive(value: false);
			_gridParentObj.gameObject.SetActive(value: true);
			_gridParentObj.Reposition();
		}
		UpdateIndicator(defaultPage);
	}

	public void UpdateIndicator(int page)
	{
		if (page > 0 && _maxPageNum >= page && _indicatorList.Count > 0)
		{
			_indicatorList[page - 1].value = true;
		}
	}
}
