using System;
using LitJson;
using Wizard.Story;

namespace Wizard;

public class StoryLeaderSelectTask : BaseTask
{
	public class StoryLeaderSelectTaskParam : BaseParam
	{
		public int section_id;

		public StoryLeaderSelectTaskParam(int sectionId)
		{
			section_id = sectionId;
		}
	}

	public StoryLeaderSelectTask(SelectedStoryInfo storyInfo)
	{
		base.type = GetApiType(storyInfo.StoryApiType);
		base.Params = new StoryLeaderSelectTaskParam(storyInfo.SectionId);
	}

	private static ApiType.Type GetApiType(StoryApiType storyType)
	{
		return storyType switch
		{
			StoryApiType.MainStory => ApiType.Type.MainStoryLeaderSelect, 
			StoryApiType.LimitedStory => ApiType.Type.LimitedStoryLeaderSelect, 
			StoryApiType.EventStory => ApiType.Type.EventStoryLeaderSelect, 
			_ => throw new NotImplementedException(), 
		};
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.StoryLeaderSelect.DataList.Clear();
		JsonData jsonData = base.ResponseData["data"];
		JsonData jsonData2 = jsonData["leader_list"];
		for (int i = 0; i < jsonData2.Count; i++)
		{
			StoryLeaderSelectData storyLeaderSelectData = new StoryLeaderSelectData();
			storyLeaderSelectData.CharaId = jsonData2[i]["chara_id"].ToInt();
			storyLeaderSelectData.IsFinished = jsonData2[i]["is_finished"].ToBoolean();
			storyLeaderSelectData.CurrentChapter = jsonData2[i]["current_chapter"].ToString();
			Data.StoryLeaderSelect.DataList.Add(storyLeaderSelectData);
		}
		if (jsonData.Keys.Contains("leader_count"))
		{
			Data.StoryLeaderSelect.LeaderCount = jsonData["leader_count"].ToInt();
		}
		else
		{
			Data.StoryLeaderSelect.LeaderCount = 8;
		}
		return num;
	}
}
