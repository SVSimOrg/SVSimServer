// AUTHORED SHIM (not copied). Third-party SDK surface swept into the copy closure by
// non-battle files (audio/movie/anti-cheat/analytics/spine). None is on the battle-
// resolution path. Namespaces must merely exist (anchors); the few types referenced
// by member get a minimal no-op surface. Members grow only as the compile loop demands.

// ---- CodeStage anti-cheat obscured prefs (static k/v facade; no persistence headless) ----
namespace CodeStage.AntiCheat.ObscuredTypes
{
    public static class ObscuredPrefs
    {
        public static int GetInt(string key, int defaultValue = 0) => defaultValue;
        public static string GetString(string key, string defaultValue = "") => defaultValue;
        public static void SetInt(string key, int value) { }
        public static void SetString(string key, string value) { }
    }
}

// ---- Spine animation ----
namespace Spine
{
}
namespace Spine.Unity
{
}

// ---- misc third-party namespaces (anchors) ----
namespace RedShellUnity { }
namespace PlatformSupport.Collections.ObjectModel
{
}
namespace Convention { }
namespace com.adjust.sdk
{
}
namespace BestHTTP.Decompression { }
namespace BestHTTP.SocketIO.Transports { }

namespace BestHTTP.Decompression.Zlib
{
}

// Native plugins (no decomp source) referenced unqualified from global scope.
public static class TimeNativePlugin { public static float GetDeviceOperatingTime() => 0f; }

// The BCL's CollectionExtensions.GetValueOrDefault only binds to IReadOnlyDictionary;
// copied code calls it on an IDictionary<,> static type (where the only by-name match is
// the copied JsonDataExtension.GetValueOrDefault(JsonData,...) — wrong receiver). Supply the
// IDictionary form globally so the call resolves.
public static class ShimDictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this System.Collections.Generic.IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        => dict != null && dict.TryGetValue(key, out var v) ? v : defaultValue;
}
