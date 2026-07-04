using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using LitJson;
using UnityEngine;

namespace Wizard;

public class PuzzleQuestSelectDialog : MonoBehaviour
{
	public class DisplayData
	{
		public PuzzleQuestData Data;

		public int Difficulty;

		public bool IsCleared;

		public bool IsAdditional;

		public bool IsUnlocked;

		public bool IsDisplayNew;

		public string UnlockConditionText;
	}

	[SerializeField]
	private GameObject _groupOrigin;

	[SerializeField]
	private Transform _groupParent;

	private Action<PuzzleQuestData, int> _onDecide;

	public static List<string> CollectResourcePath(List<DisplayData> displayDataList)
	{
		List<string> list = new List<string>();
		foreach (int item in displayDataList.Select((DisplayData data) => data.Data.PlayerSkin).Distinct())
		{
			list.Add(Toolbox.ResourcesManager.GetAssetTypePath(item.ToString(), ResourcesManager.AssetLoadPathType.DeckListTexture));
		}
		return list;
	}

	public static List<DisplayData> CreateDisplayData(bool isDisplayNew, JsonData jsonData)
	{
		List<DisplayData> list = new List<DisplayData>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			JsonData jsonData2 = jsonData[i];
			int id = jsonData2["puzzle_id"].ToInt();
			PuzzleQuestData puzzleQuestData = Data.Master.PuzzleQuestDataList.Find((PuzzleQuestData item) => item.Id == id);
			if (puzzleQuestData == null)
			{
				Debug.LogError($"unknown puzzle quest id : {id}");
				continue;
			}
			bool valueOrDefault = jsonData2.GetValueOrDefault("is_additional", defaultValue: false);
			bool valueOrDefault2 = jsonData2.GetValueOrDefault("is_playable", defaultValue: true);
			string valueOrDefault3 = jsonData2.GetValueOrDefault("release_condition_text_id", string.Empty);
			list.Add(new DisplayData
			{
				Data = puzzleQuestData,
				Difficulty = jsonData2["puzzle_difficulty"].ToInt(),
				IsCleared = jsonData2["is_cleared"].ToBoolean(),
				IsAdditional = valueOrDefault,
				IsUnlocked = valueOrDefault2,
				IsDisplayNew = (isDisplayNew && valueOrDefault && valueOrDefault2),
				UnlockConditionText = GetUnlockConditionText(valueOrDefault3)
			});
		}
		return list;
	}

	public static DialogBase CreateDialog(PuzzleQuestInfo info, Action<PuzzleQuestData, int> onDecide)
	{
		DialogBase dialog = UIManager.GetInstance().CreateDialogClose();
		dialog.SetTitleLabel(Data.SystemText.Get("Puzzle_SelectDialog_0001"));
		dialog.SetSize(DialogBase.Size.XL);
		dialog.SetButtonLayout(DialogBase.ButtonLayout.NONE);
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)Resources.Load("UI/layoutParts/Dialog/PuzzleQuestSelectDialog"));
		gameObject.GetComponent<PuzzleQuestSelectDialog>().Setup(info, delegate(PuzzleQuestData data, int difficulty)
		{
			onDecide(data, difficulty);
			dialog.Close();
		});
		dialog.SetObj(gameObject);
		return dialog;
	}

	private void Setup(PuzzleQuestInfo info, Action<PuzzleQuestData, int> onDecide)
	{
		_onDecide = onDecide;
		List<List<DisplayData>> list = CreateOrderedDisplayDataList(info.DisplayDatas);
		_groupOrigin.gameObject.SetActive(value: true);
		float num = 0f;
		foreach (List<DisplayData> item in list)
		{
			PuzzleQuestSelectGroup component = UnityEngine.Object.Instantiate(_groupOrigin, _groupParent).GetComponent<PuzzleQuestSelectGroup>();
			component.Setup(info.GetDifficultyName(item[0].Difficulty), item, OnClickItem);
			UIUtil.SetLocalPositionY(component.transform, num);
			num -= component.GetHeight();
		}
		_groupParent.GetComponent<UIScrollView>().ResetPosition();
		_groupOrigin.gameObject.SetActive(value: false);
	}

	private List<List<DisplayData>> CreateOrderedDisplayDataList(List<DisplayData> displayDataList)
	{
		List<List<DisplayData>> list = new List<List<DisplayData>>(4);
		List<List<DisplayData>> list2 = new List<List<DisplayData>>(4);
		int i = 0;
		while (i < 4)
		{
			List<DisplayData> list3 = displayDataList.Where((DisplayData data) => data.Difficulty == i).ToList();
			if (list3.Count != 0)
			{
				if (list3.All((DisplayData data) => data.IsCleared))
				{
					list2.Add(list3);
				}
				else
				{
					list.Add(list3);
				}
			}
			int num = i + 1;
			i = num;
		}
		list.AddRange(list2);
		return list;
	}

	private void OnClickItem(DisplayData displayData)
	{

		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Common_0021"));
		dialogBase.SetSize(DialogBase.Size.S);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(Data.SystemText.Get("Common_0004"), Data.SystemText.Get("Common_0005"));
		dialogBase.SetPanelDepth(20);
		dialogBase.ClickSe_Btn1 = 0;
		dialogBase.onPushButton1 = delegate
		{
			_onDecide(displayData.Data, displayData.Difficulty);
		};
		string text = string.Format(arg0: Data.SystemText.Get(displayData.Data.QuestNameTextId), format: Data.SystemText.Get("Puzzle_SelectDialog_0002"));
		if (displayData.IsCleared)
		{
			text = text + "\n" + Data.SystemText.Get("Puzzle_SelectDialog_0003");
		}
		dialogBase.SetText(text);
	}

	private static string GetUnlockConditionText(string textId)
	{
		if (!(textId != string.Empty))
		{
			return null;
		}
		return Data.SystemText.Get(textId);
	}
}
