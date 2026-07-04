using System;
using System.Collections.Generic;
using LitJson;
using Wizard.Battle.Recovery;
using Wizard.Story;

namespace Wizard;

public class StoryFinishTask : BaseTask
{
	public class StoryFinishTaskParam : BaseParam
	{
		public int story_id;

		public int is_finish;

		public int deck_no;

		public int deck_format;

		public int class_id;

		public Dictionary<string, int> mission;
	}

	public class StoryFinishTaskParamNoBattle : BaseParam
	{
		public int story_id;

		public int is_finish;
	}

	private readonly int _storyId;

	public StoryFinishTask(SelectedStoryInfo storyInfo)
	{
		base.type = GetApiType(storyInfo.StoryApiType);
		_storyId = storyInfo.FinishStoryId;
	}

	private static ApiType.Type GetApiType(StoryApiType storyType)
	{
		return storyType switch
		{
			StoryApiType.MainStory => ApiType.Type.MainStoryFinish, 
			StoryApiType.LimitedStory => ApiType.Type.LimitedStoryFinish, 
			StoryApiType.EventStory => ApiType.Type.EventStoryFinish, 
			_ => throw new NotImplementedException(), 
		};
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			DeleteRecoveryFileIfBattleAlreadyEnded(num);
			return num;
		}
		Data.StoryFinish.data = new StoryFinishDetail();
		Data.StoryFinish.data._responseData = base.ResponseData;
		Data.StoryFinish.data.class_chara_experience = 0;
		Data.StoryFinish.data.class_chara_level = 0;
		Data.StoryFinish.data.get_class_chara_experience = base.ResponseData["data"]["get_class_experience"].ToInt();
		Data.StoryFinish.data.class_chara_experience = base.ResponseData["data"]["class_experience"].ToInt();
		Data.StoryFinish.data.class_chara_level = base.ResponseData["data"]["class_level"].ToInt();
		Data.MyPageNotifications.ParseBadgeInfos(base.ResponseData);
		JsonData jsonData = base.ResponseData["data"]["achieved_info"];
		if (jsonData.GetJsonType() == JsonType.Object)
		{
			Data.StoryFinish.data.AchievedInfo.Read(jsonData);
		}
		Data.StoryFinish.data.StoryClearRewards = GetStoryClearRewards(base.ResponseData["data"]);
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}

	private static ReceivedReward[] GetStoryClearRewards(JsonData rootJsonData)
	{
		JsonData jsonData = rootJsonData["story_reward_list"];
		int count = jsonData.Count;
		ReceivedReward[] array = new ReceivedReward[count];
		for (int i = 0; i < count; i++)
		{
			JsonData jsonData2 = jsonData[i];
			array[i] = new ReceivedReward(jsonData2["reward_type"].ToInt(), jsonData2["reward_id"].ToLong(), jsonData2["reward_num"].ToInt());
		}
		return array;
	}
}
