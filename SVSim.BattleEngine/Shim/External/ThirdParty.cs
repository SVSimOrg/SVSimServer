// AUTHORED SHIM (not copied). Third-party / platform SDK surface referenced by
// tangentially-pulled engine files (audio, Steam, networking, serialization).
// Stubbed minimally in their original namespaces; none is on the battle-resolution
// path. Members grow only if the compile loop demands them.
using System;

// ---- remaining UnityEngine types ----
namespace UnityEngine
{
    public partial class Font : Object { }
    public enum Space { Self }
}

namespace UnityEngine.Networking
{
    public partial class UnityWebRequest : IDisposable { public void Dispose() { } }
}

// ---- CRI Atom/Mana audio+movie middleware: see External/CriShim.cs ----

// ---- BestHTTP Socket.IO ----
namespace BestHTTP.SocketIO
{
}

// ---- Google Play Games ----
namespace GooglePlayGames.BasicApi.Events
{
}
