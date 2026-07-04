using System;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class QuestPuzzleSelectionButton : QuestSelectionButtonBase
{
	[SerializeField]
	private UILabel _puzzleQuestClearNumLabel;

	[SerializeField]
	private UILabel _additionalPuzzleLabel;

	[SerializeField]
	private UILabel _puzzleLabel;

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private UIButton _button;

	[SerializeField]
	private UILabel _charaNameLabel;

	[SerializeField]
	private UILabel _clearLabel;

	[SerializeField]
	private UILabel _missionLabel;

	[SerializeField]
	private UILabel _noticeLabel;

	[SerializeField]
	private GameObject _questButtonRoot;

	private PuzzleQuestInfo _buttonData;

	public override void Initialize(QuestSelectionButtonData data, bool isOpenExtra, bool isLastDay, Action onClick = null)
	{
		_buttonData = data.PuzzleData;
		InitializePuzzleQuest(_buttonData, onClick);
	}

	public void InitializePuzzleQuest(PuzzleQuestInfo info, Action onClick = null)
	{
		if (onClick != null)
		{
			UIEventListener.Get(_button.gameObject).onClick = delegate
			{
				onClick();
			};
		}
		ClassCharacterMasterData charaPrmByCharaId = null; // Pre-Phase-5b: no chara master headless
		_charaNameLabel.text = charaPrmByCharaId.chara_name;
		_puzzleLabel.text = Data.SystemText.Get("Puzzle_QuestSelect_0002");
		SetTexture(info.CharaId.ToString());
		int num = info.DisplayDatas.Count((PuzzleQuestSelectDialog.DisplayData data) => data.IsCleared);
		int count = info.DisplayDatas.Count;
		if (num == count)
		{
			_missionLabel.gameObject.SetActive(value: false);
			_puzzleQuestClearNumLabel.gameObject.SetActive(value: false);
			_clearLabel.gameObject.SetActive(value: true);
		}
		else
		{
			_missionLabel.gameObject.SetActive(value: true);
			_missionLabel.GetComponent<StaticTextForUILabel>().enabled = false;
			_missionLabel.text = Data.SystemText.Get("Puzzle_QuestSelect_0003");
			_puzzleQuestClearNumLabel.gameObject.SetActive(value: true);
			_puzzleQuestClearNumLabel.text = $"{num}/{count}";
			_clearLabel.gameObject.SetActive(value: false);
		}
		SetNoticeLabel(null);
		SetAdditionalPuzzleLabel(info);
		SetButtonRootToGrey(isGrey: false);
	}

	private void SetAdditionalPuzzleLabel(PuzzleQuestInfo info)
	{
		bool flag = info?.DisplayDatas.Any((PuzzleQuestSelectDialog.DisplayData x) => x.IsAdditional && !x.IsCleared) ?? false;
		_additionalPuzzleLabel.gameObject.SetActive(flag);
		if (flag)
		{
			_additionalPuzzleLabel.text = Data.SystemText.Get("Puzzle_QuestSelect_0004");
		}
	}

	private void SetNoticeLabel(QuestOpponentData data)
	{
		bool flag = data != null && data.IsPlayable && data.NoticeLabel != null;
		_noticeLabel.gameObject.SetActive(flag);
		if (flag)
		{
			_noticeLabel.text = data.NoticeLabel;
		}
	}

	protected override void SetTexture(string textureId)
	{
		_texture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(textureId, ResourcesManager.AssetLoadPathType.ClassCharaWideThumbnail, isfetch: true));
	}

	protected override void SetButtonRootToGrey(bool isGrey)
	{
		UIManager.SetObjectToGrey(_questButtonRoot.gameObject, isGrey, null, AllLabelColorChanger.COLOR_TABLE_QUEST_BUTTON);
	}

	public override void SelectChara()
	{
		_questSelectionPage.SelectCharaPuzzleButton(_buttonData);
	}

	public override void OnDecideButtonClick()
	{
		_questSelectionPage.OnPuzzleDecideButtonClick();
	}
}
