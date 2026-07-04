// AUTHORED SHIM (not copied). Final loose ends for the M1 compile: (1) namespace
// "anchors" -- empty `using` targets in tangentially-copied files reference these
// namespaces, which must merely exist; a single internal anchor type declares them.
// (2) a few concrete tangential types referenced directly. (3) minimal third-party
// serialization/SDK surface. None is on the battle-resolution path.

namespace Wizard.AutoTest { }
namespace Wizard.Title { }
namespace Wizard.ErrorDialog { }
namespace Wizard.Bingo { }
namespace Wizard.Scripts.Network.Data.TaskData.BuildDeckPurchase { }
namespace Wizard.Scripts.Network.Data.TaskData.ItemPurchase { }
namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase { }
namespace Wizard.Scripts.Network.Data.TaskData.SpotCardExchange { }

// These are NAMESPACES (used as `using` targets in copied files), not types.
namespace Wizard.DeckSelect.FirstDisplayPageIndexGetter { }
namespace Wizary.StorySelectionWorld { }
namespace Wizard.Scripts.Network.Data.TableData.Arena.TwoPick { }

// IManager: a Cute manager interface implemented by NetworkManager/ResourcesManager.
namespace Cute { public interface IManager { } }

// ---- third-party serialization / SDK (minimal surface) ----
namespace MessagePack
{
    public static class MessagePackSerializer
    {
        public static string ToJson(byte[] bytes) => "";
        public static byte[] FromJson(string json) => new byte[0];
    }
}

namespace MiniJSON
{
    public static class Json
    {
        public static object Deserialize(string json) => null;
    }
}

// AOT P/Invoke callback attribute (IL2CPP) + StandaloneFileBrowser anchor.
namespace AOT
{
}
namespace SFB
{
}

// ---- third-party SDK namespace anchors (referenced via `using`) ----
namespace Facebook { }
namespace Facebook.Unity
{
}
namespace RedShellSDK
{
}
