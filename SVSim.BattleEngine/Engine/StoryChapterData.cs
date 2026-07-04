using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using Wizard;

public class StoryChapterData : HeaderData
{
	public enum ChapterClearStatus
	{
		NotCleared,
		AlreadyRead,
		Cleared
	}

	public class StoryReward
	{
		public int RewardType { get; }

		public long RewardUserGoodsId { get; }

		public int RewardNumber { get; }

		public StoryReward(JsonData data)
		{
			RewardType = data["reward_type"].ToInt();
			RewardUserGoodsId = data["reward_detail_id"].ToLong();
			RewardNumber = data["reward_number"].ToInt();
		}
	}

	public class SubChapterData
	{
		public int StoryId { get; private set; }

		public int SubChapterId { get; private set; }

		public ChapterClearStatus ClearStatus { get; private set; }

		public bool IsMaintenanceChapter { get; private set; }

		public SubChapterData(JsonData jsonData)
		{
			StoryId = jsonData["story_id"].ToInt();
			SubChapterId = jsonData["sub_chapter_id"].ToInt();
			ClearStatus = (jsonData["is_finish"].ToBoolean() ? ChapterClearStatus.Cleared : ChapterClearStatus.NotCleared);
			IsMaintenanceChapter = jsonData.GetValueOrDefault("is_maintenance_chapter", defaultValue: false);
		}
	}

	public int StoryId { get; }

	public int SectionId { get; }

	public int CharaId { get; }

	public string ChapterId { get; }

	public string NextChapterId { get; }

	public List<SubChapterData> SubChapterDatas { get; }

	public bool ExistsSecondHalfStory { get; }

	public bool ExistsStoryMovie { get; }

	public bool IsMaintenanceChapter { get; set; }

	public bool IsReleased { get; }

	public bool IsLocked { get; }

	public string UnlockConditionText { get; }

	public ChapterClearStatus ClearStatus { get; }

	public StoryReward[] Rewards { get; }

	public string LastChapterClearTextId { get; }

	public int ChapterRowNum { get; }

	public string SelectionDisplayPosition { get; }

	public string SelectionTextId { get; }

	public string RequiredChapterId { get; }

	public int RequiredChapterRowNum { get; }

	public int NextChapterRowNum { get; }

	public string[] NextChapterSplitted { get; }

	public string RouteName { get; }

	public bool ExistsBattle { get; }

	public bool IsEnableBattleSkip { get; }

	public int EnemyAiId { get; }

	public List<BattleSettingData> BattleSettingDatas { get; }

	public Vector2 MapIconPos { get; }

	public bool IsDisplayMapIcon { get; }

	public bool IsEnableDragMapBG { get; }

	public int ChapterButtonBgId { get; }

	public string ChapterEffectPath { get; }

	public bool IsReleasedAnotherEnding { get; }

	public bool IsPlayAnotherEndingAppearanceAnimation { get; }

	public int TutorialStep { get; }

	public bool ExistsSubChapter => SubChapterDatas.Count > 0;

	public BattleSettingData FindBattleSettingDataByPlayerCharaId(int playerCharaId)
	{
		return BattleSettingDatas.Find((BattleSettingData x) => x.PlayerCharaId == playerCharaId);
	}

	private ChapterClearStatus GetClearStatusUsingSubChapter()
	{
		if (!ExistsSubChapter)
		{
			return ChapterClearStatus.Cleared;
		}
		int num = SubChapterDatas.Where((SubChapterData item) => item.ClearStatus == ChapterClearStatus.Cleared).Count();
		if (num == SubChapterDatas.Count())
		{
			return ChapterClearStatus.Cleared;
		}
		if (num == 0)
		{
			return ChapterClearStatus.NotCleared;
		}
		return ChapterClearStatus.AlreadyRead;
	}

	public StoryChapterData(JsonData data)
	{
		StoryId = data["story_id"].ToInt();
		SectionId = data["section_id"].ToInt();
		CharaId = data["chara_id"].ToInt();
		ChapterId = data["chapter_id"].ToString();
		NextChapterId = data["next_chapter_id"].ToString();
		SubChapterDatas = GetSubChapterDatas(data);
		ExistsSecondHalfStory = data["battle_exists"].ToBoolean();
		ExistsStoryMovie = data["show_subtitles"].ToInt() == 1;
		IsMaintenanceChapter = data["is_maintenance_chapter"].ToBoolean();
		IsReleased = data["is_released"].ToBoolean();
		IsLocked = data["is_lock"].ToBoolean();
		string text = data["unlock_text"].ToString();
		UnlockConditionText = (string.IsNullOrEmpty(text) ? null : text.Replace("\\n", "\n"));
		ClearStatus = GetChapterClearStatus(data);
		Rewards = GetRewards(data);
		string text2 = data["chapter_clear_text_id"].ToString();
		LastChapterClearTextId = (string.IsNullOrEmpty(text2) ? null : text2);
		ExistsBattle = data["battle_exists"].ToBoolean();
		IsEnableBattleSkip = data["battle_exists"].ToBoolean() && data["is_skip_enabled"].ToBoolean();
		EnemyAiId = data["enemy_ai_id"].ToInt();
		BattleSettingDatas = GetBattleSettingDatas(data, new BattleSettingBaseData(data));
		float x = float.Parse(data["x_coordinate"].ToString());
		float y = float.Parse(data["y_coordinate"].ToString());
		MapIconPos = new Vector2(x, y);
		IsDisplayMapIcon = data["show_coordinate"].ToInt() == 1;
		IsEnableDragMapBG = data["is_camera_ movable"].ToInt() == 1;
		ChapterButtonBgId = int.Parse(data["bg_file_name"].ToString());
		ChapterEffectPath = data["chapter_effect_path"].ToString();
		SelectionDisplayPosition = data.GetValueOrDefault("selection_display_position", "");
		SelectionTextId = data.GetValueOrDefault("selection_text_id", "");
		RequiredChapterId = data.GetValueOrDefault("required_chapter_id", "");
		ChapterRowNum = ((!string.IsNullOrEmpty(ChapterId)) ? UIUtil.ExtractStringNumber(ChapterId) : 0);
		RequiredChapterRowNum = ((!string.IsNullOrEmpty(RequiredChapterId)) ? UIUtil.ExtractStringNumber(RequiredChapterId) : 0);
		int[] array = (from s in NextChapterId.Split(' ')
			select UIUtil.ExtractStringNumber(s)).Distinct().ToArray();
		NextChapterRowNum = array[0];
		NextChapterSplitted = NextChapterId.Split(' ');
		RouteName = GetRouteName(SectionId, CharaId, ChapterId);
		IsReleasedAnotherEnding = data["is_released_another_end"].ToBoolean();
		IsPlayAnotherEndingAppearanceAnimation = data["is_play_another_end_appearance_animation"].ToBoolean();
	}

	public StoryChapterData(TutorialAreaSelect data)
	{
		int tutorial_step = Data.Load.data._userTutorial.tutorial_step;
		bool flag = tutorial_step == 100;
		SectionId = 0;
		CharaId = 500008;
		ChapterId = data.ChapterId;
		SubChapterDatas = new List<SubChapterData>();
		ExistsSecondHalfStory = false;
		ExistsStoryMovie = data.ExistsStoryMovie;
		IsMaintenanceChapter = false;
		IsReleased = tutorial_step >= data.TutorialStep;
		ClearStatus = ((tutorial_step > data.TutorialStep) ? ChapterClearStatus.Cleared : ChapterClearStatus.NotCleared);
		Rewards = new StoryReward[0];
		ExistsBattle = true;
		IsEnableBattleSkip = flag;
		BattleSettingDatas = new List<BattleSettingData>();
		MapIconPos = data.MapIconPos;
		IsDisplayMapIcon = true;
		IsEnableDragMapBG = true;
		ChapterButtonBgId = data.ChapterButtonBgId;
		ChapterEffectPath = string.Empty;
		TutorialStep = data.TutorialStep;
	}

	private static List<SubChapterData> GetSubChapterDatas(JsonData jsonData)
	{
		List<SubChapterData> list = new List<SubChapterData>();
		if (jsonData.TryGetValue("sub_chapters", out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				list.Add(new SubChapterData(value[i]));
			}
		}
		return list;
	}

	private static StoryReward[] GetRewards(JsonData jsonData)
	{
		JsonData jsonData2 = jsonData["story_reward"];
		StoryReward[] array = new StoryReward[jsonData2.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new StoryReward(jsonData2[i]);
		}
		return array;
	}

	private static List<BattleSettingData> GetBattleSettingDatas(JsonData jsonData, BattleSettingBaseData baseData)
	{
		JsonData jsonData2 = jsonData["battle_settings"];
		List<BattleSettingData> list = new List<BattleSettingData>();
		for (int i = 0; i < jsonData2.Count; i++)
		{
			JsonData jsonData3 = jsonData2[i];
			if (jsonData3["deck_class_id"].ToInt() != 0)
			{
				list.Add(new BattleSettingData(jsonData3, baseData));
			}
		}
		return list;
	}

	private ChapterClearStatus GetChapterClearStatus(JsonData jsonData)
	{
		if (ExistsSubChapter)
		{
			return GetClearStatusUsingSubChapter();
		}
		if (jsonData["is_finish"].ToBoolean())
		{
			return ChapterClearStatus.Cleared;
		}
		if (jsonData["is_skipped"].ToBoolean())
		{
			return ChapterClearStatus.AlreadyRead;
		}
		return ChapterClearStatus.NotCleared;
	}

	private static string GetRouteName(int sectionId, int charaId, string chapterId)
	{
		string text = UIUtil.ExtractStringAlphabet(chapterId);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string key = $"Route_{sectionId}_{charaId}_{text}";
		return Data.SystemText.Get(key);
	}
}
