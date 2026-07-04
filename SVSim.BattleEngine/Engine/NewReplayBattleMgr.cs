using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.BattleMgr;

// PASS-6 STUB: 1,606-line body dropped. NewReplayBattleMgr is the client-side new-replay
// battle manager (replay playback UI, move-turn buttons, per-frame battle-info panels).
// Nothing constructs a NewReplayBattleMgr in the headless node — the node uses
// HeadlessNetworkBattleMgr : NetworkStandardBattleMgr : NetworkBattleManagerBase, a
// sibling subtree. The three externally-touched surfaces below are all the outside
// world needs:
//   1. Nested types ParameterModifierInfo / BattleInfoData / BattleLogTextureInfo —
//      NetworkBattleReceiver.cs references them as list-element and return types on
//      the headless CreateBattleInfoData / UpdateBattleInfo path (the path itself is
//      dead in headless — `networkBattleMgr as NewReplayBattleMgr` returns null so the
//      Update calls NRE, but they never fire in a real headless message flow).
//      DetailPanelControl.LoadCardHeaderTexture takes a List<BattleLogTextureInfo>
//      parameter (used to belong to the Wizard chain). BattleLogManager (Shim/Generated)
//      also has List<BattleLogTextureInfo> in two method signatures.
//   2. PlayerBattleInfoData / EnemyBattleInfoData — read via `(networkBattleMgr as
//      NewReplayBattleMgr).PlayerBattleInfoData` from NetworkBattleReceiver:774. The
//      cast is null in headless, so the deref NREs — this is the SAME class of NRE
//      as the pass-5 RecoveryOperationCollection pin, but the code path never reaches
//      NetworkBattleReceiver:774 in a real receive flow. Kept as empty lists so any
//      degenerate test path returns safely.
//   3. SetActiveMoveTurnButton — called from MainPhase.cs:98 under `_battleManager is
//      NewReplayBattleMgr` guard. In headless the guard is false so the call never
//      fires; the no-op body is just for the compile-time signature.
public class NewReplayBattleMgr : NetworkReplayBattleMgr
{
    public class ParameterModifierInfo
    {
        public NetworkBattleReceiver.ReplayParameterModifierType ModifierType;
        public int ModifierValue;

        public ParameterModifierInfo(NetworkBattleReceiver.ReplayParameterModifierType modifierType, int modifierValue)
        {
            ModifierType = modifierType;
            ModifierValue = modifierValue;
        }
    }

    public class BattleInfoData
    {
    }

    public class BattleLogTextureInfo
    {
        public string TexturePath { get; private set; }
        public string LogHeaderAssetPath { get; private set; }
        public UITexture HeaderUITexture { get; private set; }
        public Action<Texture> OnLoad { get; private set; }

        public BattleLogTextureInfo(string texturePath, string logHeaderAssetPath, UITexture headerUITexture, Action<Texture> onLoad)
        {
            TexturePath = texturePath;
            LogHeaderAssetPath = logHeaderAssetPath;
            HeaderUITexture = headerUITexture;
            OnLoad = onLoad;
        }
    }

    public NewReplayBattleMgr(IBattleMgrContentsCreator contentsCreator) : base(contentsCreator) { }

    public void SetActiveMoveTurnButton(bool isActive) { }
}
