// AUTHORED SHIM (not copied). No-op UnityEngine surface for headless battle
// resolution. Grows via the M1 compile loop -- add only members the compiler
// demands. State-bearing battle logic lives in Engine/; nothing here computes
// game state (Unity calls are VFX/IO/rendering, suppressed by IsForecast).
using System;
using System.Collections;

namespace UnityEngine
{
    // ---- value types (Vector2/3, Quaternion, Color, Mathf, Debug live in Primitives.cs) ----
    public partial struct Vector4 { public float x, y, z, w; public Vector4(float x, float y, float z, float w){ this.x=x; this.y=y; this.z=z; this.w=w; } }
    public struct Color32 { public byte r, g, b, a; public Color32(byte r, byte g, byte b, byte a){ this.r=r; this.g=g; this.b=b; this.a=a; } }
    public struct Bounds
    {
        public Vector3 center, size;
        public Bounds(Vector3 c, Vector3 s) { center = c; size = s; }
        public Vector3 extents { get => size * 0.5f; set => size = value * 2f; }
        public Vector3 min { get => center - extents; set { } }
        public Vector3 max { get => center + extents; set { } }
        public void Encapsulate(Vector3 p) { }
        public void Encapsulate(Bounds b) { }
    }
    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
        public float xMin { get => x; set { width += x - value; x = value; } }
        public float yMin { get => y; set { height += y - value; y = value; } }
        public float xMax { get => x + width; set => width = value - x; }
        public float yMax { get => y + height; set => height = value - y; }
        public Vector2 position { get => new Vector2(x, y); set { x = value.x; y = value.y; } }
        public Vector2 min { get => new Vector2(xMin, yMin); set { } }
        public Vector2 max { get => new Vector2(xMax, yMax); set { } }
        public static bool operator ==(Rect a, Rect b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
        public static bool operator !=(Rect a, Rect b) => !(a == b);
        public override bool Equals(object o) => o is Rect r && this == r;
        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (width.GetHashCode() << 4) ^ (height.GetHashCode() << 6);
    }
    public struct Matrix4x4 { public static Matrix4x4 identity => new Matrix4x4(); public Vector3 MultiplyPoint3x4(Vector3 p) => p; public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b) => identity; }
    public struct Plane { public Plane(Vector3 normal, Vector3 point) { } public Plane(Vector3 inNormal, float d) { } public Plane(Vector3 a, Vector3 b, Vector3 c) { } public bool Raycast(Ray r, out float enter) { enter = 0; return false; } }
    public struct Ray { public Ray(Vector3 origin, Vector3 dir) { this.origin = origin; this.direction = dir; } public Vector3 origin; public Vector3 direction; public Vector3 GetPoint(float d) => origin; }
    public struct RaycastHit { public Vector3 point; public Vector3 normal; public float distance; public Collider collider; }
    public struct LayerMask { public int value; public static int NameToLayer(string n) => 0; public static implicit operator int(LayerMask m) => m.value; public static implicit operator LayerMask(int v) => new LayerMask { value = v }; }

    // ---- core object model ----
    public class Object
    {
        public string name { get; set; }
        public int GetInstanceID() => 0;
        public override string ToString() => name ?? base.ToString();
        public static void Destroy(Object o) { }
        public static void Destroy(Object o, float t) { }
        public static void DestroyImmediate(Object o) { }
        public static void DestroyImmediate(Object o, bool allowDestroyingAssets) { }
        public static void DontDestroyOnLoad(Object o) { }
        public static T Instantiate<T>(T original) where T : Object => original;
        public static T Instantiate<T>(T original, Transform parent) where T : Object => original;
        public static T Instantiate<T>(T original, Vector3 pos, Quaternion rot) where T : Object => original;
        public static T Instantiate<T>(T original, Vector3 pos, Quaternion rot, Transform parent) where T : Object => original;
        public static Object Instantiate(Object original) => original;
        public static Object FindObjectOfType(System.Type t) => null;
        public static Object[] FindObjectsOfType(System.Type t) => new Object[0];
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public static implicit operator bool(Object o) => !ReferenceEquals(o, null);
        public override bool Equals(object o) => ReferenceEquals(this, o);
        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }

    public class Component : Object
    {
        internal GameObject _go;
        // Self-consistent no-op object graph (M5): a Component belongs to a GameObject, and
        // component.transform == component.gameObject.transform. Lazily materialize a backing
        // GameObject so the unguarded prefab/view touches on the createNullView:false card-creation
        // path (F1) resolve to non-null no-ops, and route GetComponent through that GameObject's
        // cached component model so a chained transform.Find(...).GetComponent<T>() yields the same
        // non-null instances rather than null.
        public virtual GameObject gameObject => _go ??= new GameObject();
        public virtual Transform transform => gameObject.transform;
        public string tag { get; set; }
        public T GetComponent<T>() => gameObject.GetComponent<T>();
        public T GetComponentInChildren<T>() => default;
        public T[] GetComponentsInChildren<T>() => new T[0];
        public T[] GetComponentsInChildren<T>(bool includeInactive) => new T[0];
        public T GetComponentInParent<T>() => default;
        public T[] GetComponents<T>() => new T[0];
        public bool CompareTag(string t) => false;
    }

    public class Behaviour : Component { public bool enabled { get; set; } public bool isActiveAndEnabled { get; } }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) => null;
        public Coroutine StartCoroutine(string methodName) => null;
        public void StopCoroutine(IEnumerator routine) { }
        public void StopCoroutine(Coroutine routine) { }
        public void StopCoroutine(string methodName) { }
        public void StopAllCoroutines() { }
    }

    public partial class Transform : Component, IEnumerable
    {
        public Transform() { }
        internal Transform(GameObject owner) { _go = owner; }
        // A Transform IS its own transform (vs Component.transform => gameObject.transform).
        public override Transform transform => this;
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; } = new Vector3(1, 1, 1);
        public Vector3 localEulerAngles { get; set; }
        public Vector3 eulerAngles { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        // Lazily non-null so `someLabel.transform.parent.gameObject` (unguarded in the NORMAL
        // card-creation path) resolves; settable so real re-parenting still records.
        private Transform _parent;
        public Transform parent { get => _parent ??= new Transform(); set => _parent = value; }
        public int childCount => 0;
        // Return a non-null cached child per name so Find(...).Find(...).GetComponent<UILabel>()
        // chains resolve to no-ops; cached so repeated Find of the same child is stable.
        private System.Collections.Generic.Dictionary<string, Transform> _children;
        public Transform Find(string n)
        {
            _children ??= new System.Collections.Generic.Dictionary<string, Transform>();
            if (!_children.TryGetValue(n ?? "", out var t)) { t = new GameObject(n).transform; _children[n ?? ""] = t; }
            return t;
        }
        public Transform GetChild(int i) => null;
        public void SetParent(Transform p) { }
        public void SetParent(Transform p, bool worldPositionStays) { }
        public void SetSiblingIndex(int i) { }
        public int GetSiblingIndex() => 0;
        public Vector3 up { get => Vector3.up; set { } }
        public Vector3 TransformPoint(Vector3 p) => p;
        public Vector3 TransformPoint(float x, float y, float z) => new Vector3(x, y, z);
        public Vector3 InverseTransformPoint(Vector3 p) => p;
        public Vector3 TransformDirection(Vector3 d) => d;
        public Vector3 InverseTransformDirection(Vector3 d) => d;
        public void LookAt(Transform t, Vector3 worldUp) { }
        public void LookAt(Vector3 p, Vector3 worldUp) { }
        public Transform Find(string n, bool includeInactive) => Find(n);
        public IEnumerator GetEnumerator() { yield break; }
    }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public GameObject(string name, params Type[] components) { this.name = name; }
        private Transform _transform;
        public Transform transform => _transform ??= new Transform(this);
        public GameObject gameObject => this;
        public bool activeSelf => false;
        public bool activeInHierarchy => false;
        public int layer { get; set; }
        public string tag { get; set; }
        public void SetActive(bool value) { }

        // Headless component model: the resolution-path ctor (and copied views) acquire components
        // off prefab GameObjects and use them unguarded (F1). Lazily create + cache a no-op instance
        // per concrete Component-derived type so those touches resolve harmlessly instead of NRE.
        // Non-Component T or abstract/uninstantiable T still returns default (null).
        // ConcurrentDictionary because Resources.Load returns SHARED prefab GameObjects across
        // concurrent battle setups, so two engines' Setup() may race on the same _components map.
        private System.Collections.Concurrent.ConcurrentDictionary<Type, object> _components;
        private object GetOrAddComponent(Type t)
        {
            if (t == null || t.IsAbstract || !typeof(Component).IsAssignableFrom(t)) return null;
            var map = _components;
            if (map == null)
            {
                var fresh = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();
                map = System.Threading.Interlocked.CompareExchange(ref _components, fresh, null) ?? fresh;
            }
            // GetOrAdd may invoke the factory more than once under contention; only one result wins.
            // Safe here: the discarded instance has its _go set to `this` (private write to a soon-
            // unreachable object) and WireComponentFields only assigns to the new tree's own private
            // fields. The shared _noopViewMaterial sentinel below is read-only. No global state leaks.
            return map.GetOrAdd(t, ty =>
            {
                object inst;
                try { inst = Activator.CreateInstance(ty); }
                catch { return null; }
                if (inst is Component comp) comp._go = this;
                WireComponentFields(inst);
                return inst;
            });
        }
        // The createNullView:false card-creation path reads many view-leaf reference fields off a
        // CardTemplate component (UILabel/MeshRenderer/Transform/GameObject) UNGUARDED, plus the
        // copied NGUI cosmetic helpers (CardTemplate.SetNumberLabelStyle -> UIBase_CardManager ->
        // UIFont.material / UILabel.material) read material backing fields. The real engine wires all
        // of this from the prefab in SBattleLoad.CreateUnitCardTemplate, which we skip headless. Fill
        // any null GameObject/Component-derived view field with a no-op instance. Pure no-ops:
        // nothing here computes game state (the token's authoritative stats come from CardCSVData).
        internal const System.Reflection.BindingFlags WireFlags =
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance;
        private static readonly Material _noopViewMaterial = new Material { name = "ShimNoOpMaterial" };
        // Create a no-op view-leaf instance and pre-set the NGUI material backing fields the copied
        // UIFont.material / UILabel.material getters read so they return non-null. Only mMat/mMaterial
        // are filled: blanket-filling mReplacement/mAtlas/mDynamicFont would make those getters
        // DELEGATE down a chain that re-nulls. One level deep — no recursion into the created leaf.
        private static object Materialize(Type t)
        {
            var o = Activator.CreateInstance(t);
            SetBackingMaterial(o, "mMat");      // UIFont
            SetBackingMaterial(o, "mMaterial"); // UILabel / UIWidget
            return o;
        }
        private static void SetBackingMaterial(object o, string field)
        {
            var f = o.GetType().GetField(field, WireFlags);
            if (f != null && f.FieldType == typeof(Material) && f.GetValue(o) == null)
                f.SetValue(o, _noopViewMaterial);
        }
        internal static void WireComponentFields(object inst)
        {
            foreach (var f in inst.GetType().GetFields(WireFlags))
            {
                if (f.GetValue(inst) != null) continue;
                var ft = f.FieldType;
                if (ft == typeof(GameObject) || (typeof(Component).IsAssignableFrom(ft) && !ft.IsAbstract))
                { try { f.SetValue(inst, Materialize(ft)); } catch { } }
                else if (ft == typeof(Material))
                { f.SetValue(inst, _noopViewMaterial); }
            }
        }
        public T GetComponent<T>() => (T)(GetOrAddComponent(typeof(T)) ?? default(T));
        public T GetComponentInChildren<T>() => default;
        public T GetComponentInChildren<T>(bool includeInactive) => default;
        public T[] GetComponentsInChildren<T>() => new T[0];
        public T[] GetComponentsInChildren<T>(bool includeInactive) => new T[0];
        public T GetComponentInParent<T>(bool includeInactive) => default;
        public T[] GetComponents<T>() => new T[0];
        public T AddComponent<T>() where T : Component => (T)(GetOrAddComponent(typeof(T)) ?? default(T));
        public void SendMessage(string method, object value, SendMessageOptions options) { }
        public static GameObject Find(string n) => null;
    }

    // Factory for no-op view/manager objects that are NOT acquired via GameObject.GetComponent (e.g.
    // UIManager.getUIBase_CardManager()). Creates the instance and runs the same field-wiring the
    // component model applies, so the copied cosmetic helpers it exposes resolve headless.
    public static class ShimView
    {
        public static T Create<T>() where T : class
        {
            var o = System.Activator.CreateInstance(typeof(T));
            GameObject.WireComponentFields(o);
            return (T)o;
        }
    }

    // ---- rendering / physics / audio (pure no-op presentation) ----
    public class Renderer : Component
    {
        public Material material { get; set; }
        public Material[] materials { get; set; }
        public Material sharedMaterial { get; set; }
        public Material[] sharedMaterials { get; set; }
        public int sortingOrder { get; set; }
    }
    public class MeshRenderer : Renderer { }
    public class SpriteRenderer : Renderer { public Color color { get; set; } }
    public class MeshFilter : Component { public Mesh sharedMesh { get; set; } }
    public class ParticleSystem : Component
    {
public int particleCount => 0;
        public MainModule main => default;
        public int GetParticles(Particle[] p) => 0;
        public void SetParticles(Particle[] p, int n) { }
        public struct MainModule { public MinMaxGradient startColor; }
        public struct MinMaxGradient { public Color color; public MinMaxGradient(Color c) { color = c; } public static implicit operator MinMaxGradient(Color c) => new MinMaxGradient(c); }
        public struct EmissionModule { }
        public struct Particle { public Vector3 position; public Color32 startColor; }
    }
    public class ParticleSystemRenderer : Renderer { }
    public partial class LODGroup : Component { }
    public class Collider : Component { public bool enabled { get; set; } }
    public class BoxCollider : Collider { public Vector3 size { get; set; } public Vector3 center { get; set; } }
    public partial class Rigidbody : Component { }
    public partial class Material : Object
    {
        public Material() { }
        public Material(Material src) { }
        public Material(Shader shader) { }
        public Color color { get; set; }
        public Shader shader { get; set; }
        public Texture mainTexture { get; set; }
        public Vector2 mainTextureOffset { get; set; }
        public Vector2 mainTextureScale { get; set; }
        public Color GetColor(string n) => Color.white;
        public void SetFloat(string n, float v) { }
        public void SetInt(string n, int v) { }
        public void SetColor(string n, Color c) { }
        public void SetTexture(string n, Texture t) { }
    }
    public partial class Mesh : Object { }
    public class Texture : Object { public int width => 0; public int height => 0; }
    public partial class Texture2D : Texture { public Texture2D(int w, int h) { } public Texture2D(int w, int h, TextureFormat format, bool mipChain) { } public void Apply() { } public void SetPixel(int x, int y, Color c) { } }
    public enum WrapMode { Once = 1, Default = 0}
    public struct Keyframe { public float time; public float value; public float inTangent; public float outTangent; public Keyframe(float t, float v) { time = t; value = v; inTangent = 0; outTangent = 0; } public Keyframe(float t, float v, float inT, float outT) { time = t; value = v; inTangent = inT; outTangent = outT; } }
    public struct AnimatorStateInfo { public bool IsName(string name) => false; public float normalizedTime => 0f; public int fullPathHash => 0; public float length => 0f; }
    public partial class Sprite : Object { public Rect rect => default; public Texture2D texture => null; }
    public partial class Shader : Object { public static Shader Find(string n) => null; }
    public partial class Animation : Component, IEnumerable { public bool isPlaying => false; public void Play() { } public void Play(string n) { } public IEnumerator GetEnumerator() { yield break; } }
    public class Animator : Component
    {
public void Play(string n, int layer, float normalizedTime) { }
public void Play(int hash, int layer, float normalizedTime) { }
        public float speed { get; set; } public void Update(float dt) { }
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer) => default;
    }
    public class AnimationCurve { public AnimationCurve() { } public AnimationCurve(params Keyframe[] keys) { } public float Evaluate(float t) => 0f; public int length => 0; public Keyframe[] keys { get; set; } public WrapMode preWrapMode { get; set; } public WrapMode postWrapMode { get; set; } }
    public class AudioClip : Object { public float length => 0f; }
    public partial class AudioSource : Component { public AudioClip clip { get; set; } public float volume { get; set; } }
    public partial class Camera : Component
    {
        public static Camera main => null;
        public static int allCamerasCount => 0;
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public float fieldOfView { get; set; }
        public float orthographicSize { get; set; }
        public bool orthographic { get; set; }
        public float aspect { get; set; }
        public float depth { get; set; }
        public int cullingMask { get; set; }
        public int pixelHeight => 1080;
        public Rect rect { get; set; }
        public Rect pixelRect { get; set; }
        public Vector3 ViewportToWorldPoint(Vector3 p) => p;
        public Vector3 WorldToViewportPoint(Vector3 p) => p;
        public Vector3 ScreenToWorldPoint(Vector3 p) => p;
        public Vector3 WorldToScreenPoint(Vector3 p) => p;
        public Ray ScreenPointToRay(Vector3 p) => default;
    }

    // ---- coroutine machinery (never pumped headless; types must exist) ----
    public class YieldInstruction { }
    public sealed class Coroutine : YieldInstruction { }
    public sealed class WaitForSeconds : YieldInstruction { public WaitForSeconds(float s) { } }
    public sealed class WaitForFixedUpdate : YieldInstruction { }

    // ---- enums (grow members as the compiler demands) ----
    public enum FontStyle { Normal}
    public enum SendMessageOptions { DontRequireReceiver }

    // ---- attributes: permissive ctors accept any compile-time attribute args ----
    public class SerializeField : Attribute { }
    public class HideInInspector : Attribute { }
    public class ExecuteInEditMode : Attribute { }
    public class AddComponentMenu : Attribute { public AddComponentMenu(string n) { } public AddComponentMenu(string n, int o) { } }
    public class ContextMenu : Attribute { public ContextMenu(string n) { } }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponent : Attribute { public RequireComponent(Type a) { } public RequireComponent(Type a, Type b) { } public RequireComponent(Type a, Type b, Type c) { } }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string h) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }

    // ---- subsystem singletons / statics ----
    public static partial class Application
    {
        public static bool isEditor => false;
        public static bool isPlaying => true;
        public static string persistentDataPath => "";
        public static string version => "1.0";
        public static int targetFrameRate { get; set; }
        public static RuntimePlatform platform => RuntimePlatform.WindowsPlayer;
    }
    public enum RuntimePlatform { WindowsPlayer, OSXPlayer, IPhonePlayer, Android, OSXEditor, BlackBerryPlayer}
    public static partial class Time
    {
        public static float deltaTime => 0f;
        public static float time => 0f;
        public static float unscaledTime => 0f;
        public static float unscaledDeltaTime => 0f;
        public static float realtimeSinceStartup => 0f;
        public static int frameCount => 0;
    }
    public static partial class Screen
    {
        public static int width => 1920;
        public static int height => 1080;
        public static float dpi => 96f;
        public static bool fullScreen { get; set; }
    }
}
