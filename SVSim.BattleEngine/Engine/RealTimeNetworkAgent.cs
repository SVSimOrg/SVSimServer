using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

// PASS-7 STUB: 1,495-line body dropped. RealTimeNetworkAgent is the client-side
// Socket.IO transport singleton — connect / emit / handle-receive / disconnect / recovery-
// record / gungnir-heartbeat. In the headless node the same wire moves through
// SessionBattleEngine and SVSim.BattleNode; ToolboxGame.RealTimeNetworkAgent is
// ambient-backed and returns null when no scope has attached a real agent (which
// is always for headless). The type still has to exist because callers hold field
// references (`NetworkStandardBattleMgr._agent` etc.) and use static methods
// (`RealTimeNetworkAgent.IsFromResumeData`, `IsNormalNetworkBattle`).
//
// This stub keeps the ~30 members callers touch, all as no-ops / default returns.
// The MonoBehaviour base + enums stay so the type surface stays wire-compatible.
// Any actual RTA invocation in headless would be a call on a null instance —
// which is the CURRENT behavior in production; the null-guards / `?.` patterns
// in call sites already handle it.
public class RealTimeNetworkAgent : MonoBehaviour
{
    public enum EmitCategory
    {
}

    public enum MatchingStatus
    {
        Loaded = 40,
        Prepared = 50    }

    public enum DESTROY_OBJECT_LOG
    {
        WatchMaintenance    }

    public static FinishTaskBase FinishTaskBase;
    public Action<NetworkBattleDefine.NetworkBattleURI> OnEmit;
    public Action<Dictionary<string, object>> OnAck;
    public NetworkStatus OpponentNetworkStatus { get; protected set; }
    public NetworkStatus PlayerNetworkStatus { get; private set; }
    public bool IsReceiveSelfDisconnect => false;
    public int LastEmitSeqNumber => 0;
    public INetworkLogger<NetworkLog> NetworkLogger { get; protected set; }

    public int GetIsFirstPlayer() => 0;
    public void SetCurrentMatchingStatus(MatchingStatus status) { }
    public void SetNetworkBattleMgr(NetworkBattleManagerBase mgr) { }

    public static bool IsFromResumeData(Dictionary<string, object> data) => false;
    public static void ReconnectSocketAndLogFlagOn() { }

    public object ReconnectSocket() => null;
    public bool IsOpen() => false;
    // The URI overload fires OnEmit — the O1 liveness signal that the test-side
    // HeadlessEngineEnv (M13 EmitPathReadOracle) subscribes to. Preserving this call
    // was the entire reason RTA still exists as a stub rather than being deleted.
    public void EmitMsgPack(NetworkBattleDefine.NetworkBattleURI uri, Dictionary<string, object> info = null, Action onFinishedSend = null, bool isGetableAck = true, int fixedSeqNumber = -1, bool isStockData = true, bool isNotActionSeq = false)
    {
        OnEmit?.Invoke(uri);
    }
    public void EmitHandData(List<object> parameters, NetworkBattleSender.HAND_URI_TYPE uri, bool isSkipDuplicateCheck = false) { }

    public bool GetTurnState() => false;
    public void FinishBattleTask() { }
    public void CallMaintenanceError() { }
    public virtual void StopNetworkBattle(bool isNotCloseWindow = false) { }
    public void DestroyObj(DESTROY_OBJECT_LOG log) { }
    public void ResetDisconnectLogNum() { }
    public int GetTurnSequence() => 0;
}
