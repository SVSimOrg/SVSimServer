using System.Collections.Generic;

namespace Wizard;

public class PuzzleQuestData : Master.ReadFromCsv
{
	private int _playerEmblemId;

	private int _playerDegreeId;

	private int _puzzleBattelMasterId;

	public int Id { get; private set; }

	public string QuestNameTextId { get; private set; }

	public int StageId { get; private set; }

	public string ClearVoiceId { get; private set; }

	public int PlayerSkin { get; private set; }

	public int EnemySkin { get; private set; }

	public int EnemyEmblemId { get; private set; }

	public int EnemyDegreeId { get; private set; }

	public PuzzleBattleMasterData BattleData { get; private set; }

	public void ReadCsvColumns(string[] columns)
	{
		int num = 0;
		Id = int.Parse(columns[num]);
		num++;
		QuestNameTextId = columns[num];
		num++;
		StageId = int.Parse(columns[num]);
		num++;
		ClearVoiceId = columns[num];
		num++;
		PlayerSkin = int.Parse(columns[num]);
		num++;
		_playerEmblemId = (string.IsNullOrEmpty(columns[num]) ? (-1) : int.Parse(columns[num]));
		num++;
		_playerDegreeId = (string.IsNullOrEmpty(columns[num]) ? (-1) : int.Parse(columns[num]));
		num++;
		EnemySkin = int.Parse(columns[num]);
		num++;
		EnemyEmblemId = (string.IsNullOrEmpty(columns[num]) ? 100000000 : int.Parse(columns[num]));
		num++;
		EnemyDegreeId = (string.IsNullOrEmpty(columns[num]) ? 300003 : int.Parse(columns[num]));
		num++;
		_puzzleBattelMasterId = int.Parse(columns[num]);
		num++;
	}
}
