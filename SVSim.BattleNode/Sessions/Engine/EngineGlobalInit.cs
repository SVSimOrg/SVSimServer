extern alias engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using BattleManagerBase = engine::BattleManagerBase;
using BattleRecoveryInfo = engine::Wizard.BattleRecoveryInfo;
using CardCSVData = engine::Wizard.CardCSVData;
using CardMaster = engine::Wizard.CardMaster;
using Certification = engine::Cute.Certification;
using ClassCharacterMasterData = engine::Wizard.ClassCharacterMasterData;
using Crossover = engine::Wizard.Crossover;
using Data = engine::Wizard.Data;
using GameMgr = engine::GameMgr;
using Load = engine::Load;
using LoadDetail = engine::LoadDetail;
using Master = engine::Wizard.Master;
using NetworkUserInfoData = engine::NetworkUserInfoData;

namespace SVSim.BattleNode.Sessions.Engine;

/// <summary>Host-owned, process-once initializer for the engine's global statics (Phase 2 N2,
/// carried-risk A). The decompiled engine assumes a set of process-globals exist that the client
/// populates from /load/index at login: the static <c>CardMaster</c>, <c>Wizard.Data</c>
/// (Load/Master/Crossover), the <c>GameMgr</c> DataMgr chara ids, a <c>NetworkUserInfoData</c>, and
/// <c>Cute.Certification.udid</c>. Without them <see cref="SessionBattleEngine.Setup"/> throws inside
/// its try/catch and the shadow silently no-ops (the N1 carried risk). Calling
/// <see cref="EnsureInitialized"/> once at host startup primes them so Setup succeeds.
///
/// This is the production analogue of the test fixtures <c>HeadlessEngineEnv.EnsureInitialized</c> +
/// <c>HeadlessCardMaster</c> + <c>HeadlessMasterData</c>; the reflection seams are transcribed verbatim.
/// It differs in exactly three ways: (1) it loads the FULL cards.json (every row, no id filter) since
/// the live host serves arbitrary decks; (2) it installs ALL 8 classes in ClassCharacterList; and
/// (3) the call is idempotent (process-once via <c>_done</c>). Coexistence: it does not OVERWRITE
/// harmless globals another initializer set (<c>Data.Load</c>, <c>Crossover</c>, leader chara ids,
/// netUser, udid are all set-only-if-absent), but it ALWAYS guarantees the postcondition — the FULL
/// card master and the all-8-class Master — by rebuilding/re-injecting both unconditionally rather
/// than deferring to a possibly-thinner existing global (e.g. a test harness's single-card master).</summary>
internal static class EngineGlobalInit
{
    private static readonly object _gate = new();
    private static bool _done;

    private static readonly string CardsJsonPath =
        Path.Combine(AppContext.BaseDirectory, "Data", "cards.json");

    // chara ids -> a ClassCharacterMasterData in Master; mirrors HeadlessMasterData.
    private const int PlayerCharaId = 1;
    private const int EnemyCharaId = 2;

    /// <summary>The headless engine's "self" viewer id (seeded into <c>Certification.viewer_id</c>). Any
    /// stable nonzero value works; it only has to be DISTINCT from the vid an attack/evolve stamps for a
    /// target on the OTHER seat so the IsRecovery target parse resolves owners correctly. Exposed for the
    /// node-native harness to build attack frames whose target vid matches this perspective.</summary>
    internal const int ThisViewerId = 1001;

    public static void EnsureInitialized()
    {
        // Phase-5 chunk 46: per-session GameMgr wiring moved to SessionBattleEngine.SetupInternal
        // where the session's own GameMgr is in scope. EnsureInitialized only handles the true
        // process-globals below.

        if (_done) return;
        lock (_gate)
        {
            if (_done) return;

            // --- Wizard.Data globals (the static /load/index snapshot) -----------------------------
            // The mgr ctor's CreateBackgroundId reads Data.Load.data._userTutorial (LoadDetail
            // self-inits _userTutorial). ??= so we don't clobber a snapshot HeadlessEngineEnv set.
            Data.Load ??= new Load { data = new LoadDetail() };

            // CardParameter(CardCSVData) reads Data.Crossover.RestrictedCard for the deck-limit calc;
            // an empty Crossover returns the default count (no restriction). Private setter -> reflect.
            // Only set when null so we coexist with HeadlessEngineEnv.
            if (Data.Crossover == null)
            {
                typeof(Data).GetProperty("Crossover",
                        BindingFlags.Static | BindingFlags.Public)!
                    .SetValue(null, new Crossover());
            }

            // Phase-5 chunk 43: was `BattleManagerBase.IsForecast = true` — a static ambient-bridge
            // write that ran before any mgr was constructed, so it stashed in ambient.IsForecast for
            // pre-Phase-5 propagation onto the mgr's InstanceIsForecast at attach. With chunk 42's
            // ambient rip that propagation is gone and the write became a silent no-op. Init here is
            // now redundant: BattleManagerBase.InstanceIsForecast = true by default.

            // BattleRecoveryInfo lives on the mgr instance itself (InstanceRecoveryInfo). Historical
            // process-global write here was dead the moment the mgr's per-instance slot landed
            // (Task 7 / Phase-5a).

            // --- static CardMaster (full cards.json) ----------------------------------------------
            // ALWAYS rebuild + re-inject the FULL master. We must not defer to a possibly-thin
            // existing Default (e.g. a HeadlessCardMaster.Load(singleCard) from an earlier test in
            // the same NUnit process): the postcondition is the COMPLETE card master, and a thin
            // master makes Setup→SeedDeck NRE in SkillCreator.CreateBuildInfo. EnsureInitialized is
            // process-once via _done, so an unconditional load runs at most once — the skip-
            // optimization bought nothing and traded correctness for a coexistence hazard.
            // LoadFullCardMaster rebuilds + re-injects Default, so it's idempotent.
            LoadFullCardMaster();

            // --- Master reference data (all 8 classes' chara list) ---------------------------------
            // ALWAYS install all 8 classes. Don't defer to a prior (e.g. 2-class) harness master:
            // the postcondition is all-8-class ClassCharacterList. Idempotent (replaces Data.Master).
            InstallMaster();

            // --- Cute.Certification.udid -----------------------------------------------------------
            // The emit-path payload builder reads Certification.Udid, whose getter lazily decodes from
            // Toolbox.SavedataManager (null headless). Seed the private static backing field with a
            // non-empty placeholder (opaque — only echoed, never matched) so the getter short-
            // circuits. Matches the harness value. Only set when empty (coexistence).
            var udidField = typeof(Certification).GetField("udid",
                BindingFlags.Static | BindingFlags.NonPublic)!;
            if (string.IsNullOrEmpty(udidField.GetValue(null) as string))
                udidField.SetValue(null, "headless-udid");

            // --- Cute.Certification.viewer_id ------------------------------------------------------
            // Viewer id lives on the mgr instance (InstanceViewerId, default 1001 = ThisViewerId).
            // Certification.ViewerId's static getter reads the mgr via GetIns()?.InstanceViewerId
            // — no seeding needed here.

            _done = true;
        }
    }

    // Per-session GameMgr wiring: seeds DataMgr chara ids + NetworkUserInfoData on the session's own
    // GameMgr. Caller (SessionBattleEngine.SetupInternal) passes _gameMgr BEFORE mgr construction so
    // the base ctor's BattlePlayer/DataMgr reads resolve. Idempotent (SetFieldIfZeroOrNull no-ops once
    // set; netUser is null-guarded).
    public static void WirePerSessionGameMgr(GameMgr gm)
    {
        var dm = gm.GetDataMgr();
        SetFieldIfZeroOrNull(dm, "_playerCharaId", PlayerCharaId);
        SetFieldIfZeroOrNull(dm, "_enemyCharaId", EnemyCharaId);

        if (gm.GetNetworkUserInfoData() == null)
        {
            var netUser = new NetworkUserInfoData();
            netUser.SetSelfInfo(
                new Dictionary<string, object> { ["fieldId"] = 1 },
                isWatchReplayRecovery: false);
            gm.SetNetworkUserInfoData(netUser);
        }
    }

    // --- CardMaster (full load) ----------------------------------------------------------------------

    // Production difference (1): enumerate EVERY card row — no want.Contains(id) filter.
    private static void LoadFullCardMaster()
    {
        var rows = new List<CardCSVData>();
        using (var doc = JsonDocument.Parse(File.ReadAllText(CardsJsonPath)))
        {
            int sort = 0;
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (!el.TryGetProperty("card_id", out var idEl)) continue;
                if (!int.TryParse(idEl.GetString(), out _)) continue; // skip malformed ids
                rows.Add(BuildCardCsvData(el, sort++));
            }
        }

        var cm = NewCardMaster(rows);
        InjectAsDefault(cm);
    }

    // Transcribed from HeadlessCardMaster.BuildCardCsvData.
    private static CardCSVData BuildCardCsvData(JsonElement el, int sortIndex)
    {
        var c = (CardCSVData)FormatterServices.GetUninitializedObject(typeof(CardCSVData));
        const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        foreach (var prop in el.EnumerateObject())
        {
            string val = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.ToString();
            var f = typeof(CardCSVData).GetField(prop.Name, bf);
            if (f != null) { SetMember(f.FieldType, val, v => f.SetValue(c, v)); continue; }
            var p = typeof(CardCSVData).GetProperty(prop.Name, bf);
            if (p != null && p.CanWrite) SetMember(p.PropertyType, val, v => p.SetValue(c, v));
        }
        var si = typeof(CardCSVData).GetProperty("SortIndex", bf);
        if (si != null && si.CanWrite) si.SetValue(c, sortIndex);
        return c;
    }

    private static void SetMember(Type t, string val, Action<object> set)
    {
        if (t == typeof(string)) set(val);
        else if (t == typeof(int)) set(int.TryParse(val, out var i) ? i : 0);
        else if (t == typeof(bool)) set(val == "1" || string.Equals(val, "true", StringComparison.OrdinalIgnoreCase));
        // other types left at default
    }

    private static CardMaster NewCardMaster(List<CardCSVData> rows)
    {
        var ctor = typeof(CardMaster).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(List<CardCSVData>) }, null);
        if (ctor == null) throw new InvalidOperationException("CardMaster(List<CardCSVData>) ctor not found");
        return (CardMaster)ctor.Invoke(new object[] { rows });
    }

    private static void InjectAsDefault(CardMaster cm)
    {
        var idType = typeof(CardMaster).GetNestedType("CardMasterId");
        var defaultId = Enum.Parse(idType, "Default");
        var dictType = typeof(Dictionary<,>).MakeGenericType(idType, typeof(CardMaster));
        var dict = (IDictionary)Activator.CreateInstance(dictType);
        dict[defaultId] = cm;
        var fld = typeof(CardMaster).GetField("_dictCardMaster",
            BindingFlags.Static | BindingFlags.NonPublic);
        fld.SetValue(null, dict);
    }

    // --- Master reference data (all 8 classes) -------------------------------------------------------

    // Transcribed from HeadlessMasterData.Install. Production difference (2): install ALL 8 classes.
    private static void InstallMaster()
    {
        var master = (Master)FormatterServices.GetUninitializedObject(typeof(Master));
        EnsureEmptyCollections(master);
        var list = new List<ClassCharacterMasterData>();
        for (int c = 1; c <= 8; c++)
            list.Add(NewChara(c, c)); // charaId == classId for class c
        SetMember(master, "ClassCharacterList", list);
        Data.Master = master;
    }

    private static void EnsureEmptyCollections(object obj)
    {
        const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        foreach (var f in obj.GetType().GetFields(bf))
        {
            if (f.GetValue(obj) != null) continue;
            var empty = EmptyOf(f.FieldType);
            if (empty != null) f.SetValue(obj, empty);
        }
    }

    private static object EmptyOf(Type t)
    {
        if (t.IsArray) return Array.CreateInstance(t.GetElementType(), 0);
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition();
            if (def == typeof(List<>) || def == typeof(Dictionary<,>) ||
                def == typeof(HashSet<>) || def == typeof(IList<>) ||
                def == typeof(IDictionary<,>) || def == typeof(ICollection<>) ||
                def == typeof(IEnumerable<>))
            {
                var concrete = def == typeof(List<>) || def == typeof(IList<>) ||
                               def == typeof(ICollection<>) || def == typeof(IEnumerable<>)
                    ? typeof(List<>).MakeGenericType(t.GetGenericArguments())
                    : def == typeof(HashSet<>)
                        ? typeof(HashSet<>).MakeGenericType(t.GetGenericArguments())
                        : typeof(Dictionary<,>).MakeGenericType(t.GetGenericArguments());
                return Activator.CreateInstance(concrete);
            }
        }
        return null;
    }

    private static ClassCharacterMasterData NewChara(int charaId, int classId)
    {
        var c = (ClassCharacterMasterData)FormatterServices.GetUninitializedObject(typeof(ClassCharacterMasterData));
        SetMember(c, "chara_id", charaId);
        SetMember(c, "class_id", classId);
        SetMember(c, "skin_id", charaId);
        SetMember(c, "is_usable", true);
        return c;
    }

    // --- reflection helpers (transcribed from the test fixtures) --------------------------------------

    // Set a member (auto-property backing field or field) by name, tolerating private setters.
    private static void SetMember(object obj, string name, object value)
    {
        var t = obj.GetType();
        const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var p = t.GetProperty(name, bf);
        if (p != null && p.SetMethod != null) { p.SetValue(obj, value); return; }
        var f = t.GetField(name, bf)
                ?? t.GetField($"<{name}>k__BackingField", bf);
        if (f != null) { f.SetValue(obj, value); return; }
        throw new InvalidOperationException($"{t.Name} has no settable member '{name}'");
    }

    // Idempotent backing-field set: only writes when the field is currently 0 (int) or null.
    private static void SetFieldIfZeroOrNull(object obj, string name, object value)
    {
        var f = obj.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new InvalidOperationException($"{obj.GetType().Name} has no field '{name}'");
        var cur = f.GetValue(obj);
        if (cur is null || (cur is int i && i == 0))
            f.SetValue(obj, value);
    }
}
