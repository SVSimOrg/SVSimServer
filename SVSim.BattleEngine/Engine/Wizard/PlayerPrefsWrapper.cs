using System.Collections.Generic;
using UnityEngine;
// TODO(engine-cleanup-pass2): 14 of 15 methods unrun in baseline
//   Type: Wizard.PlayerPrefsWrapper
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public static class PlayerPrefsWrapper
{
	public static readonly int TRUE = 1;

	public static readonly int FALSE = 0;

	public static readonly KeyValuePair<string, int> SOUND_MUTE = new KeyValuePair<string, int>("SOUND_MUTE", FALSE);

	public static readonly KeyValuePair<string, int> MOVIE_SUBTITLES = new KeyValuePair<string, int>("MOVIE_SUBTITLES", FALSE);

	public static readonly KeyValuePair<string, int> CONFIRM_TURN_END = new KeyValuePair<string, int>("CONFIRM_TURN_END", FALSE);

	public static readonly KeyValuePair<string, int> CONFIRM_TURN_END_WITHOUT_USING_HERO_SKILL = new KeyValuePair<string, int>("CONFIRM_TURN_END_WITHOUT_USING_HERO_SKILL", TRUE);

	public static readonly KeyValuePair<string, int> CONFIRM_EVOLVE = new KeyValuePair<string, int>("CONFIRM_EVOLVE", FALSE);

	public static readonly KeyValuePair<string, int> FIXEDUSE_COST_INFO = new KeyValuePair<string, int>("FIXEDUSE_COST_INFO", FALSE);

	public static readonly KeyValuePair<string, int> SHOW_OTHER_PLAYER_EMOTE = new KeyValuePair<string, int>("SHOW_OPPONENT_EMOTE", TRUE);

	public static readonly KeyValuePair<string, int> SHOW_OPPONENT_DEFAULT_SKIN = new KeyValuePair<string, int>("SHOW_OPPONENT_DEFAULT_SKIN", FALSE);

	public static readonly KeyValuePair<string, int> SHOW_FOIL_CARD_ANIMATION = new KeyValuePair<string, int>("SHOW_FOIL_CARD_ANIMATION", TRUE);

	public static readonly KeyValuePair<string, int> SHOW_BATTLE_EFFECT = new KeyValuePair<string, int>("SHOW_BATTLE_EFFECT", FALSE);

	public static readonly KeyValuePair<string, int> SHOW_LEADER_ANIMATION = new KeyValuePair<string, int>("SHOW_LEADER_ANIMATION", TRUE);

	public static readonly KeyValuePair<string, int> SHOW_PANEL_ALWAYS = new KeyValuePair<string, int>("SHOW_PANEL_ALWAYS", FALSE);

	public static readonly KeyValuePair<string, int> SHOW_PREDICTION_ICONS = new KeyValuePair<string, int>("SHOW_PREDICTION_ICONS", FALSE);

	public static readonly KeyValuePair<string, int> DEVICE_ORIENTATION = new KeyValuePair<string, int>("DEVICE_ORIENTATION", FALSE);

	public static readonly KeyValuePair<string, int> AUTO_MESSAGE = new KeyValuePair<string, int>("AUTO_MESSAGE", FALSE);

	public static readonly KeyValuePair<string, int> SHOW_SIDE_LOG = new KeyValuePair<string, int>("SHOW_SIDE_LOG", TRUE);

	public static readonly KeyValuePair<string, int> SHOW_FUSION_CARD_PLAY_DIALOG = new KeyValuePair<string, int>("SHOW_FUSION_CARD_PLAY_DIALOG", FALSE);

	public static readonly KeyValuePair<string, bool> CARDPACK_CARD_AUTO_OPEN = new KeyValuePair<string, bool>("SKIPCARDPACK", value: false);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_DECK_ID = new KeyValuePair<string, int>("LAST_BATTLE_DECK_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_DECK_ID_FOR_MYPAGE = new KeyValuePair<string, int>("LAST_BATTLE_DECK_ID_FOR_MYPAGE", 1);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_DECK_FORMAT_FOR_SINGLE_RECOVER = new KeyValuePair<string, int>("LAST_BATTLE_DECK_FORMAT_FOR_SINGLE_RECOVER", 2);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_LEADER_ID = new KeyValuePair<string, int>("LAST_BATTLE_LEADER_ID", 0);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_IS_DEFDECK = new KeyValuePair<string, int>("LAST_BATTLE_IS_DEFAULT_DECK_FOR_BATTLE", FALSE);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_IS_TRIALDECK = new KeyValuePair<string, int>("LAST_BATTLE_IS_TRIAL_DECK_FOR_BATTLE", FALSE);

	public static readonly KeyValuePair<string, string> LAST_BATTLE_NAME_TRIALDECK = new KeyValuePair<string, string>("LAST_BATTLE_NAME_TRIALDECK", "");

	public static readonly KeyValuePair<string, string> LAST_BATTLE_DECK_CARD_LIST = new KeyValuePair<string, string>("LAST_BATTLE_DECK_CARD_LIST", "");

	public static readonly KeyValuePair<string, string> LAST_BATTLE_DECK_SLEEVE_ID = new KeyValuePair<string, string>("LAST_BATTLE_DECK_SLEEVE_ID", "");

	public static readonly KeyValuePair<string, string> LAST_BATTLE_ENEMY_CARD_ID_ROOM1 = new KeyValuePair<string, string>("LAST_BATTLE_ENEMY_CARD_ID_ROOM1", string.Empty);

	public static readonly KeyValuePair<string, string> LAST_BATTLE_ENEMY_CARD_ID_ROOM2 = new KeyValuePair<string, string>("LAST_BATTLE_ENEMY_CARD_ID_ROOM2", string.Empty);

	public static readonly KeyValuePair<string, string> LAST_BATTLE_ENEMY_CARD_ID_ROOM3 = new KeyValuePair<string, string>("LAST_BATTLE_ENEMY_CARD_ID_ROOM3", string.Empty);

	public static readonly KeyValuePair<string, string> LAST_BATTLE_ENEMY_CARD_ID_ROOM4 = new KeyValuePair<string, string>("LAST_BATTLE_ENEMY_CARD_ID_ROOM4", string.Empty);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_ENEMY_CARD_ID_FORMAT1 = new KeyValuePair<string, int>("LAST_BATTLE_ENEMY_CARD_ID_FORMAT1", 2);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_ENEMY_CARD_ID_FORMAT2 = new KeyValuePair<string, int>("LAST_BATTLE_ENEMY_CARD_ID_FORMAT2", 2);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_ENEMY_CARD_ID_FORMAT3 = new KeyValuePair<string, int>("LAST_BATTLE_ENEMY_CARD_ID_FORMAT3", 2);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_ENEMY_CARD_ID_FORMAT4 = new KeyValuePair<string, int>("LAST_BATTLE_ENEMY_CARD_ID_FORMAT4", 2);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_PLAYER_BAN_DECK_ID = new KeyValuePair<string, int>("LAST_BATTLE_PLAYER_BAN_DECK_ID", 1);

	public static readonly KeyValuePair<string, int> LAST_BATTLE_ENEMY_BAN_DECK_ID = new KeyValuePair<string, int>("LAST_BATTLE_ENEMY_BAN_DECK_ID", 1);

	public static readonly KeyValuePair<string, string> QUEST_LAST_USED_DECK_INFO = new KeyValuePair<string, string>("QUEST_LAST_USED_DECK_INFO", string.Empty);

	public static readonly KeyValuePair<string, string> FIRST_TIPS = new KeyValuePair<string, string>("FIRST_TIPS", "0");

	public static readonly KeyValuePair<string, int> FIRST_TIPS_AFTER_ROTATION_USER_FLAG = new KeyValuePair<string, int>("FIRST_TIPS_AFTER_ROTATION_USER_FLAG", 0);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_QUEST_ID = new KeyValuePair<string, int>("FIRST_TIPS_QUEST_ID", 0);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_ADDITIONAL_PUZZLE_QUEST_ID = new KeyValuePair<string, int>("FIRST_TIPS_ADDITIONAL_PUZZLE_QUEST_ID", 0);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_BOSSRUSH_QUEST_ID = new KeyValuePair<string, int>("FIRST_TIPS_BOSSRUSH_QUEST_ID", 0);

	public static readonly KeyValuePair<string, string> BOSSRUSH_LAST_USED_DECK_INFO = new KeyValuePair<string, string>("BOSSRUSH_LAST_USED_DECK_INFO", string.Empty);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_COLOSSEUM_ID = new KeyValuePair<string, int>("FIRST_TIPS_COLOSSEUM_ID", 0);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_NEUTRAL_POPULARITY_VOTE_CAMPAIGN_ID = new KeyValuePair<string, int>("FIRST_TIPS_NEUTRAL_POPULARITY_VOTE_CAMPAIGN_ID", 0);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_LEADER_POPULARITY_VOTE_CAMPAIGN_ID = new KeyValuePair<string, int>("FIRST_TIPS_LEADER_POPULARITY_VOTE_CAMPAIGN_ID", 0);

	public static readonly KeyValuePair<string, int> FIRST_TIPS_BINGO_ID = new KeyValuePair<string, int>("FIRST_TIPS_BINGO_ID", 0);

	public static readonly KeyValuePair<string, string> INSTALL_REGION_CODE = new KeyValuePair<string, string>("INSTALL_REGION_CODE", "");

	public static readonly KeyValuePair<string, string> CURRENT_REGION_CODE = new KeyValuePair<string, string>("CURRENT_REGION_CODE", "");

	public static readonly KeyValuePair<string, int> SIMPLE_STAGE = new KeyValuePair<string, int>("SIMPLE_STAGE", FALSE);

	public static readonly KeyValuePair<string, int> COLLABORATION_SOUND = new KeyValuePair<string, int>("COLLABORATION_SOUND", TRUE);

	public static readonly KeyValuePair<string, int> USE_OFF_STAGE = new KeyValuePair<string, int>("USE_OFF_STAGE", TRUE);

	public static readonly KeyValuePair<string, int> OFF_STAGE = new KeyValuePair<string, int>("OFF_STAGE", 0);

	public static readonly KeyValuePair<string, string> OFF_STAGE_ID = new KeyValuePair<string, string>("OFF_STAGE_ID", "");

	public static readonly KeyValuePair<string, string> VIDEOHOSTING_VERSION = new KeyValuePair<string, string>("VIDEOHOSTING_VERSION", "");

	public static readonly KeyValuePair<string, int> VIDEOHOSTING_FLAGS = new KeyValuePair<string, int>("VIDEOHOSTING_FLAGS", 1);

	public static readonly KeyValuePair<string, float> VIDEOHOSTING_MICROPHONE_GAIN = new KeyValuePair<string, float>("VIDEOHOSTING_MICROPHONE_GAIN", 0.5f);

	public static readonly KeyValuePair<string, int> SOCIAL_ACHIEVEMENT_SIGNIN = new KeyValuePair<string, int>("SOCIAL_ACHIEVEMENT_SIGNIN", TRUE);

	public static readonly KeyValuePair<string, int> PLAY_SOUND_IN_BACKGROUND = new KeyValuePair<string, int>("PLAY_SOUND_IN_BACKGROUND", TRUE);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_EMOTE_MINE = new KeyValuePair<string, int>("ROOM_MATCH_EMOTE_MINE", -1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_EMOTE_ENEMY = new KeyValuePair<string, int>("ROOM_MATCH_EMOTE_ENEMY", -1);

	public static readonly KeyValuePair<string, string> ROOM_MATCH_DISPLAY_ID = new KeyValuePair<string, string>("ROOM_MATCH_DISPLAY_ID", "");

	public static readonly KeyValuePair<string, int> ROOM_MATCH_RULE = new KeyValuePair<string, int>("ROOM_MATCH_RULE", -1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_ENEMY_IS_FRIEND = new KeyValuePair<string, int>("ROOM_MATCH_ENEMY_IS_FRIEND", -1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_ENEMY_IS_SAME_GUILD = new KeyValuePair<string, int>("ROOM_MATCH_ENEMY_IS_SAME_GUILD", -1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_ENEMY_IS_JOIN_GUILD = new KeyValuePair<string, int>("ROOM_MATCH_ENEMY_IS_JOIN_GUILD", -1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_OWNER_IS_FIRST_TURN = new KeyValuePair<string, int>("ROOM_MATCH_OWNER_IS_FIRST_TURN", -1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_FRIEND_WATCH_PERMIT = new KeyValuePair<string, int>("ROOM_MATCH_FRIEND_WATCH_PERMIT", FALSE);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_GUILD_WATCH_PERMIT = new KeyValuePair<string, int>("ROOM_MATCH_GUILD_WATCH_PERMIT", FALSE);

	public static readonly KeyValuePair<string, int> LAST_ROOM_MATCH_BASE_RULE = new KeyValuePair<string, int>("LAST_ROOM_MATCH_BASE_RULE", 0);

	public static readonly KeyValuePair<string, int> LAST_ROOM_MATCH_RULE = new KeyValuePair<string, int>("LAST_ROOM_MATCH_RULE", 1);

	public static readonly KeyValuePair<string, int> LAST_ROOM_MATCH_RULE_2PICK = new KeyValuePair<string, int>("LAST_ROOM_MATCH_RULE_2PICK", 1);

	public static readonly KeyValuePair<string, int> LAST_ROOM_MATCH_FORMAT_2PICK = new KeyValuePair<string, int>("LAST_ROOM_MATCH_FORMAT_2PICK", 1);

	public static readonly KeyValuePair<string, int> ROOM_MATCH_FORMAT = new KeyValuePair<string, int>("ROOM_MATCH_FORMAT", 0);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_ID_ROTATION = new KeyValuePair<string, int>("LAST_SELECT_DECK_ID_ROTATION", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_ID_UNLIMITED = new KeyValuePair<string, int>("LAST_SELECT_DECK_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_ID_PRE_ROTATION = new KeyValuePair<string, int>("LAST_SELECT_DECK_ID_PRE_ROTATION", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_ID_CROSSOVER = new KeyValuePair<string, int>("LAST_SELECT_DECK_ID_CROSSOVER", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_ID_MY_ROTATION = new KeyValuePair<string, int>("LAST_SELECT_DECK_ID_MY_ROTATION", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_ID_AVATAR = new KeyValuePair<string, int>("LAST_SELECT_DECK_ID_AVATAR", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_DECK_FORMAT = new KeyValuePair<string, int>("LAST_SELECT_DECK_FORMAT", 2);

	public static readonly KeyValuePair<string, int> CONVENTION_LAST_SELECT_DECK_ID = new KeyValuePair<string, int>("CONVENTION_LAST_SELECT_DECK_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_SELECT_IS_DEFDECK_ROTATION = new KeyValuePair<string, int>("LAST_SELECT_IS_DEFDECK_ROTATION", TRUE);

	public static readonly KeyValuePair<string, int> LAST_SELECT_IS_DEFDECK_UNLIMITED = new KeyValuePair<string, int>("LAST_SELECT_IS_DEFDECK", TRUE);

	public static readonly KeyValuePair<string, int> LAST_SELECT_IS_BUILD_DECK = new KeyValuePair<string, int>("LAST_SELECT_IS_BUILD_DECK", FALSE);

	public static readonly KeyValuePair<string, int> MOUSE_CONTROL = new KeyValuePair<string, int>("MOUSE_CONTROL", FALSE);

	public static readonly KeyValuePair<string, int> MOUSE_SHORTCUT = new KeyValuePair<string, int>("MOUSE_SHORTCUT", 1);

	public static readonly KeyValuePair<string, int> MOUSE_SHORTCUT_PLAY = new KeyValuePair<string, int>("MOUSE_SHORTCUT_PLAY", 1);

	public static readonly KeyValuePair<string, int> MOUSE_SHORTCUT_EVOLUTION = new KeyValuePair<string, int>("MOUSE_SHORTCUT_EVOLUTION", 1);

	public static readonly KeyValuePair<string, int> MOUSE_SHORTCUT_DETAIL = new KeyValuePair<string, int>("MOUSE_SHORTCUT_DETAIL", 3);

	public static readonly KeyValuePair<string, int> FRAMERATE = new KeyValuePair<string, int>("FRAMERATE", 0);

	public static readonly KeyValuePair<string, int> SHOWQRCODE = new KeyValuePair<string, int>("SHOWQRCODE", 0);

	public static readonly KeyValuePair<string, int> PACK_FIRST_TRANSITION_SERIAL_ID = new KeyValuePair<string, int>("PACK_FIRST_TRANSITION_SERIAL_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_PURCHASE_PACK_ID = new KeyValuePair<string, int>("LAST_PURCHASE_PACK_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_PURCHASE_PACK_ID_IN_EXIST_ADDITIONALPACK = new KeyValuePair<string, int>("LAST_PURCHASE_PACK_ID_IN_EXIST_ADDITIONALPACK", -1);

	public static readonly KeyValuePair<string, int> LAST_PURCHASE_DECK_SERIES_ID = new KeyValuePair<string, int>("LAST_PURCHASE_DECK_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> SCENE_TRANSITION_VIEW_DECK_SERIES_ID = new KeyValuePair<string, int>("SCENE_TRANSITION_VIEW_DECK_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> LATEST_DECK_SERIES_ID = new KeyValuePair<string, int>("LATEST_DECK_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_PURCHASE_SKIN_SERIES_ID = new KeyValuePair<string, int>("LAST_PURCHASE_SKIN_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> SCENE_TRANSITION_VIEW_SKIN_SERIES_ID = new KeyValuePair<string, int>("SCENE_TRANSITION_VIEW_SKIN_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> LATEST_SKIN_SERIES_ID = new KeyValuePair<string, int>("LATEST_SKIN_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> LAST_PURCHASE_SLEEVE_SERIES_ID = new KeyValuePair<string, int>("LAST_PURCHASE_SLEEVE_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> SCENE_TRANSITION_VIEW_SLEEVE_SERIES_ID = new KeyValuePair<string, int>("SCENE_TRANSITION_VIEW_SLEEVE_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> LATEST_SLEEVE_SERIES_ID = new KeyValuePair<string, int>("LATEST_SLEEVE_SERIES_ID", -1);

	public static readonly KeyValuePair<string, int> BATTLE_DETAIL_PANEL_SIZE = new KeyValuePair<string, int>("BATTLE_DETAIL_PANEL_SIZE", 0);

	public static readonly KeyValuePair<string, string> LAST_REPORT_DATETIME = new KeyValuePair<string, string>("LAST_REPORT_DATETIME", "01/01/0001 00:00:00");

	public static readonly KeyValuePair<string, int> KEYBOARD_CONTROL = new KeyValuePair<string, int>("KEYBOARD_CONTROL", FALSE);

	public static readonly KeyValuePair<string, int> KEYBOARD_SHORTCUT_EVOLUTION = new KeyValuePair<string, int>("KEYBOARD_SHORTCUT_EVOLUTION", FALSE);

	public static readonly KeyValuePair<string, int> KEYBOARD_SHORTCUT_SPACE = new KeyValuePair<string, int>("KEYBOARD_SHORTCUT_SPACE", FALSE);

	public static readonly KeyValuePair<string, int> IS_COPY_SLEEVE_AND_SKIN = new KeyValuePair<string, int>("IS_COPY_SLEEVE_AND_SKIN", FALSE);

	public static readonly KeyValuePair<string, int> IS_COPY_SUBCLASS_CARDS = new KeyValuePair<string, int>("IS_COPY_SUBCLASS_CARDS", TRUE);

	public static readonly KeyValuePair<string, int> IS_SKIP_CLEARED_STORY_BATTLE = new KeyValuePair<string, int>("IS_SKIP_CLEARED_STORY_BATTLE", FALSE);

	public static readonly KeyValuePair<string, int> BATTLE_WINNER_REWARD_GRADE = new KeyValuePair<string, int>("BATTLE_WINNER_REWARD_GRADE", 0);

	public static readonly KeyValuePair<string, string> BATTLE_WINNER_REWARD_STRING = new KeyValuePair<string, string>("BATTLE_WINNER_REWARD_STRING", "");

	public static readonly KeyValuePair<string, int> IS_SELECT_WSS = new KeyValuePair<string, int>("IS_SELECT_WSS", FALSE);

	public static readonly KeyValuePair<string, int> IS_SELECT_IPV6 = new KeyValuePair<string, int>("IS_SELECT_IPV6", TRUE);

	public static readonly KeyValuePair<string, int> COLOSSEUM_PUBLISHED_SETTING = new KeyValuePair<string, int>("COLOSSEUM_PUBLISHED_SETTING", TRUE);

	public static readonly KeyValuePair<string, int> COMPETITION_PUBLISHED_SETTING = new KeyValuePair<string, int>("COMPETITION_PUBLISHED_SETTING", TRUE);

	public static readonly KeyValuePair<string, int> AUTO_CACHE_CLEAR_FLAG = new KeyValuePair<string, int>("AUTO_CACHE_CLEAR_FLAG", FALSE);

	public static readonly KeyValuePair<string, int> PURCHASE_ALERT = new KeyValuePair<string, int>("PURCHASE_ALERT", TRUE);

	public static readonly KeyValuePair<string, int> JOINING_GUILD_ID = new KeyValuePair<string, int>("JOINING_GUILD_ID", -1);

	public static readonly KeyValuePair<string, int> READ_LATEST_GUILD_CHAT_MESSAGE_ID = new KeyValuePair<string, int>("READ_LATEST_GUILD_CHAT_MESSAGE_ID", -1);

	public static readonly KeyValuePair<string, int> READ_LATEST_GATHERING_CHAT_MESSAGE_ID = new KeyValuePair<string, int>("READ_LATEST_GATHERING_CHAT_MESSAGE_ID", -1);

	public static readonly KeyValuePair<string, int> RANKING_START_TYPE = new KeyValuePair<string, int>("RANKING_START_TYPE", 0);

	public static readonly KeyValuePair<string, int> GATHERING_BATTLE_TYPE = new KeyValuePair<string, int>("GATHERING_BATTLE_TYPE", 1);

	public static readonly KeyValuePair<string, int> GATHERING_BATTLE_TORNAMENT_TYPE = new KeyValuePair<string, int>("GATHERING_BATTLE_TORNAMENT_TYPE", 0);

	public static readonly KeyValuePair<string, int> GATHERING_IS_RESET = new KeyValuePair<string, int>("GATHERING_IS_RESET", 0);

	public static readonly KeyValuePair<string, int> GATHERING_BATTLE_STYLE = new KeyValuePair<string, int>("GATHERING_BATTLE_STYLE", 1);

	public static readonly KeyValuePair<string, int> GATHERING_MAX_MEMBER = new KeyValuePair<string, int>("GATHERING_MAX_MEMBER", 2);

	public static readonly KeyValuePair<string, int> GATHERING_FORMAT = new KeyValuePair<string, int>("GATHERING_FORMAT", 0);

	public static readonly KeyValuePair<string, int> GATHERING_BATTLE_HOUR = new KeyValuePair<string, int>("GATHERING_BATTLE_HOUR", 1);

	public static readonly KeyValuePair<string, int> GATHERING_OWNER_ENTRY_BATTLE = new KeyValuePair<string, int>("GATHERING_OWNER_ENTRY_BATTLE", 1);

	public static readonly KeyValuePair<string, int> GATHERING_IS_ENTRY_DECK_ONLY = new KeyValuePair<string, int>("GATHERING_IS_ENTRY_DECK_ONLY", 1);

	public static readonly KeyValuePair<string, int> GATHERING_WATCH_SETTING = new KeyValuePair<string, int>("GATHERING_WATCH_SETTING", 1);

	public static readonly KeyValuePair<string, int> BATTLE_PASS_SHOW_RESULT = new KeyValuePair<string, int>("BATTLE_PASS_SHOW_RESULT", TRUE);

	public static readonly KeyValuePair<string, int> BATTLE_PASS_LAST_TIPS_LEASON = new KeyValuePair<string, int>("BATTLE_PASS_LAST_TIPS_LEASON", -1);

	public static readonly KeyValuePair<string, int> PREMIUM_PASS_APPEAL_LAST_SEASON = new KeyValuePair<string, int>("PREMIUM_PASS_APPEAL_LAST_SEASON", -1);

	public static readonly KeyValuePair<string, int> PREMIUM_PASS_APPEAL_LAST_LEVEL = new KeyValuePair<string, int>("PREMIUM_PASS_APPEAL_LAST_LEVEL", -1);

	public static readonly KeyValuePair<string, int> COMPETITION_JOIN_BUTTON_LATEST_ID = new KeyValuePair<string, int>("COMPETITION_JOIN_BUTTON_LATEST_ID", -1);

	public static readonly KeyValuePair<string, float> SELF_DISCONNECT_OPEN_STATUS_TO_REPLACE_LOG = new KeyValuePair<string, float>("SELF_DISCONNECT_OPEN_STATUS_TO_REPLACE_LOG", -1f);

	public static readonly KeyValuePair<string, int> ANDROID_ALARM_REMINDER_ENABLE = new KeyValuePair<string, int>("ANDROID_ALARM_REMINDER_ENABLE", 1);

	public static readonly KeyValuePair<string, int> ANDROID_ALARM_RIMINDER_SETTING = new KeyValuePair<string, int>("ANDROID_ALARM_RIMINDER_SETTING", 1);

	public static readonly string STORY_BATTLE_LOSE_COUNT = "STORY_BATTLE_LOSE_COUNT";

	public static readonly KeyValuePair<string, int> SMALL_RESOURCE_STATUS = new KeyValuePair<string, int>("SMALL_RESOURCE_STATUS", 0);

	public static readonly KeyValuePair<string, int> DECK_INTRO_IS_MYROTATION_COPY_EQUAL_PERIOD = new KeyValuePair<string, int>("DECK_INTRO_IS_MYROTATION_COPY_EQUAL_PERIOD", 0);

	public static readonly KeyValuePair<string, int> DECK_INTRO_IS_MYROTATION_COPY_NOT_EQUAL_PERIOD = new KeyValuePair<string, int>("DECK_INTRO_IS_MYROTATION_COPY_NOT_EQUAL_PERIOD", 1);

	public static bool GetBool(KeyValuePair<string, int> id)
	{
		return PlayerPrefs.GetInt(id.Key, id.Value) == TRUE;
	}

	public static bool GetValue(KeyValuePair<string, bool> id)
	{
		return PlayerPrefs.GetInt(id.Key, id.Value ? TRUE : FALSE) == TRUE;
	}

	public static int GetValue(KeyValuePair<string, int> id)
	{
		return PlayerPrefs.GetInt(id.Key, id.Value);
	}

	public static string GetValue(KeyValuePair<string, string> id)
	{
		return PlayerPrefs.GetString(id.Key, id.Value);
	}

	public static void SetBool(KeyValuePair<string, int> id, bool flag)
	{
		PlayerPrefs.SetInt(id.Key, flag ? TRUE : FALSE);
	}

	public static void SetValue(KeyValuePair<string, bool> id, bool flag)
	{
		PlayerPrefs.SetInt(id.Key, flag ? TRUE : FALSE);
	}

	public static void SetValue(KeyValuePair<string, int> id, int value)
	{
		PlayerPrefs.SetInt(id.Key, value);
	}

	public static void SetValue(KeyValuePair<string, string> id, string value)
	{
		PlayerPrefs.SetString(id.Key, value);
	}

	public static void TurnOnFirsStageIfStageIdListAllOff()
	{
		List<int> list = CreateStageOffList();
		for (int i = 0; i < Data.Load.data.OpenBattleFieldIdList.Count; i++)
		{
			if (!list.Contains(int.Parse(Data.Load.data.OpenBattleFieldIdList[i])))
			{
				return;
			}
		}
		bool[] array = new bool[Data.Load.data.OpenBattleFieldIdList.Count];
		array[0] = false;
		for (int j = 1; j < Data.Load.data.OpenBattleFieldIdList.Count; j++)
		{
			array[j] = true;
		}
		SetValue(OFF_STAGE_ID, ConvertStageIdListToSaveData(array));
	}

	public static string ConvertStageIdListToSaveData(bool[] indexs)
	{
		string text = "";
		for (int i = 0; i < Data.Load.data.OpenBattleFieldIdList.Count; i++)
		{
			if (indexs[i])
			{
				text = text + Data.Load.data.OpenBattleFieldIdList[i] + ",";
			}
		}
		return text;
	}

	public static List<int> CreateServerSendStageOffList()
	{
		TurnOnFirsStageIfStageIdListAllOff();
		return CreateStageOffList();
	}

	private static List<int> CreateStageOffList()
	{
		string[] array = GetValue(OFF_STAGE_ID).Split(',');
		List<int> list = new List<int>();
		int result = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out result))
			{
				list.Add(result);
			}
		}
		return list;
	}
}
