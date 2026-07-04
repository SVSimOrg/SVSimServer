using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wizard;

public class QuestLastUsedDeckSaveDataManager
{
	public class ExtractedDeckData : IEquatable<ExtractedDeckData>
	{
		public Format Format { get; }

		public int ID { get; }

		public DateTime? CreatedTime { get; }

		public ExtractedDeckData(DeckData deckData)
		{
			Format = deckData.Format;
			ID = deckData.GetDeckID();
			CreatedTime = deckData.CreatedTime;
		}

		public ExtractedDeckData(Format format, int id, DateTime? createdTime)
		{
			Format = format;
			ID = id;
			CreatedTime = createdTime;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ExtractedDeckData);
		}

		public bool Equals(ExtractedDeckData x)
		{
			if ((object)x == null)
			{
				return false;
			}
			if ((object)this == x)
			{
				return true;
			}
			if (GetType() != x.GetType())
			{
				return false;
			}
			if (Format == x.Format && ID == x.ID)
			{
				DateTime? createdTime = CreatedTime;
				DateTime? createdTime2 = x.CreatedTime;
				if (createdTime.HasValue != createdTime2.HasValue)
				{
					return false;
				}
				if (!createdTime.HasValue)
				{
					return true;
				}
				return createdTime.GetValueOrDefault() == createdTime2.GetValueOrDefault();
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Format, ID, CreatedTime).GetHashCode();
		}

		public static bool operator ==(ExtractedDeckData lhs, ExtractedDeckData rhs)
		{
			return lhs?.Equals(rhs) ?? ((object)rhs == null);
		}

		public static bool operator !=(ExtractedDeckData lhs, ExtractedDeckData rhs)
		{
			return !(lhs == rhs);
		}
	}

	private class SaveData
	{
		public readonly Dictionary<int, ExtractedDeckData> StageDeckTable = new Dictionary<int, ExtractedDeckData>();

		public int? QuestId { get; set; }
	}

	private static readonly string Version = "1";

	private static readonly char DataDelimiter = ',';

	private static readonly char DeckDataDelimiter = '&';

	private readonly SaveData _saveData;

	public int? QuestId => _saveData.QuestId;

	public QuestLastUsedDeckSaveDataManager()
	{
		string value = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.QUEST_LAST_USED_DECK_INFO);
		_saveData = DeserializeSaveData(value);
	}

	private void Save()
	{
		PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.QUEST_LAST_USED_DECK_INFO, Serialize(_saveData));
	}

	public void DeleteAll()
	{
		_saveData.QuestId = null;
		_saveData.StageDeckTable.Clear();
		Save();
	}

	public void SaveQuestId(int questId)
	{
		if (_saveData.QuestId != questId)
		{
			_saveData.QuestId = questId;
			Save();
		}
	}

	private static SaveData DeserializeSaveData(string serializedData)
	{
		if (serializedData == string.Empty)
		{
			return new SaveData();
		}
		string[] array = serializedData.Split(DataDelimiter);
		if (array[0] != Version)
		{
			return new SaveData();
		}
		SaveData saveData = new SaveData();
		if (int.TryParse(array[1], out var result))
		{
			saveData.QuestId = result;
		}
		for (int i = 2; i < array.Length; i += 2)
		{
			int key = int.Parse(array[i]);
			ExtractedDeckData value = DeserializeDeckData(array[i + 1]);
			saveData.StageDeckTable[key] = value;
		}
		return saveData;
	}

	private static ExtractedDeckData DeserializeDeckData(string serializedData)
	{
		string[] array = serializedData.Split(DeckDataDelimiter);
		Format format = (Format)int.Parse(array[0]);
		int id = int.Parse(array[1]);
		DateTime? createdTime = null;
		if (DateTime.TryParse(array[2], out var result))
		{
			createdTime = result;
		}
		return new ExtractedDeckData(format, id, createdTime);
	}

	private static string Serialize(SaveData saveData)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string arg = (saveData.QuestId.HasValue ? $"{saveData.QuestId.Value}" : string.Empty);
		stringBuilder.Append($"{Version}{DataDelimiter}{arg}");
		foreach (KeyValuePair<int, ExtractedDeckData> item in saveData.StageDeckTable)
		{
			stringBuilder.Append($"{DataDelimiter}{item.Key}{DataDelimiter}{Serialize(item.Value)}");
		}
		return stringBuilder.ToString();
	}

	private static string Serialize(ExtractedDeckData deckData)
	{
		string text = (deckData.CreatedTime.HasValue ? $"{deckData.CreatedTime.Value}" : string.Empty);
		return $"{(int)deckData.Format}{DeckDataDelimiter}{deckData.ID}{DeckDataDelimiter}{text}";
	}
}
