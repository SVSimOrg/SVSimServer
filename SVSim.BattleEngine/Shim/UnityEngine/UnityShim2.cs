// AUTHORED SHIM (not copied). Second UnityEngine batch surfaced by the M1 compile
// loop (wave after the 2,570-file copy closure). Same rules as UnityShim.cs: no-op
// presentation/IO surface; add only what the compiler demands. Asset/light/collider
// types are referenced only as field/parameter types or via suppressed-IO calls.
using System;

namespace UnityEngine
{
    public class TextAsset : Object
    {
    }

    public class AsyncOperation
    {
        public bool isDone => true;
        public float progress => 1f;
        public int priority { get; set; }
    }

    public class AssetBundle : Object
    {
        public static AssetBundle LoadFromFile(string path) => null;
        public static AssetBundleCreateRequest LoadFromFileAsync(string path) => null;
        public string[] GetAllAssetNames() => new string[0];
        public AssetBundleRequest LoadAllAssetsAsync() => null;
        public void Unload(bool unloadAllLoadedObjects) { }
    }

    public class AssetBundleCreateRequest : AsyncOperation { public AssetBundle assetBundle => null; }
    public class AssetBundleRequest : AsyncOperation { public Object[] allAssets => new Object[0]; }

    public class Collider2D : Component { public bool enabled { get; set; } }
    public partial class BoxCollider2D : Collider2D { public Vector2 offset { get; set; } public Vector2 size { get; set; } }
    public partial class Light : Behaviour { }

    public enum RuntimeInitializeLoadType
    {
    }

    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType) { }
    }
}

// Sub-namespace anchors (referenced via `using`; types unmask in later waves if used).
namespace UnityEngine.Experimental { }
namespace UnityEngine.Experimental.Rendering { }
namespace UnityEngine.SceneManagement
{
    public static class SceneManager
    {
        public static void LoadScene(string sceneName) { }
    }
}
namespace UnityEngine.SocialPlatforms
{
    // NOTE: no IAchievementCallback here -- the engine's AchievementImpl uses
    // Cute.IAchievementCallback; adding one here makes the unqualified name ambiguous.
    public interface IAchievement { }
    public interface IAchievementDescription { }
}
