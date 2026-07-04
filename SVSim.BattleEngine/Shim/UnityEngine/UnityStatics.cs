// AUTHORED SHIM (not copied). UnityEngine static-class surface (prefs, physics, IMGUI,
// device info, rendering) referenced by tangentially-copied engine code (settings,
// post-processing, raycast UI, device fingerprinting). All cosmetic / off the battle
// path; every member is a no-op returning a safe default.
using System;

namespace UnityEngine
{
    public enum CursorLockMode { None}
    public static class GUIUtility { public static string systemCopyBuffer { get; set; } }
    public static class Cursor
    {
        public static CursorLockMode lockState { get; set; }
        public static bool visible { get; set; }
    }
    public static class RenderSettings { public static bool fog { get; set; } }
    public static class Social
    {
        public static void ReportProgress(string achievementID, double progress, Action<bool> callback) { }
        public static void ShowAchievementsUI() { }
    }

    public static class PlayerPrefs
    {
        public static void DeleteKey(string key) { }
        public static int GetInt(string key, int defaultValue = 0) => defaultValue;
        public static string GetString(string key, string defaultValue = "") => defaultValue;
        public static void SetInt(string key, int value) { }
        public static void SetString(string key, string value) { }
    }

    public static class Physics
    {
        public static Vector3 gravity { get; set; }
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = float.PositiveInfinity, int layerMask = -1)
        { hit = default; return false; }
        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance = float.PositiveInfinity, int layerMask = -1)
            => Array.Empty<RaycastHit>();
        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -1)
            => Array.Empty<RaycastHit>();
    }

    public static class SystemInfo
    {
        public static string deviceModel => "";
        public static string deviceUniqueIdentifier => "";
        public static string operatingSystem => "";
        public static string graphicsDeviceName => "";
        public static int systemMemorySize => 0;
        public static int processorCount => 1;
        public static bool SupportsTextureFormat(TextureFormat format) => true;
    }

    public static class QualitySettings
    {
        public static ColorSpace activeColorSpace => ColorSpace.Linear;
        public static int vSyncCount { get; set; }
    }

    public static class StackTraceUtility
    {
        public static string ExtractStackTrace() => "";
    }

    public enum ColorSpace { Linear = 1 }

    public enum TextureFormat
    {
ARGB32, ASTC_6x6, ETC2_RGB, ETC2_RGBA8    }
}

namespace UnityEngine.Experimental.Rendering
{
}
