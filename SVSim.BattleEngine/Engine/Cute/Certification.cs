using System;
using System.Collections;
using System.Text;
using UnityEngine;
using Wizard;
using Wizard.Title;
// TODO(engine-cleanup-pass2): 43 of 45 methods unrun in baseline
//   Type: Cute.Certification
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Cute;

public class Certification : MonoBehaviour
{

	private static string udid;

	private static int short_udid;

	private static string sessionId;

	public static string Udid
	{
		get
		{
			if (string.IsNullOrEmpty(udid))
			{
				udid = Cryptographer.decode(Toolbox.SavedataManager.GetString("UDID"));
			}
			return udid;
		}
	}

	public static int ViewerId
	{
		// Instance-backed via BattleManagerBase.InstanceViewerId (Phase 5, chunk 40 — ambient
		// fallback dropped; all consumers routed through mgr in chunks 38-39). Default 1001 matches
		// EngineGlobalInit.ThisViewerId. Setter no-op preserved from Phase-4 semantics.
		get => BattleManagerBase.GetIns()?.InstanceViewerId ?? 1001;
		set { /* no-op; SavedataManager path dead headless */ }
	}

	public static int ShortUdid
	{
		get
		{
			if (short_udid == 0)
			{
				short_udid = Toolbox.SavedataManager.GetInt("SHORT_UDID");
			}
			return short_udid;
		}
		set
		{
			Toolbox.SavedataManager.SetInt("SHORT_UDID", value);
			short_udid = value;
		}
	}

	public static string SessionId
	{
		get
		{
			if (string.IsNullOrEmpty(sessionId))
			{
				sessionId = ViewerId + Udid;
			}
			return Cryptographer.MakeMd5(sessionId);
		}
		set
		{
			sessionId = value;
		}
	}

	public static ulong SteamID { get; private set; }

	public static string SteamSessionTicket { get; private set; }

	public static string GetKeyChainViewerId()
	{
		return "";
	}

	public static string GetIDFA()
	{
		return "";
	}
}
