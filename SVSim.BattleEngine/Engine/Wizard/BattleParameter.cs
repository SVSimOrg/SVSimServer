using LitJson;
using Wizard.RoomMatch;

namespace Wizard;

public class BattleParameter
{
	public NetworkDefine.ServerBattleType BattleType { get; set; }

	public Format DeckFormat { get; set; }

	public RoomConnectController.BattleRule Rule { get; set; }

	public TwoPickFormat TwoPickFormat { get; set; }

	public bool IsOpenDeckRoom { get; private set; }

	public BattleParameter(NetworkDefine.ServerBattleType battleType, Format format, TwoPickFormat deckFormatType, RoomConnectController.BattleRule rule, bool isOpenDeckRoom)
	{
		BattleType = battleType;
		DeckFormat = format;
		TwoPickFormat = deckFormatType;
		Rule = rule;
		IsOpenDeckRoom = isOpenDeckRoom;
	}

	public DataMgr.BattleType ConvertClientBattleType()
	{
		DataMgr.BattleType result = DataMgr.BattleType.None;
		switch (BattleType)
		{
		case NetworkDefine.ServerBattleType.Practice:
			result = DataMgr.BattleType.Practice;
			break;
		case NetworkDefine.ServerBattleType.Story:
			result = DataMgr.BattleType.Story;
			break;
		case NetworkDefine.ServerBattleType.Free:
			result = DataMgr.BattleType.FreeBattle;
			break;
		case NetworkDefine.ServerBattleType.Rank:
			result = DataMgr.BattleType.RankBattle;
			break;
		case NetworkDefine.ServerBattleType.OpenRoom:
			result = DataMgr.BattleType.RoomBattle;
			break;
		case NetworkDefine.ServerBattleType.TwoPick:
			result = DataMgr.BattleType.TwoPick;
			break;
		case NetworkDefine.ServerBattleType.RoomTwoPick:
			switch (TwoPickFormat)
			{
			case TwoPickFormat.Normal:
				result = DataMgr.BattleType.RoomTwoPick;
				break;
			case TwoPickFormat.Backdraft:
			case TwoPickFormat.BackdraftCube:
			case TwoPickFormat.BackdraftChaos:
				result = DataMgr.BattleType.TwoPickBackdraft;
				break;
			case TwoPickFormat.Cube:
				result = DataMgr.BattleType.RoomTwoPick;
				break;
			case TwoPickFormat.Chaos:
				result = DataMgr.BattleType.RoomTwoPick;
				break;
			}
			break;
		case NetworkDefine.ServerBattleType.Quest:
			result = DataMgr.BattleType.Quest;
			break;
		case NetworkDefine.ServerBattleType.BossRushQuest:
			result = DataMgr.BattleType.BossRushQuest;
			break;
		case NetworkDefine.ServerBattleType.SecretBossQuest:
			result = DataMgr.BattleType.SecretBossQuest;
			break;
		case NetworkDefine.ServerBattleType.Colosseum:
			result = ((DeckFormat != Format.Hof) ? ((DeckFormat != Format.Windfall) ? DataMgr.BattleType.ColosseumNormal : DataMgr.BattleType.ColosseumWindFall) : DataMgr.BattleType.ColosseumHof);
			break;
		case NetworkDefine.ServerBattleType.ColosseumTwoPick:
			result = DataMgr.BattleType.ColosseumTwoPick;
			break;
		case NetworkDefine.ServerBattleType.Competition:
			result = DataMgr.BattleType.CompetitionNormal;
			break;
		case NetworkDefine.ServerBattleType.CompetitionTwoPick:
			result = DataMgr.BattleType.CompetitionTwoPick;
			break;
		case NetworkDefine.ServerBattleType.Sealed:
			result = DataMgr.BattleType.Sealed;
			break;
		case NetworkDefine.ServerBattleType.Gathering:
			result = DataMgr.BattleType.Gathering;
			break;
		case NetworkDefine.ServerBattleType.OfflineEvent:
			result = DataMgr.BattleType.OfflineEvent;
			break;
		}
		return result;
	}

	public static BattleParameter JsonToBattleParameter(JsonData json)
	{
		TwoPickFormat deckFormatType = TwoPickFormat.None;
		RoomConnectController.BattleRule rule = RoomConnectController.BattleRule.None;
		bool isOpenDeckRoom = false;
		if (json.Keys.Contains("battle_type"))
		{
			NetworkDefine.ServerBattleType battleType = (NetworkDefine.ServerBattleType)json["battle_type"].ToInt();
			if (json.Keys.Contains("deck_format"))
			{
				Format format = Data.ParseApiFormat(json["deck_format"].ToInt());
				if (json.Keys.Contains("two_pick_type"))
				{
					deckFormatType = (TwoPickFormat)json["two_pick_type"].ToInt();
				}
				if (json.Keys.Contains("battle_rule"))
				{
					rule = (RoomConnectController.BattleRule)json["battle_rule"].ToInt();
				}
				if (json.TryGetValue("is_deck_confirmable", out var value))
				{
					isOpenDeckRoom = value.ToInt() == 1;
				}
				return new BattleParameter(battleType, format, deckFormatType, rule, isOpenDeckRoom);
			}
			return null;
		}
		return null;
	}
}
