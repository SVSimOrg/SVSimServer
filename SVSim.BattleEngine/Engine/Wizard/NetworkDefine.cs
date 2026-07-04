using System.Collections.Generic;

namespace Wizard;

public static class NetworkDefine
{
	public enum MAINTENANCE_TYPE
	{
		PROFILE_MAINTENANCE = 2001,
		GIFT_MAINTENANCE = 2003,
		SHOP_CARDPACK_MAINTENANCE = 2007,
		SHOP_BUILDDECK_MAINTENANCE = 2032,
		SHOP_SLEEVE_MAINTENANCE = 2008,
		SHOP_SKIN_MAINTENANCE = 2024,
		SHOP_ITEM_MAINTENANCE = 2037,
		MISSION_MAINTENANCE = 2010,
		STORY_MAINTENANCE = 2015,
		CARD_MAINTENANCE = 2018,
		DECK_MAINTENANCE = 2019,
		COLOSSEUM = 2050,
		ARENA_SEALED_BATTLE_MAINTENANCE = 2056,
		BATTLE_PASS = 2070,
		AUTO_DECK_CREATE = 2080,
		BOSS_RUSH = 2096,
		REPLAY_ALL = 2034,
		NEWREPLAY_ALL = 2097,
		NEWREPLAY_EXCLUDE_ROTATION = 2098,
		NEWREPLAY_RECORD = 2099}

	public enum ServerBattleType
	{
		Practice = 1,
		Story = 2,
		Free = 3,
		Rank = 4,
		OpenRoom = 6,
		TwoPick = 11,
		RoomTwoPick = 13,
		Colosseum = 21,
		ColosseumTwoPick = 22,
		Quest = 37,
		Sealed = 32,
		Gathering = 34,
		OfflineEvent = 40,
		Competition = 42,
		BossRushQuest = 45,
		SecretBossQuest = 46,
		CompetitionTwoPick = 47
	}
}
