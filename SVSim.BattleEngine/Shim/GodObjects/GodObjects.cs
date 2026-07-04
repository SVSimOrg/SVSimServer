// AUTHORED SHIM (not copied). The god-object singletons the engine reaches for
// presentation/scene/effects/sound/data. These are the M0 "stop the bleed" types:
// copying them re-explodes the closure into the whole app (audio, scene, UI, net),
// so we shim a minimal surface. Manager GETTERS return the (copied) manager types as
// null fields -- the engine only dereferences them inside never-run VFX (IsForecast
// suppresses VFX) or non-battle code paths, so a null is harmless headless and avoids
// constructing copied types with heavy ctors. Member signatures mirror the decomp
// exactly (extracted, not guessed) so call sites compile unchanged.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMgr
{
    public enum EffectType
    {
        NONE, CMN_CARD_DRAW_2, CMN_CARD_SET_1, CMN_CARD_SET_2, CMN_CARD_SET_3, CMN_CARD_ACCELERATE_1, CMN_CARD_CRYSTALLIZE_1, CMN_CARD_TARGET_1, CMN_CARD_TARGET_2, CMN_CARD_SELECT_3, CMN_UI_TURN_1, CMN_UI_TURN_5, CMN_UI_TURN_6,     }
    public enum MoveType
    {
        NONE, SKIP, DIRECT,         DIRECT_HAND, DIRECT_DECK, DIRECT_LEADER, CENTER    }
    public enum TargetType { NONE, NONE_WAIT, SINGLE, SINGLE_ONLY_OPPONENT, AREA_ALL, AREA_OPPONENT, AREA_SELF }
    public enum EngineType { NONE, SHURIKEN}

    public Effect Start(EffectType type, Vector3 pos, Quaternion rot, int layer = -1) => null;
    public Effect Start(EffectType type, Vector3 pos, GameObject obj = null) => null;
    public Effect Stop(EffectType type) => null;
    public void ImmediateDestroyBattleEffectContainer() { }
    public void ClearBattleFeildEffect() { }
    public void RestUnneededEffect() { }
    public EffectBattle GetEffectBattle(string key) => null;
    public EffectBattle GetEnemyEffectBattle(string key) => null;
    public static MoveType ToStrMoveType(string str) => MoveType.NONE;
    public static EngineType ToStrEngineType(string str) => EngineType.NONE;
    public static TargetType ToStrTargetType(string str) => TargetType.NONE;
}

public partial class GameObjMgr { public GameObjMgr() { } }

public class GameMgr
{

    public GameObject m_GameManagerObj;
    public float ScreenAspect = 1.777f;

    // Six mode flags collapsed to const-false getters in Phase 4 (2026-07-02): headless
    // is neither watching, replaying, admin-watching, an AI-network client, a puzzle
    // quest, nor a "new replay" — every branch guarded on `!IsWatchBattle` etc. was a
    // tautology, and every branch guarded on the positive was dead code. Setters
    // dropped: pre-Phase-4 callers wrote `false` at end-of-battle cleanup which is now
    // unnecessary.
    public bool IsAdminWatch => false;
    public bool IsWatchBattle => false;
    public bool IsReplayBattle => false;
    public bool IsAINetwork => false;
    public bool IsPuzzleQuest => false;
    public bool IsNewReplayBattle => false;
    public bool IsAdmin => false;
    // IsNetworkBattle stays writable — headless PvP battles set true; SingleBattleMgr paths stay false.
    public bool IsNetworkBattle { get; set; }

    private EffectMgr _effect;
    private DataMgr _data;
    private GameObjMgr _gameObj = new GameObjMgr();
    private PrefabMgr _prefab;
    private InputMgr _input;
    private NetworkUserInfoData _netUser;

    public EffectMgr GetEffectMgr() => _effect ??= new EffectMgr();
    // Headless: hand back non-null no-op instances. The copied manager types are pure
    // data/dictionary/no-op holders (no Unity in their ctors); the resolution-path ctor
    // dereferences these immediately (CreateBackgroundId / CreateManager / UnityEventAgent wiring).
    public DataMgr GetDataMgr() => _data ??= new DataMgr();
    public GameObjMgr GetGameObjMgr() => _gameObj;
    public PrefabMgr GetPrefabMgr() => _prefab ??= new PrefabMgr();
    public InputMgr GetInputMgr() => _input ??= new InputMgr();
    public NetworkUserInfoData GetNetworkUserInfoData() => _netUser;
    public void SetNetworkUserInfoData(NetworkUserInfoData infoData) => _netUser = infoData;
    public bool IsUseUnapprovedList(bool isPlayer) => false;
}

// UIManager surface (members, ViewScene enum, ChangeViewSceneParam) is generated from
// decomp into Shim/Generated/UIManager*.g.cs. This partial keeps only the singleton +
// MonoBehaviour base (gameObject/transform/StartCoroutine/StopCoroutine come from the base).
public partial class UIManager : UnityEngine.MonoBehaviour
{
    private static UIManager _ins;
    public static UIManager GetInstance() => _ins ??= new UIManager();
}

// VideoHostingHUD (members + HUDMode enum) provided by Generated/VideoHostingHUD.g.cs

namespace Wizard
{
    public partial class DeckUpdateTask { }
}
