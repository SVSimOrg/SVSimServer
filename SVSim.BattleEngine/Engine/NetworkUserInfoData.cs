using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;

public class NetworkUserInfoData
{
	public class NetworkUserInfo
	{
		public int Rank { get; private set; }

		public bool IsMasterRank { get; private set; }

		public int BattlePoint { get; private set; }

		public int MasterPoint { get; private set; }

		public int ClassId { get; private set; }

		public int SubClassId { get; private set; } = 10;

		public int CharaId { get; private set; }

		public string MyRotationId { get; private set; } = "";

		public string AvatarBattleId { get; private set; } = "";

		public void SetParameter(Dictionary<string, object> info)
		{
			Rank = Convert.ToInt32(info["rank"]);
			IsMasterRank = info.ContainsKey("isMasterRank") && Convert.ToInt32(info["isMasterRank"]) != 0;
			BattlePoint = (info.ContainsKey("battlePoint") ? Convert.ToInt32(info["battlePoint"]) : 0);
			MasterPoint = (info.ContainsKey("masterPoint") ? Convert.ToInt32(info["masterPoint"]) : 0);
			ClassId = Convert.ToInt32(info["classId"]);
			if (info.ContainsKey("subclassId"))
			{
				SubClassId = Convert.ToInt32(info["subclassId"]);
			}
			CharaId = Convert.ToInt32(info["charaId"]);
			if (info.ContainsKey("rotationId"))
			{
				MyRotationId = Convert.ToString(info["rotationId"]);
			}
			AvatarBattleId = info["charaId"].ToString();
		}
	}

	private Dictionary<string, object> _selfInfo = new Dictionary<string, object>();

	private Dictionary<string, object> _oppoInfo = new Dictionary<string, object>();

	public int TurnState { get; set; }

	public NetworkUserInfo SelfBattleStartInfo { get; private set; }

	public NetworkUserInfo OppoBattleStartInfo { get; private set; }

	public NetworkUserInfoData()
	{
		TurnState = -1;
	}

	public void SetSelfInfo(Dictionary<string, object> info, bool isWatchReplayRecovery)
	{
		_selfInfo = info;
		if (isWatchReplayRecovery)
		{
			SetNetworkSelfInfo(info);
		}
		if (_selfInfo != null && _selfInfo.ContainsKey("seed"))
		{
			LocalLog.AccumulateLastTraceLog("SetSelfInfo seed" + Convert.ToInt32(_selfInfo["seed"]));
		}
	}

	// SetNetworkSelfInfo used to fan the received chara/skin/rotation info out to the
	// process-wide GameMgr.DataMgr (production-only path). In the current headless world
	// the only call site is the isWatchReplayRecovery=true branch of SetSelfInfo, which
	// is never taken in either test seeder — so this is dead in every code path we run.
	// Body preserved as documentation until a live-network path revives it, but the
	// GameMgr/Data reach has been dropped so it can't ambient-race anymore.
	public void SetNetworkSelfInfo(Dictionary<string, object> info)
	{
		if (SelfBattleStartInfo == null)
		{
			SelfBattleStartInfo = new NetworkUserInfo();
		}
		SelfBattleStartInfo.SetParameter(info);
		// TODO(post-Phase-5b, revive-live-network): thread a mgr param through and fan out
		// GetSelfCharaId/GetSelfSubClassId/GetSelfMyRotationId/GetSelfAvatarBattleId into
		// mgr.GameMgr.GetDataMgr() + Data.RoomTwoPickBeforeBattleInfo once a caller exists.
		if (_selfInfo != null && _selfInfo.ContainsKey("seed"))
		{
			LocalLog.AccumulateLastTraceLog("SetNetworkSelfInfo seed" + Convert.ToInt32(_selfInfo["seed"]));
		}
	}

	public int GetFieldId()
	{
		return Convert.ToInt32(_selfInfo["fieldId"]);
	}

	public int GetRandomSeed()
	{
		if (_selfInfo == null || !_selfInfo.ContainsKey("seed"))
		{
			string text = "NotSeed ";
			text = text + ((_selfInfo == null) ? "infoNull" : "noneKey") + " ";
			if (_selfInfo != null)
			{
				foreach (KeyValuePair<string, object> item in _selfInfo)
				{
					text = text + item.Key + ":" + item.Value?.ToString() + " ";
				}
			}
			LocalLog.AccumulateLastTraceLog(text);
			return 0;
		}
		return Convert.ToInt32(_selfInfo["seed"]);
	}

	public int GetSelfViewerId()
	{
		return Convert.ToInt32(_selfInfo["viewerId"]);
	}

	public string GetSelfAvatarBattleId()
	{
		if (SelfBattleStartInfo == null)
		{
			return "";
		}
		return SelfBattleStartInfo.AvatarBattleId;
	}

	public int GetSelfChaosId()
	{
		// The `!IsNetworkBattle` guard is redundant with the dict-lookup below: in single-player
		// paths `_selfInfo` is empty (no network setup), so the dict never contains "chaosId"
		// and this returns -1 anyway. Removing the ambient reach.
		if (_selfInfo.ContainsKey("chaosId"))
		{
			return Convert.ToInt32(_selfInfo["chaosId"]);
		}
		return -1;
	}

	public string GetOpponentAvatarBattleId()
	{
		if (SelfBattleStartInfo == null)
		{
			return "";
		}
		return OppoBattleStartInfo.AvatarBattleId;
	}

	public int GetOpponentChaosId()
	{
		// See GetSelfChaosId — same rationale, the `!IsNetworkBattle` guard is redundant with
		// the dict-lookup fallthrough (single-player paths never populate `_oppoInfo`).
		if (_oppoInfo.ContainsKey("chaosId"))
		{
			return Convert.ToInt32(_oppoInfo["chaosId"]);
		}
		return -1;
	}
}
