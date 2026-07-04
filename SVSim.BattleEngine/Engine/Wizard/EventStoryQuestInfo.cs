using LitJson;

namespace Wizard;

public class EventStoryQuestInfo
{
	public bool EventStoryExist { get; private set; }

	public int SectionId { get; private set; }

	public int ChapterTotalNum { get; private set; }

	public int UnlockChapterNum { get; private set; }

	public bool IsAllFinish { get; private set; }

	public int NextChapterUnlockPoint { get; private set; }

	public bool IsEventStoryInProgress { get; private set; }

	public EventStoryQuestInfo(JsonData data)
	{
		if (data.TryGetValue("event_story", out var value))
		{
			EventStoryExist = true;
			SectionId = value["section_id"].ToInt();
			ChapterTotalNum = value["chapter_total_num"].ToInt();
			UnlockChapterNum = value["unlock_chapter_num"].ToInt();
			IsAllFinish = value["is_all_finish"].ToBoolean();
			NextChapterUnlockPoint = value["next_chapter_unlock_point"].ToInt();
			IsEventStoryInProgress = !IsAllFinish;
		}
		else
		{
			EventStoryExist = false;
		}
	}
}
