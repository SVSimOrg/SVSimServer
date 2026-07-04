using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace Wizard;

public class PuzzleQuestInfo
{
	private Dictionary<string, string> _difficultyNameTable;

	public int CharaId { get; }

	public PuzzleQuestStatus Status { get; }

	public List<PuzzleQuestSelectDialog.DisplayData> DisplayDatas { get; }

	public PuzzleQuestInfo(JsonData data)
	{
		CharaId = data.GetValueOrDefault("puzzle_quest_chara_id", 0);
		if (data.TryGetValue("puzzle_quest", out var value))
		{
			bool valueOrDefault = data.GetValueOrDefault("is_display_puzzle_new", defaultValue: false);
			DisplayDatas = PuzzleQuestSelectDialog.CreateDisplayData(valueOrDefault, value);
			_difficultyNameTable = CreateDifficultyNameTable(data["puzzle_difficulty_name_list"]);
		}
		else
		{
			DisplayDatas = new List<PuzzleQuestSelectDialog.DisplayData>();
		}
		Status = GetStatus(DisplayDatas);
	}

	public string GetDifficultyName(int difficulty)
	{
		return _difficultyNameTable[$"{difficulty}"];
	}

	private static PuzzleQuestStatus GetStatus(List<PuzzleQuestSelectDialog.DisplayData> displayDatas)
	{
		if (displayDatas.Count <= 0)
		{
			return PuzzleQuestStatus.None;
		}
		if (!displayDatas.All((PuzzleQuestSelectDialog.DisplayData x) => x.IsCleared))
		{
			return PuzzleQuestStatus.InProgress;
		}
		return PuzzleQuestStatus.Cleared;
	}

	private static Dictionary<string, string> CreateDifficultyNameTable(JsonData data)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key2 in data.Keys)
		{
			string key = data[key2].ToString();
			dictionary[key] = key2;
		}
		return dictionary;
	}
}
