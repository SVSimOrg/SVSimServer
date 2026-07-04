using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class QuestOpponentData
{
	public string Name { get; private set; }

	public string Difficulty { get; private set; }

	public int ClearNum { get; private set; }

	public int QuestNum { get; private set; }

	public List<Format> BonusFormatList { get; private set; }

	public List<CardBasePrm.ClanType> BonusClassList { get; private set; }

	public QuestBattleData BattleData { get; private set; }

	public int WinCount { get; }

	public int WinCountForWinBonusPoint { get; }

	public QuestFinishDetail.WinBonusStatus WinBonusPointStatus { get; }

	public bool IsPlayable { get; private set; }

	public string PlayFactor { get; private set; } = string.Empty;

	public string MockBattleLabel { get; }

	public string NoticeLabel { get; }

	public QuestOpponentData(JsonData data)
	{
		Name = data["name"].ToString();
		Difficulty = data["difficulty_name"].ToString();
		ClearNum = data["clear_num"].ToInt();
		QuestNum = data["total_mission_num"].ToInt();
		BonusFormatList = new List<Format>();
		for (int i = 0; i < data["bonus_deck_format"].Count; i++)
		{
			BonusFormatList.Add(Data.ParseApiFormat(data["bonus_deck_format"][i].ToInt()));
		}
		BonusClassList = new List<CardBasePrm.ClanType>();
		for (int j = 0; j < data["bonus_class_id"].Count; j++)
		{
			BonusClassList.Add((CardBasePrm.ClanType)data["bonus_class_id"][j].ToInt());
		}
		WinCount = data.GetValueOrDefault("win_count", 0);
		WinCountForWinBonusPoint = data.GetValueOrDefault("required_win_count_for_win_bonus_point", 0);
		WinBonusPointStatus = (QuestFinishDetail.WinBonusStatus)data.GetValueOrDefault("win_bonus_point_status", 0);
		IsPlayable = data.GetValueOrDefault("is_released", defaultValue: true);
		string valueOrDefault = data.GetValueOrDefault("release_text_id", string.Empty);
		if (!string.IsNullOrEmpty(valueOrDefault))
		{
			PlayFactor = Data.SystemText.Get(valueOrDefault);
		}
		string valueOrDefault2 = data.GetValueOrDefault("mock_battle_text_id", string.Empty);
		MockBattleLabel = ((valueOrDefault2 != string.Empty) ? Data.SystemText.Get(valueOrDefault2) : null);
		string text = data["notice_text_id"].ToString();
		NoticeLabel = ((text != string.Empty) ? Data.SystemText.Get(text) : null);
		BattleData = new QuestBattleData(data);
	}
}
