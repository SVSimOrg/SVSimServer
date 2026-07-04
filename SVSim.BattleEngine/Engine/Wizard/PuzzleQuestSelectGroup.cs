using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class PuzzleQuestSelectGroup : MonoBehaviour
{
	[SerializeField]
	private UILabel _difficultyLabel;

	[SerializeField]
	private UILabel _clearCountLabel;

	[SerializeField]
	private UILabel _clearLabel;

	[SerializeField]
	private UIGrid _grid;

	[SerializeField]
	private GameObject _itemOrigin;

	public void Setup(string difficultyName, List<PuzzleQuestSelectDialog.DisplayData> displayDataList, Action<PuzzleQuestSelectDialog.DisplayData> onClick)
	{
		_difficultyLabel.text = difficultyName;
		_clearCountLabel.gameObject.SetActive(value: false);
		_clearLabel.gameObject.SetActive(value: false);
		_itemOrigin.SetActive(value: true);
		foreach (PuzzleQuestSelectDialog.DisplayData displayData in displayDataList)
		{
			UnityEngine.Object.Instantiate(_itemOrigin, _grid.transform).GetComponent<PuzzleQuestSelectItem>().Setup(displayData, onClick);
		}
		_itemOrigin.SetActive(value: false);
		_grid.Reposition();
	}

	public float GetHeight()
	{
		int childCount = _grid.transform.childCount;
		if (childCount <= 0)
		{
			return 56f;
		}
		int num = (childCount - 1) / _grid.maxPerLine + 1;
		return 56f + 137f * (float)num;
	}
}
