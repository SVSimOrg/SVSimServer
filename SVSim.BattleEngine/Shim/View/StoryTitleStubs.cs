// AUTHORED SHIM (not copied). Final M1 residual: non-battle Story chapter-selection /
// Title / Friend / RoomMatch types swept into the closure but never driven headless,
// plus the namespace anchors their `using` directives target. Empty stubs (no copied
// type inherits them); members would unmask only if a battle path touched them.

namespace Wizard.Story
{
    public enum StoryEntranceType { None, LimitedStory, AllStory }
}

namespace Wizard.Story.ChapterSelection
{
}

namespace Wizard.UIFriend
{
}
namespace Wizard.Title { }
namespace Wizard.RoomMatch { }

// ---- namespace anchors (referenced via `using`) ----
namespace Wizard.Scripts.Network.Task.ItemAcquireHistory { }
namespace Wizard.UI.Profile { }
namespace Wizard.UI.ReportToManagement { }
namespace Wizard.Battle.Tutorial { }
