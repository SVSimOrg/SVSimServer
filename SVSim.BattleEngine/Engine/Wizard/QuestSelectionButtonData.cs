namespace Wizard;

public class QuestSelectionButtonData
{
	public enum SortPriority
	{
		CLEAR_PUZZLE,
		CLEAR_SECRET_BOSS,
		CLEAR_BOSS_RUSH,
		CLEAR_EVENT_STORY,
		QUEST,
		PUZZLE,
		EVENT_STORY,
		SECRET_BOSS,
		BOSS_RUSH
	}

	public enum PlateType
	{
		QUEST,
		PUZZLE,
		EVENT_STORY,
		BOSS_RUSH,
		SECRET_BOSS
	}

	private int _sortIndex;

	public QuestOpponentData QuestData { get; }

	public PuzzleQuestInfo PuzzleData { get; }

	public BossRushInfo BossRushData { get; }

	public EventStoryQuestInfo EventStoryData { get; }

	public SecretBossInfo SecretBossData { get; }

	public PlateType GetPlateType()
	{
		if (PuzzleData != null)
		{
			return PlateType.PUZZLE;
		}
		if (BossRushData != null)
		{
			return PlateType.BOSS_RUSH;
		}
		if (EventStoryData != null)
		{
			return PlateType.EVENT_STORY;
		}
		if (SecretBossData != null)
		{
			return PlateType.SECRET_BOSS;
		}
		return PlateType.QUEST;
	}

	public QuestSelectionButtonData(QuestOpponentData quest, int index)
	{
		QuestData = quest;
		_sortIndex = index;
	}

	public QuestSelectionButtonData(PuzzleQuestInfo puzzle)
	{
		PuzzleData = puzzle;
	}

	public QuestSelectionButtonData(BossRushInfo data)
	{
		BossRushData = data;
	}

	public QuestSelectionButtonData(EventStoryQuestInfo data)
	{
		EventStoryData = data;
	}

	public QuestSelectionButtonData(SecretBossInfo data)
	{
		SecretBossData = data;
	}

	private SortPriority GetPriority()
	{
		if (BossRushData != null)
		{
			if (!BossRushData.IsAllChallengeFinished)
			{
				return SortPriority.BOSS_RUSH;
			}
			return SortPriority.CLEAR_BOSS_RUSH;
		}
		if (PuzzleData != null)
		{
			if (PuzzleData.Status != PuzzleQuestStatus.Cleared)
			{
				return SortPriority.PUZZLE;
			}
			return SortPriority.CLEAR_PUZZLE;
		}
		if (EventStoryData != null)
		{
			if (!EventStoryData.IsAllFinish)
			{
				return SortPriority.EVENT_STORY;
			}
			return SortPriority.CLEAR_EVENT_STORY;
		}
		if (SecretBossData != null)
		{
			if (!SecretBossData.IsCleared)
			{
				return SortPriority.SECRET_BOSS;
			}
			return SortPriority.CLEAR_SECRET_BOSS;
		}
		return SortPriority.QUEST;
	}

	public int SortValue()
	{
		return (int)GetPriority() * 1000 + _sortIndex;
	}
}
