extern alias engine;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SVSim.BattleEngine.Tests.Coverage;

/// <summary>Harmony-prefix-based method-invocation logger over SVSim.BattleEngine.dll.
/// Idempotent: Install is safe to call from every fixture's [OneTimeSetUp]; only the first
/// call patches.</summary>
public static class CoverageRecorder
{
    private static readonly ConcurrentDictionary<string, byte> _hit = new();
    private static readonly Harmony _h = new("svsim.engine.coverage");
    private static bool _installed;
    private static readonly object _gate = new();

    public static int HitCount => _hit.Count;

    public static void Reset() => _hit.Clear();

    public static void Install()
    {
        lock (_gate)
        {
            if (_installed) return;
            _installed = true;
        }

        var asm = typeof(engine::BattleManagerBase).Assembly;
        var prefix = AccessTools.Method(typeof(CoverageRecorder), nameof(Hit));
        var hm = new HarmonyMethod(prefix);

        foreach (var t in asm.GetTypes())
        {
            var ns = t.FullName ?? string.Empty;
            // Only engine + shim types — skip framework/3rd-party that happens to be co-located.
            if (!ns.StartsWith("SVSim.BattleEngine.") &&
                !ns.StartsWith("Wizard.") &&
                !ns.StartsWith("Cute.") &&
                !ns.StartsWith("Toolbox.") &&
                !ns.StartsWith("UnityEngine.")) continue;
            if (t.IsGenericTypeDefinition) continue;

            var members = t.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.Public   | BindingFlags.NonPublic |
                                       BindingFlags.DeclaredOnly);
            foreach (var m in members)
            {
                if (m.IsAbstract) continue;
                if (m.ContainsGenericParameters) continue;
                if (m.GetMethodBody() is null) continue;
                try { _h.Patch(m, prefix: hm); } catch { /* generic/inlined; skipped */ }
            }
        }
    }

    public static void Dump(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllLines(path, _hit.Keys.OrderBy(k => k));
    }

    private static void Hit(MethodBase __originalMethod)
    {
        var t = __originalMethod.DeclaringType;
        if (t is null) return;
        _hit.TryAdd($"{t.FullName}.{__originalMethod.Name}", 0);
    }
}
