// AUTHORED SHIM (not copied). No-op member extensions to the UnityEngine value/component
// shims, added as the M1 compile loop surfaced specific calls from copied engine/view
// code. Signatures mirror the real UnityEngine API at the call sites (arg counts/types
// taken from the decomp). None executes headless — all return safe defaults.
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public partial struct Vector4
    {
        public static Vector4 zero => default;
        public static Vector4 operator *(Vector4 a, float s) => new Vector4(a.x * s, a.y * s, a.z * s, a.w * s);
        public static Vector4 operator *(float s, Vector4 a) => new Vector4(a.x * s, a.y * s, a.z * s, a.w * s);
        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        public static bool operator ==(Vector4 a, Vector4 b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);
        public override bool Equals(object o) => o is Vector4 v && this == v;
        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() << 4) ^ (w.GetHashCode() << 6);
        public Vector4(float x, float y, float z) { this.x = x; this.y = y; this.z = z; this.w = 0f; }
        public Vector4(float x, float y) { this.x = x; this.y = y; this.z = 0f; this.w = 0f; }
        // UnityEngine implicitly promotes/truncates between Vector2/3/4.
        public static implicit operator Vector4(Vector3 v) => new Vector4(v.x, v.y, v.z, 0f);
        public static implicit operator Vector3(Vector4 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator Vector4(Vector2 v) => new Vector4(v.x, v.y, 0f, 0f);
        public static implicit operator Vector2(Vector4 v) => new Vector2(v.x, v.y);
    }

    public partial class Transform
    {
        public Matrix4x4 worldToLocalMatrix => default;
        public void Translate(Vector3 translation, Space relativeTo) { }
        public void Rotate(Vector3 eulers, Space relativeTo) { }
    }

    public partial class LODGroup
    {
        public bool enabled { get; set; }
        public LOD[] GetLODs() => Array.Empty<LOD>();
    }

    public struct LOD
    {
        public float screenRelativeTransitionHeight;
        public Renderer[] renderers;
        public LOD(float height, Renderer[] rends) { screenRelativeTransitionHeight = height; renderers = rends; }
    }

    public partial class Rigidbody
    {
        public void MovePosition(Vector3 position) { }
        public void MoveRotation(Quaternion rotation) { }
    }

    public partial class Material
    {
    }

    public partial class Mesh
    {
    }

    public partial class Texture2D
    {
    }

    public partial class Sprite
    {
        public Rect textureRect => default;
        public Vector2 textureRectOffset => default;
    }

    public partial class Shader
    {
        public static int PropertyToID(string name) => 0;
        public static void SetGlobalColor(string name, Color value) { }
    }

    public partial class Animation
    {
        public bool enabled { get; set; }
        public bool IsPlaying(string name) => false;
        public void Sample() { }
    }

    public partial class AudioSource
    {
        public float pitch { get; set; }
        public bool playOnAwake { get; set; }
        public int priority { get; set; }
        public void PlayOneShot(AudioClip clip) { }
    }

    public partial class Camera
    {
        public bool enabled { get; set; }
        public static int GetAllCameras(Camera[] cameras) => 0;
    }

    public partial class Font
    {
        public string[] fontNames { get; set; }
        public Material material { get; set; }
    }

    public partial class Light
    {
        public Color color { get; set; }
    }

    public partial class Time
    {
        public static float smoothDeltaTime => 0f;
    }

    public partial class Application
    {
        public static NetworkReachability internetReachability => NetworkReachability.NotReachable;
    }

    public enum NetworkReachability { NotReachable}

    public partial class Screen
    {
        public static int sleepTimeout { get; set; }
    }

    public enum IMECompositionMode { Auto, On}

    public partial class Input
    {
        public static Vector2 compositionCursorPos { get; set; }
        public static string compositionString => "";
        public static IMECompositionMode imeCompositionMode { get; set; }
    }

    public partial class Resources
    {
        public static Object[] FindObjectsOfTypeAll(Type type) => Array.Empty<Object>();
    }
}

namespace UnityEngine.Networking
{
    public partial class UnityWebRequest
    {
        public UnityWebRequest() { }
        public UnityWebRequest(string url, string method) { }
        public static UnityWebRequest Get(string uri) => new UnityWebRequest();
        public UnityWebRequestAsyncOperation SendWebRequest() => new UnityWebRequestAsyncOperation();
        public void SetRequestHeader(string name, string value) { }
        public Dictionary<string, string> GetResponseHeaders() => new Dictionary<string, string>();
        public bool isDone => true;
        public float downloadProgress => 1f;
        public long responseCode => 200;
        public string error => null;
        public DownloadHandler downloadHandler { get; set; }
        public UploadHandler uploadHandler { get; set; }
    }

    public class DownloadHandler { public byte[] data => Array.Empty<byte>(); public string text => ""; }
    public class UploadHandler { }
    public class UploadHandlerRaw : UploadHandler { public UploadHandlerRaw(byte[] data) { } }
    public class DownloadHandlerBuffer : DownloadHandler { }
    public class UnityWebRequestAsyncOperation : AsyncOperation { }
}

namespace UnityEngine
{
    // ---- additional off-battle-path Unity type stubs (CS0246 closure) ----
    public sealed class WaitForSecondsRealtime : YieldInstruction { public WaitForSecondsRealtime(float time) { } }
    public class AnimationState { public string name { get; set; } public float speed { get; set; } public float time { get; set; } public float length => 0f; }
}
