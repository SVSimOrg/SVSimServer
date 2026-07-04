using System;
using LitJson;

namespace Wizard;

public class BattleRecoveryInfo : ReplayDetailInfo
{
	public int turn_state;

	public string node_server_url = "";

	public int Play_seq;

	public bool is_owner;

	public bool _isConventionRoom;

	public bool is_opponent_master_rank;

	public bool IsReceivedDeal;

	public bool IsMulliganEnd;

	public string roomId;

	public DataMgr.BattleType BattleType { get; private set; } = DataMgr.BattleType.None;

	public int Pub_seq { get; private set; }

	public BattleParameter BattleParameterInstance { get; private set; }

	public int OpponentHighFormatRank { get; private set; }

	public bool IsEnableGuildInviteButton { get; private set; }

	public ConventionInfo ConventionInfo { get; private set; }

	public bool IsGatheringRoom { get; private set; }

	public bool CanUseNonPossessionCard { get; private set; }

	public BattleRecoveryInfo(JsonData data)
		: base(data)
	{
		BattleParameterInstance = BattleParameter.JsonToBattleParameter(data);
		BattleType = BattleParameterInstance.ConvertClientBattleType();
		if (BattleType == DataMgr.BattleType.Gathering)
		{
			IsGatheringRoom = true;
			BattleType = DataMgr.BattleType.RoomBattle;
		}
		if (BattleType == DataMgr.BattleType.OfflineEvent)
		{
			_isConventionRoom = true;
			BattleType = DataMgr.BattleType.RoomBattle;
		}
		Data.CurrentFormat = BattleParameterInstance.DeckFormat;
		/* Pre-Phase-5b: DataMgr.TwoPickFormat write dropped */
		node_server_url = data["node_server_url"].ToString();
		turn_state = data["turn_state"].ToInt();
		Pub_seq = data["pubSeq1"].ToInt();
		Play_seq = data["playSeq1"].ToInt();
		if (data.Keys.Contains("isOwner"))
		{
			is_owner = data["isOwner"].ToInt() == 1;
		}
		if (data.Keys.Contains("is_colosseum_rank_battle"))
		{
			Data.ArenaData.ColosseumData.IsRankMatching = true;
		}
		if (data.Keys.Contains("is_competition_rank_battle"))
		{
			Data.ArenaData.CompetitionData.IsRankMatching = true;
		}
		if (data.Keys.Contains("maxRank2"))
		{
			OpponentHighFormatRank = data["maxRank2"].ToInt();
		}
		if (data.Keys.Contains("roomId"))
		{
			roomId = data["roomId"].ToString();
		}
		if (data.Keys.Contains("tournament"))
		{
			ConventionInfo = new ConventionInfo(data["tournament"]);
		}
		if (data.Keys.Contains("is_invitation_user"))
		{
			IsEnableGuildInviteButton = data["is_invitation_user"].ToBoolean();
		}
		is_opponent_master_rank = data["isMasterRank2"].ToInt() == 1;
		CanUseNonPossessionCard = data.GetValueOrDefault("is_enabled_all_card", defaultValue: false);
	}

	protected override void SetOpponentDeck(JsonData data)
	{
	}

	protected override void CheckMulliganFlags(JsonData data)
	{
		if (IsMulliganEnd && IsReceivedDeal)
		{
			return;
		}
		foreach (string key in data.Keys)
		{
			if (!(key == "uri"))
			{
				continue;
			}
			JsonData jsonData = data[key];
			if (Enum.IsDefined(typeof(NetworkBattleDefine.NetworkBattleURI), jsonData.ToString()))
			{
				switch ((NetworkBattleDefine.NetworkBattleURI)Enum.Parse(typeof(NetworkBattleDefine.NetworkBattleURI), jsonData.ToString()))
				{
				case NetworkBattleDefine.NetworkBattleURI.Deal:
					IsReceivedDeal = true;
					break;
				case NetworkBattleDefine.NetworkBattleURI.Ready:
					IsMulliganEnd = true;
					break;
				}
			}
		}
	}
}
