// AUTHORED SHIM (not copied). Input / Random / Resources statics + the full KeyCode
// enum. Referenced almost entirely by non-battle UI/input files swept into the closure;
// headless we never pump input and never load resources, so all of this is inert.
using System;

namespace UnityEngine
{
    public static partial class Input
    {
        public static Vector3 mousePosition => Vector3.zero;
        public static int touchCount => 0;
        public static bool GetMouseButton(int b) => false;
        public static bool GetMouseButtonDown(int b) => false;
        public static bool GetKey(KeyCode k) => false;
        public static bool GetKeyDown(KeyCode k) => false;
        public static bool GetKeyUp(KeyCode k) => false;
        public static string inputString => "";
        public static bool multiTouchEnabled { get; set; }
    }

    public static class Random
    {
        public static float value => 0f;
        public static int Range(int minInclusive, int maxExclusive) => minInclusive;
        public static float Range(float min, float max) => min;
        public static Quaternion rotation => Quaternion.identity;
        public static int seed { get; set; }
    }

    public static partial class Resources
    {
        public static T Load<T>(string path) where T : Object => null;
        public static T Load<T>(string path, Type t) where T : Object => null;
        // Headless: the non-generic Load is used by the copied PrefabMgr to back a prefab
        // dictionary that the resolution-path ctor then Instantiate()s + GetComponent()s
        // (e.g. Prefab/Game/UnityEventAgent). Return a cached no-op GameObject per path so that
        // chain yields a non-null object. Typed asset loads go through the generic Load<T> (null).
        // ConcurrentDictionary + GetOrAdd so concurrent battle setups don't race on first-miss.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, GameObject> _loaded
            = new System.Collections.Concurrent.ConcurrentDictionary<string, GameObject>();
        public static Object Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return _loaded.GetOrAdd(path, static p => new GameObject(p));
        }
        public static Object Load(string path, Type t) => Load(path);
        public static ResourceRequest LoadAsync(string path) => null;
        public static AsyncOperation UnloadUnusedAssets() => null;
    }

    public class ResourceRequest : AsyncOperation { }

    public enum KeyCode
    {
        None = 0, Tab = 9, Return = 13, Escape = 27,         Alpha0 = 48,         A = 97, B, D, E, F, N, P, S, X, RightArrow = 275, LeftArrow = 276,         RightShift = 303, LeftShift = 304, RightControl = 305, LeftControl = 306,         Mouse0 = 323,         JoystickButton0 = 330, JoystickButton1    }
}
